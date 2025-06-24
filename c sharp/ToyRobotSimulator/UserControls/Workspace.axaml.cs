using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Linq;
using System.Reactive;

namespace ToyRobotSimulator.UserControls;

public partial class Workspace : UserControl
{
    public double unitSize = 50;
    public int width, height;

    public double zoomSpeed = 5;
    public SolidColorBrush robotColor = new() { Color = Colors.LightBlue };
    public SolidColorBrush DangerColor = new() { Color = Colors.Crimson };

    private double originX, originY;
    private TranslateTransform? robotTranslate;
    private RotateTransform? robotRotate;

    private DispatcherTimer? flashTimer;

    public Workspace()
    {
        InitializeComponent();

        Reload(5,5);
    }

    public void Reload(int workspaceW, int workspaceH)
    {
        width = workspaceW; 
        height = workspaceH;

        Resize();
        Reset();

        #region Reference Line
        grd_Ref.ColumnDefinitions = new();
        for (int i = 0; i < workspaceW; i++)
        {
            grd_Ref.ColumnDefinitions.Add(new ColumnDefinition());
        }

        grd_Ref.RowDefinitions = new();
        for (int i = 0; i < workspaceH; i++)
        {
            grd_Ref.RowDefinitions.Add(new RowDefinition());
        }

        grd_Ref.Children.Clear();
        for (int i = 0; i < workspaceW; i++)
        {
            for (int j = 0; j < workspaceH; j++)
            {
                Border bdr = new()
                {
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Colors.Transparent),
                };

                Grid.SetRow(bdr, j);
                Grid.SetColumn(bdr, i);

                grd_Ref.Children.Add(bdr);
            }
        }
        #endregion

        #region Danger
        flashTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        flashTimer.Tick += (sender, e) =>
        {
            if (bdr_InnerRobot.Background == robotColor)
            {
                bdr_InnerRobot.Background = DangerColor;
            }
            else
            {
                bdr_InnerRobot.Background = robotColor;
            }
        };
        #endregion
    }

    public void Reset()
    {
        robotTranslate = new TranslateTransform(originX, originY);
        bdr_Robot.RenderTransform = robotTranslate;
        robotRotate = new RotateTransform(0);
        bdr_InnerRobot.RenderTransform = robotRotate;

        if (flashTimer?.IsEnabled == true)
        {
            Danger(false);
        }
    }

    public void Place(int unitX, int unitY, Direction dir)
    {
        robotTranslate!.X = originX + unitX * unitSize;
        robotTranslate!.Y = originY + unitY * unitSize;
        robotRotate!.Angle = ((int)dir) % 4 * 90;
    }

    public void Move(int unitX, int unitY)
    {
        double curX = robotTranslate!.X;
        double curY = robotTranslate.Y;
        robotTranslate.X += unitX * unitSize;
        robotTranslate.Y += unitY * unitSize;
    }

    public void Rotate(Direction dir)
    {
        robotRotate!.Angle = ((int)dir) % 4 * 90;
    }

    public void Danger(bool status)
    {
        if(status == true)
        {
            flashTimer?.Start();
        }
        else
        {
            flashTimer?.Stop();
            bdr_InnerRobot.Background = robotColor;
        }
    }

    public void ZoomIn()
    {
        unitSize = Math.Min(unitSize + zoomSpeed, 100);
        Resize();
    }

    public void ZoomOut()
    {
        unitSize = Math.Max(unitSize - zoomSpeed, 10);
        Resize();
    }

    private void Resize()
    {
        cav_Workspace.Width = width * unitSize;
        cav_Workspace.Height = height * unitSize;

        originX = 0;
        originY = unitSize * (height - 1);

        if(robotTranslate != null && robotTranslate != null)
        {
            double posX = robotTranslate.X / bdr_Robot.Width;
            double posY = robotTranslate.Y / bdr_Robot.Height;
            robotTranslate.X = posX * unitSize;
            robotTranslate.Y = posY * unitSize;
        }

        bdr_Robot.Width = unitSize;
        bdr_Robot.Height = unitSize;

        bdr_InnerRobot.CornerRadius = new CornerRadius(bdr_InnerRobot.Width / 2, bdr_InnerRobot.Width / 2, 0, 0);
    }
}