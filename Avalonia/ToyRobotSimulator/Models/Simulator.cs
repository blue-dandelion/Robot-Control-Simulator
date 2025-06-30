using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RobotControlSimulator.UserControls;

namespace RobotControlSimulator.Models;

public class Simulator
{
    public int spaceW;
    public int spaceH;

    public event EventHandler<MessageEventArgs>? eh_ChangeRunLine;
    public event EventHandler<MessageEventArgs>? eh_FinishRunning;

    public Robot? rob;

    public bool isRunning = false;

    private int runningLineId;
    private CancellationTokenSource? cts;
    private TaskCompletionSource<bool>? tcs;

    private Workspace workspace;

    public Simulator(Workspace ws)
    {
        workspace = ws;
        Reset();
    }

    public void Reset()
    {
        // Generate a new virtual robot
        rob = new Robot(workspace);
        rob.eh_Falling += (sender, e) =>
        {
            AppConsole.Write(new("The robot falls out of the work space!", runningLineId), HazLev.Warning);
        };
        rob.eh_Report += (sender, e) =>
        {
            if (string.IsNullOrEmpty(rob.message)) return;
            AppConsole.Write(new(rob.message, runningLineId), HazLev.Normal);
        };
    }

    public void Clear()
    {
        workspace.Reset();
        isRunning = false;
        cts?.Cancel();
        cts = new();
        rob?.Reset();
    }
    
    public async void Run(string commands, TimeSpan pause, bool runLine = false)
    {
        cts?.Cancel();
        cts = new();

        // Tokenize the commands
        List<TokenLine> tokenLines = IDE.Tokenize(commands);

        // Debug the commands
        if (!IDE.GrammarCheck(tokenLines))
        {
            StopRunning();
            return;
        }

        // Run commands
        try
        {
            isRunning = true;
            runningLineId = 0;

            for (int i = 0; i < tokenLines.Count; i++)
            {
                runningLineId++;
                eh_ChangeRunLine?.Invoke(this, new(string.Empty, runningLineId));

                cts.Token.ThrowIfCancellationRequested();

                tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                for (int j = 0; j < tokenLines[i].tokens.Count; j++)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    switch (tokenLines[i].tokens[j])
                    {
                        case "PLACE":
                            string[] placeInfo = tokenLines[i].tokens[++j].Split(',');
                            rob?.Place(int.Parse(placeInfo[0]), int.Parse(placeInfo[1]), Enum.Parse<Direction>(placeInfo[2]));
                            break;
                        case "MOVE":
                            rob?.Move();
                            break;
                        case "LEFT":
                            rob?.Rotate("LEFT");
                            break;
                        case "RIGHT":
                            rob?.Rotate("RIGHT");
                            break;
                        case "REPORT":
                            rob?.Report();
                            break;
                        default:
                            break;
                    }
                }

                if (runLine)
                {
                    await tcs.Task;
                }
                else
                {
                    cts.Token.ThrowIfCancellationRequested();
                    await Task.Delay(pause);
                }

            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            isRunning = false;
            eh_FinishRunning?.Invoke(this, new MessageEventArgs("", -1));
        }
    }

    public void StopRunning()
    {
        isRunning = false;
        eh_FinishRunning?.Invoke(this, new MessageEventArgs("", -1));

        cts?.Cancel();
    }

    public void RunNextLine()
    {
        if(tcs is { Task.IsCompleted: false })
        {
            tcs.SetResult(true);
        }
    }

}
