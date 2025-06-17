using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Linq;
using System.Reactive;

namespace ToyRobotSimulator.UserControls;

public partial class RobotPreview : UserControl
{
    public double unitSize = 50;
    public int workspaceWidthUnit = 5, workspaceHeightUnit = 5;
    public new SolidColorBrush robotColor = new() { Color = Colors.LightBlue };
    public new SolidColorBrush DangerColor = new() { Color = Colors.Crimson };

    private double originX, originY;
    private TranslateTransform robotTranslate;
    private RotateTransform robotRotate;

    private DispatcherTimer flashTimer;

    public RobotPreview()
    {
        InitializeComponent();

        Reload();
    }

    public void Reload()
    {
        #region Resize
        cav_Workspace.Width = workspaceWidthUnit * unitSize;
        cav_Workspace.Height = workspaceHeightUnit * unitSize;
        #endregion

        #region Reference Line
        grd_Ref.Width = workspaceWidthUnit * unitSize;
        grd_Ref.Height = workspaceHeightUnit * unitSize;

        grd_Ref.ColumnDefinitions = new();
        for(int i = 0; i < workspaceWidthUnit; i++)
        {
            grd_Ref.ColumnDefinitions.Add(new ColumnDefinition());
        }

        grd_Ref.RowDefinitions = new();
        for (int i = 0; i < workspaceHeightUnit; i++)
        {
            grd_Ref.RowDefinitions.Add(new RowDefinition());
        }

        grd_Ref.Children.Clear();
        for (int i = 0; i < workspaceWidthUnit; i++)
        {
            for(int j = 0; j < workspaceHeightUnit; j++)
            {
                Border bdr = new()
                {
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Colors.Transparent),
                    ZIndex = -1000
                };

                Grid.SetRow(bdr, i);
                Grid.SetColumn(bdr, j);

                grd_Ref.Children.Add(bdr);
            }
        }
        #endregion

        #region Robot
        originX = 0;
        originY = unitSize * (workspaceHeightUnit - 1);

        bdr_Robot.Width = unitSize;
        bdr_Robot.Height = unitSize;

        bdr_InnerRobot.Background = robotColor;
        bdr_InnerRobot.CornerRadius = new CornerRadius(bdr_InnerRobot.Width / 2, bdr_InnerRobot.Width / 2, 0, 0);

        Reset();
        #endregion

        #region Danger
        flashTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(250)
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
    }

    public void Place(int unitX, int unitY, Direction dir)
    {
        robotTranslate.X = originX + unitX * unitSize;
        robotTranslate.Y = originY + unitY * unitSize;
        robotRotate.Angle = ((int)dir) % 4 * 90;
    }

    public void Move(int unitX, int unitY)
    {
        double curX = robotTranslate.X;
        double curY = robotTranslate.Y;
        robotTranslate.X += unitX * unitSize;
        robotTranslate.Y += unitY * unitSize;
    }

    public void Rotate(Direction dir)
    {
        robotRotate.Angle = ((int)dir) % 4 * 90;
    }

    public void Danger(bool status)
    {
        if(status == true)
        {
            flashTimer.Start();
        }
        else
        {
            flashTimer.Stop();
            bdr_InnerRobot.Background = robotColor;
        }
    }
}