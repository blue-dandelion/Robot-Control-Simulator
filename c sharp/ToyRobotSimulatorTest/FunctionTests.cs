using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using RobotControlSimulator;
using RobotControlSimulator.Models;
using RobotControlSimulator.Views;
using Xunit.Abstractions;

namespace RobotControlSimulatorTest;

public class FunctionTests
{
    private readonly ITestOutputHelper _output;

    public FunctionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private string code = "PLACE 1,2,EAST\r\nMOVE\r\nMOVE\r\nLEFT\r\nMOVE\r\nREPORT";

    [Fact]
    public void Code_Tokenize_TokenNumber()
    {
        List<TokenLine> tokens = IDE.Tokenize(code);

        string message = string.Empty;
        for (int i = 0; i < tokens.Count; i++) 
        {
            message += tokens[i];
            if (i < tokens.Count - 1) message += " | ";
        }

        _output.WriteLine(message);

        Assert.Equal(5, tokens.Count);
    }

    [Fact]
    public void Code_CheckGrammar_Result()
    {
        List<TokenLine> tokens = IDE.Tokenize(code);

        string message = string.Empty;
        IDE.eh_Warning += (sender, e) => { message += $"Warning: {e.Message}\n"; };
        IDE.eh_Error += (sender, e) => { message += $"Error: {e.Message}\n"; };

        bool result = IDE.GrammarCheck(tokens);

        _output.WriteLine(message);

        Assert.True(result);
    }

    [Fact]
    public void Code_Run_Output()
    {
        //Simulator sim = new Simulator();
        //string message = string.Empty;
        //sim.eh_Falling += (sender, e) => { message += $"Warning: {e.Message}\n"; };
        //sim.eh_SendMessage += (sender, e) => { message += $"Output: {e.Message}\n"; };
        //
        //sim.Run(code, TimeSpan.Zero);
        //
        //_output.WriteLine(message);
        //
        //Assert.Equal("Output: 0,2,EAST\n", message);
    }
}