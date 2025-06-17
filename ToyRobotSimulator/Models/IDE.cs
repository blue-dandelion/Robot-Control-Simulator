using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ToyRobotSimulator.Models;

public class IDE
{
    public static readonly HashSet<string> commands = new()
    {
        "PLACE", "MOVE", "LEFT", "RIGHT", "REPORT"
    };

    public static event EventHandler<MessageEventArgs>? eh_Warning;
    public static event EventHandler<MessageEventArgs>? eh_Error;

    #region Check string & tokens
    public static bool IsEnd(char ch)
    {
        return char.IsWhiteSpace(ch);
    }

    public static bool IsCommand(string code)
    {
        return commands.Contains(code);
    }

    public static bool IsDirection(string code)
    {
        return Enum.TryParse(code, false, out Direction direction);
    }

    public static bool IsInt(string code)
    {
        return int.TryParse(code, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out int _);
    }
    #endregion

    #region Functions
    public static List<TokenLine> Tokenize(string code)
    {
        List<TokenLine> tokens = new();

        // Seperate code into lines
        string[] codeLines = code.Split('\n');

        for(int i = 0; i < codeLines.Length; i++)
        {
            string codeLine = codeLines[i];
            TokenLine tokenLine = new TokenLine();
            tokenLine.lineId = i;

            for (int j = 0; j < codeLine.Length; j++)
            {
                var ch = codeLine[j];

                // Skip End character
                if (IsEnd(ch))
                {
                    continue;
                }

                // Get all the characters until end
                var buffer = ch.ToString();
                while (j + 1 < codeLine.Length && !IsEnd(codeLine[j + 1]))
                {
                    buffer += codeLine[++j];
                }

                // Add this token to the tokenline
                tokenLine.tokens.Add(buffer);
            }

            tokens.Add(tokenLine);
        }

        return tokens;
    }

    public static bool GrammarCheck(List<TokenLine> tokenLines)
    {
        bool codeStart = false;

        int lineId = 0;

        for (int i = 0; i < tokenLines.Count; i++)
        {
            lineId++;

            List<string> tokens = tokenLines[i].tokens;

            for (int j = 0; j < tokens.Count; j++)
            {
                // Make sure the token is valid
                if (IsCommand(tokens[j]) || IsInt(tokens[j]) || IsDirection(tokens[j]))
                {
                    // Make sure the "PLACE" token is followed by necessary infos
                    if (tokens[j] == "PLACE")
                    {
                        if (!codeStart) codeStart = true;

                        if (j + 1 < tokens.Count)
                        {
                            string[] placeInfo = tokens[++j].Split(',');

                            if (placeInfo.Length == 3)
                            {
                                if (!IsInt(placeInfo[0]) || !IsInt(placeInfo[1]) || !IsDirection(placeInfo[2]))
                                {
                                    eh_Error?.Invoke(null, new MessageEventArgs("Invalid info for PLACE", lineId));
                                    return false;
                                }
                            }
                            else
                            {
                                eh_Error?.Invoke(null, new MessageEventArgs("Invalid info for PLACE", lineId));
                                return false;
                            }
                        }
                        else
                        {
                            eh_Error?.Invoke(null, new MessageEventArgs("Invalid info for PLACE", lineId));
                            return false;
                        }
                    }
                    else
                    {
                        // Make sure the commands follow at least one "PLACE"
                        if (!codeStart)
                        {
                            eh_Warning?.Invoke(null, new MessageEventArgs("Command needs to start with PLACE. Otherwise the command will not run.", lineId));
                        }
                    }

                    continue;
                }

                eh_Error?.Invoke(null, new MessageEventArgs("Invalid token.", lineId));
                return false;
            }
        }

        return true;
    }

    #endregion
}

public class KeywordColorizer : DocumentColorizingTransformer
{
    static SolidColorBrush colPLACE = new SolidColorBrush(Color.FromArgb(255, 60, 150, 75));
    static SolidColorBrush colMOVE = new SolidColorBrush(Color.FromArgb(255, 195, 40, 40));
    static SolidColorBrush colROTATE = new SolidColorBrush(Color.FromArgb(255, 40, 150, 210));
    static SolidColorBrush colREPORT = new SolidColorBrush(Color.FromArgb(255, 170, 105, 30));
    public Dictionary<string, SolidColorBrush> keywords = new()
    {
        {"PLACE", colPLACE },
        {"MOVE", colMOVE },
        {"LEFT", colROTATE },
        {"RIGHT", colROTATE },
        {"REPORT", colREPORT },
    };

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.IsDeleted) return;

        string text = CurrentContext.Document.GetText(line);

        foreach (string child in keywords.Keys)
        {
            int index = text.IndexOf(child);
            while (index >= 0)
            {
                ChangeLinePart(
                    line.Offset + index,
                    line.Offset + index + child.Length,
                    element => element.TextRunProperties.SetForegroundBrush(
                        keywords[child]
                    ));

                index = text.IndexOf(child, index + child.Length);
            }
        }
    }
}

public class LineHighlighter : DocumentColorizingTransformer
{
    public int runningLineId { get; set; }
    public List<int> warningLineIds { get; set; } = new();
    public List<int> errorLineIds { get; set; } = new();
    
    static SolidColorBrush colRunningLine = new SolidColorBrush(SignColors.Running, SignColors.lineBackgroundOpacity);
    static SolidColorBrush colWarningLine = new SolidColorBrush(SignColors.warning, SignColors.lineBackgroundOpacity);
    static SolidColorBrush colErrorLine = new SolidColorBrush(SignColors.error, SignColors.lineBackgroundOpacity);

    public void ResetHighlight()
    {
        runningLineId = -1;
        warningLineIds.Clear();
        errorLineIds.Clear();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.LineNumber == runningLineId)
        {
            // paint the entire line background
            ChangeLinePart(
                line.Offset,
                line.EndOffset,
                element => element.TextRunProperties.SetBackgroundBrush(colRunningLine)
            );
        }
        else if (warningLineIds.Contains(line.LineNumber))
        {
            // paint the entire line background
            ChangeLinePart(
                line.Offset,
                line.EndOffset,
                element => element.TextRunProperties.SetBackgroundBrush(colWarningLine)
            );
        }
        else if (errorLineIds.Contains(line.LineNumber))
        {
            // paint the entire line background
            ChangeLinePart(
                line.Offset,
                line.EndOffset,
                element => element.TextRunProperties.SetBackgroundBrush(colErrorLine)
            );
        }
    }
}
