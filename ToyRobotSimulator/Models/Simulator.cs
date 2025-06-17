using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyRobotSimulator.Models;

public class Simulator
{
    public static int spaceW = 5;
    public static int spaceH = 5;

    public event EventHandler<MessageEventArgs>? eh_Warning;
    public event EventHandler<MessageEventArgs>? eh_SendMessage;

    public int runningLineId = 0;

    public void Run(string commands)
    {
        // Generate a new virtual robot
        Robot rob = new Robot(spaceW, spaceH);
        rob.eh_Warning += (sender, e) =>
        {
            eh_Warning?.Invoke(this, new MessageEventArgs(e.Message, runningLineId));
        };

        // Tokenize the commands
        List<TokenLine> tokenLines = IDE.Tokenize(commands);

        // Debug the commands
        if (!IDE.GrammarCheck(tokenLines)) return;

        // Run commands
        runningLineId = 0;
        for (int i = 0; i < tokenLines.Count; i++)
        {
            runningLineId++;
            for (int j = 0; j < tokenLines[i].tokens.Count; j++)
            {
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
        }
    }
}
