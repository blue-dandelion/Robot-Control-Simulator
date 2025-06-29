﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotControlSimulator.UserControls;

namespace RobotControlSimulator.Models;

public class Robot(Workspace ws)
{
    public int posX = -1;
    public int posY = -1;
    public Direction facing = Direction.NORTH;

    public bool startMove = false;
    public string message = string.Empty;

    public event EventHandler? eh_Falling;
    public event EventHandler? eh_Report;

    public bool IsInWorkspace(int x, int y)
    {
        if (x >= 0 && x < ws.width && y >= 0 && y < ws.height)
        {
            ws?.Danger(false);
            return true;
        }

        eh_Falling?.Invoke(this, EventArgs.Empty);
        ws?.Danger(true);
        return false;
    }

    public void Place(int x, int y, Direction dir)
    {
        // Make sure the robot is placed in the workplace
        if (!IsInWorkspace(x, y)) return;

        // Place the robot
        posX = x;
        posY = y;
        facing = dir;

        // Since the robot has been places, the following commands will be able to run
        startMove = true;

        ws?.Place(x, -y, facing);
    }

    public void Move()
    {
        if (!startMove) return;

        // Make sure the robot is placed in the workplace
        if (!IsInWorkspace(posX, posY)) return;

        // Record the current position
        int moveX = 0;
        int moveY = 0;

        // Move one unit to the facing facing
        // Origin(0,0) is the South West most corner
        switch (facing)
        {
            case Direction.NORTH:
                moveY = 1;
                break;
            case Direction.EAST:
                moveX = 1;
                break;
            case Direction.SOUTH:
                moveY = -1;
                break;
            case Direction.WEST:
                moveX = -1;
                break;
            default:
                break;
        }

        // If the robot will move out of the workspace, ignore this movement
        if (!IsInWorkspace(posX + moveX, posY + moveY)) return;

        // Otherwise, apply this movement
        posX += moveX;
        posY += moveY;

        ws?.Move(moveX, -moveY);
    }

    public void Rotate(string dir)
    {
        if (!startMove) return;

        // Make sure the robot is placed in the workplace
        if (!IsInWorkspace(posX, posY)) return;

        switch (dir)
        {
            case "LEFT":
                facing = (Direction)(((int)facing + 3) % 4);
                break;
            case "RIGHT":
                facing = (Direction)(((int)facing + 1) % 4);
                break;
            default:
                break;
        }

        ws?.Rotate(facing);
    }

    public void Report()
    {
        if (!startMove) return;

        message = $"{posX},{posY},{facing}";

        eh_Report?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        startMove = false;
    }
}
