using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyRobotSimulator.Models;

public class Utils
{
    public static Window? GetParentWindow(Control control)
    {
        Visual currentControl = control;
        while (currentControl is not Window)
        {
            currentControl = (currentControl.GetVisualParent())!;
        }

        return currentControl as Window;
    }

    public static TextBox TextBox_Int(TextBox txtbox, int minValue, int maxValue, bool enterToLostFocus = true)
    {
        if (enterToLostFocus)
        {
            txtbox.KeyDown += (object? sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                {
                    GetParentWindow(txtbox)?.Focus();
                    e.Handled = true;
                }
            };
        }

        txtbox.GotFocus += (sender, e) =>
        {
            DataStorage.Instance.txtbox_Buffer = txtbox.Text;
        };

        txtbox.LostFocus += (object? sender, RoutedEventArgs e) =>
        {
            if (int.TryParse(txtbox.Text, out int result))
            {
                result = Math.Clamp(result, minValue, maxValue);
                txtbox.Text = result.ToString();
            }
            else
            {
                txtbox.Text = DataStorage.Instance.txtbox_Buffer;
            }

            DataStorage.Instance.txtbox_Buffer = null;
        };

        return txtbox;
    }

    public static TextBox TextBox_Double(TextBox txtbox, double minValue, double maxValue, bool enterToLostFocus = true)
    {
        if (enterToLostFocus)
        {
            txtbox.KeyDown += (object? sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                {
                    GetParentWindow(txtbox)?.Focus();
                    e.Handled = true;
                }
            };
        }

        txtbox.GotFocus += (sender, e) =>
        {
            DataStorage.Instance.txtbox_Buffer = txtbox.Text;
        };

        txtbox.LostFocus += (object? sender, RoutedEventArgs e) =>
        {
            if (double.TryParse(txtbox.Text, out double result))
            {
                result = Math.Clamp(result, minValue, maxValue);
                txtbox.Text = result.ToString();
            }
            else
            {
                txtbox.Text = DataStorage.Instance.txtbox_Buffer;
            }

            DataStorage.Instance.txtbox_Buffer = null;
        };

        return txtbox;
    }

}

public class AppConsole
{
    public static EventAggregator EA_WriteConsoleMessage = new();

    public AppConsole()
    {
    }

    public static void Write(MessageEventArgs mea, HazLev hazLev)
    {
        EA_WriteConsoleMessage.Publish(new WriteConsoleMessage(mea, hazLev));
    }

}

public class EventAggregator
{
    private readonly IDictionary<Type, List<Action<object>>> subscribers = new Dictionary<Type, List<Action<object>>>();

    public void Subscribe<T>(Action<T> action)
    {
        if (!subscribers.ContainsKey(typeof(T)))
        {
            subscribers[typeof(T)] = new List<Action<object>>();
        }

        subscribers[typeof(T)].Add(x => action((T)x));
    }

    public void Publish<T>(T message)
    {
        if (subscribers.ContainsKey(typeof(T)) && message != null)
        {
            foreach (var subscriber in subscribers[typeof(T)])
            {
                subscriber(message);
            }
        }
    }
}

public class WriteConsoleMessage
{
    public int RelatedCodeLineId;
    public string Message { get; set; }
    public HazLev HazzardLevel { get; set; }

    public WriteConsoleMessage(MessageEventArgs mea, HazLev hazLev)
    {
        HazzardLevel = hazLev;
        RelatedCodeLineId = mea.RelatedCodeLineId;

        string header = string.Empty;
        switch (hazLev)
        {
            case HazLev.Normal:
                header = $"Output{(mea.RelatedCodeLineId == -1 ? "" : $" (line {mea.RelatedCodeLineId})")}: ";
                break;
            case HazLev.Warning:
                header = $"Warning{(mea.RelatedCodeLineId == -1 ? "" : $" (line {mea.RelatedCodeLineId})")}: ";
                break;
            case HazLev.Error:
                header = $"Error{(mea.RelatedCodeLineId == -1 ? "" : $" (line {mea.RelatedCodeLineId})")}: ";
                break;
            default:
                break;
        }

        Message = header + mea.Message;
    }
}

public class OutputKeywordColorizer : DocumentColorizingTransformer
{
    public Dictionary<string, SolidColorBrush> keywords = new()
    {
        {"Warning", new SolidColorBrush(SignColors.warning) },
        {"Error", new SolidColorBrush(SignColors.error) },
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



