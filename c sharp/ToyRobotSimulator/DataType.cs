using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyRobotSimulator;

public enum Direction
{
    // Do not change the clockwise order
    NORTH, EAST, SOUTH, WEST
}

public enum HazLev
{
    Normal, Warning, Error
}

public class TokenLine
{
    public int lineId;
    public List<string> tokens = new();
}

public class SignColors
{
    public static Color Running = Color.FromRgb(175, 215, 230);
    public static Color warning = Color.FromRgb(205, 145, 0);
    public static Color error = Color.FromRgb(255, 0, 0);
    public static double lineBackgroundOpacity = 0.5;
}

public class MessageEventArgs: EventArgs
{
    public int RelatedCodeLineId { get; }
    public string Message { get; }

    public MessageEventArgs(string message, int relatedCodeLineId = -1)
    {
        RelatedCodeLineId = relatedCodeLineId;
        Message = message;
    }
}