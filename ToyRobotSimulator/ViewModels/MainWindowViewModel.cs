using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using ToyRobotSimulator.Views;

namespace ToyRobotSimulator.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Bind values
    private string _placeX = "0";
    public string PlaceX
    {
        get => _placeX;
        set => this.RaiseAndSetIfChanged(ref _placeX, value);
    }

    private string _placeY = "0";
    public string PlaceY
    {
        get => _placeY;
        set => this.RaiseAndSetIfChanged(ref _placeY, value);
    }

    private int _directionSelectIndex = 0;
    public int DirectionSelectIndex
    {
        get => _directionSelectIndex;
        set => this.RaiseAndSetIfChanged(ref _directionSelectIndex, value);
    }

    private string _debugMessage = string.Empty;
    public string DebugMessage
    {
        get => _debugMessage;
        set => this.RaiseAndSetIfChanged(ref _debugMessage, value);
    }

    private string _output = string.Empty;
    public string Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }
    #endregion

    public MainWindowViewModel() 
    {
    }
}
