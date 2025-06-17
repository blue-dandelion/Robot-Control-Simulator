using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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

