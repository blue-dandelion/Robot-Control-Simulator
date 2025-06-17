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

public class TokenLine
{
    public int lineId;
    public List<string> tokens = new();
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