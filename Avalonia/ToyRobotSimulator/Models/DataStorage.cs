using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControlSimulator.Models;

public class DataStorage
{
    public static DataStorage Instance { get; set; } = new();

    public string? txtbox_Buffer = null;
}
