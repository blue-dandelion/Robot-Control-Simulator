using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using ToyRobotSimulator.Views;
using Xunit;

namespace ToyRobotSimulatorTest;

public class UITests
{
    [AvaloniaFact]
    public void Button_Clicked_EnterCode()
    {
        // Setup Avalonia in Headless Mode in the Test
        var appBuilder = AppBuilder.Configure<ToyRobotSimulator.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .SetupWithoutStarting();

        var window = new MainWindow();
        window.Show();

        var button = window.FindControl<Button>("btn_EnterPLACE");
        var textEditor = window.FindControl<AvaloniaEdit.TextEditor>("aetxtedt_CodeEditor");

        if (button != null && textEditor != null)
        {
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("PLACE 0,0,NORTH\n", textEditor.Text);
        }
    }

    [AvaloniaFact]
    public async void Text_Enter_IntOnly()
    {
        // Setup Avalonia in Headless Mode in the Test
        var appBuilder = AppBuilder.Configure<ToyRobotSimulator.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .SetupWithoutStarting();

        var window = new MainWindow();
        window.Show();

        var textbox = window.FindControl<TextBox>("txtbox_PlaceX");

        if(textbox != null)
        {
            textbox.RaiseEvent(new Avalonia.Input.GotFocusEventArgs
            {
                RoutedEvent = TextBox.GotFocusEvent,
                Source = textbox
            });
            textbox.Text = "\n";
            textbox.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(TextBox.LostFocusEvent));
            Assert.Equal("0", textbox.Text);

            await Dispatcher.UIThread.InvokeAsync(() => textbox.Focus());

            textbox.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.Enter,
                Source = textbox
            });

            Assert.False(textbox.IsFocused);
            Assert.True(window.IsFocused);
        }
    }
}