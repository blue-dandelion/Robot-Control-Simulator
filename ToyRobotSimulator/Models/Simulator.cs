using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToyRobotSimulator.UserControls;

namespace ToyRobotSimulator.Models;

public class Simulator
{
    public int spaceW;
    public int spaceH;

    public event EventHandler<MessageEventArgs>? eh_ChangeRunLine;
    public event EventHandler<MessageEventArgs>? eh_FinishRunning;
    public event EventHandler<MessageEventArgs>? eh_Warning;
    public event EventHandler<MessageEventArgs>? eh_SendMessage;

    public Robot rob;

    public bool isRunning = false;

    private int runningLineId;
    private CancellationTokenSource cts;
    private TaskCompletionSource<bool> tcs;

    public Simulator()
    {

    }

    public void SetVirtualRobot(RobotPreview? prv = null)
    {
        // Generate a new virtual robot
        rob = new Robot(spaceW, spaceH, prv);
        rob.eh_Warning += (sender, e) =>
        {
            eh_Warning?.Invoke(this, new MessageEventArgs(e.Message, runningLineId));
        };
    }

    public async void Run(string commands, TimeSpan pause, bool runLine = false)
    {
        cts?.Cancel();
        cts = new();

        // Tokenize the commands
        List<TokenLine> tokenLines = IDE.Tokenize(commands);

        // Debug the commands
        if (!IDE.GrammarCheck(tokenLines)) return;

        // Run commands
        try
        {
            isRunning = true;
            runningLineId = 0;

            for (int i = 0; i < tokenLines.Count; i++)
            {
                runningLineId++;
                eh_ChangeRunLine?.Invoke(this, new MessageEventArgs("", runningLineId));

                cts.Token.ThrowIfCancellationRequested();

                tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                for (int j = 0; j < tokenLines[i].tokens.Count; j++)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    switch (tokenLines[i].tokens[j])
                    {
                        case "PLACE":
                            string[] placeInfo = tokenLines[i].tokens[++j].Split(',');
                            rob.Place(int.Parse(placeInfo[0]), int.Parse(placeInfo[1]), Enum.Parse<Direction>(placeInfo[2]));
                            break;
                        case "MOVE":
                            rob.Move();
                            break;
                        case "LEFT":
                            rob.Rotate("LEFT");
                            break;
                        case "RIGHT":
                            rob.Rotate("RIGHT");
                            break;
                        case "REPORT":
                            rob.Report();
                            if (!string.IsNullOrEmpty(rob.message)) eh_SendMessage?.Invoke(this, new MessageEventArgs(rob.message, runningLineId));
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
        cts.Cancel();
    }

    public void RunNextLine()
    {
        if(tcs is { Task.IsCompleted: false })
        {
            tcs.SetResult(true);
        }
    }

}
