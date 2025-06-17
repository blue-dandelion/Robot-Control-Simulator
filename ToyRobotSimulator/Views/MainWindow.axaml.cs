using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ToyRobotSimulator.Models;
using ToyRobotSimulator.ViewModels;

namespace ToyRobotSimulator.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel vm = new();

    string errorList = string.Empty;
    string output = string.Empty;

    Simulator sim = new();
    LineHighlighter lineHighlighter = new LineHighlighter();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = vm;
        this.Focusable = true;

        #region UI
        txtbox_PlaceX = Utils.TextBox_Int(txtbox_PlaceX, int.MinValue, int.MaxValue);
        txtbox_PlaceY = Utils.TextBox_Int(txtbox_PlaceY, int.MinValue, int.MaxValue);
        txtbox_TimeSpan = Utils.TextBox_Double(txtbox_TimeSpan, 0, double.MaxValue);
        #endregion

        #region Execute
        btn_Run.Click += (sender, e) =>
        {
            ClearResult();
            ResetSimulator();

            btn_Run.IsEnabled = false;
            btn_RunLine.IsEnabled = false;
            btn_StopRun.IsEnabled = true;
            btn_Reset.IsEnabled = false;

            Dispatcher.UIThread.Post(() =>
            {
                sim.Run(aetxtedt_CodeEditor.Text, TimeSpan.FromSeconds(double.Parse(txtbox_TimeSpan.Text!)));
            });
        };

        btn_RunLine.Click += (sender, e) =>
        {
            if (!sim.isRunning)
            {
                ClearResult();
                ResetSimulator();

                btn_RunLine.Content = "Next Line";
                btn_Run.IsEnabled = false;
                btn_StopRun.IsEnabled = true;
                btn_Reset.IsEnabled = false;

                Dispatcher.UIThread.Post(() =>
                {
                    sim.Run(aetxtedt_CodeEditor.Text, TimeSpan.Zero, true);
                });
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    sim.RunNextLine();
                });
            }
        };

        btn_StopRun.Click += (sender, e) =>
        {
            if (sim.isRunning)
            {
                sim.StopRunning();
            }

            StopRunningUI();
        };

        btn_Reset.Click += (sender, e) =>
        {
            ClearResult();
            robprv.Reset();
        };

        #endregion

        #region Enter code
        btn_EnterPLACE.Click += (sender, e) =>
        {
            // Make sure all the essential infos are provided
            if (string.IsNullOrEmpty(txtbox_PlaceX.Text) || string.IsNullOrEmpty(txtbox_PlaceY.Text) || cobbox_Direction.SelectedIndex < 0 || cobbox_Direction.SelectedIndex >= Enum.GetValues(typeof(Direction)).Length) return;
        
            AddContentToCodeEditor($"PLACE {txtbox_PlaceX.Text},{txtbox_PlaceY.Text},{(Direction)(cobbox_Direction.SelectedIndex)}\n");
        };
        btn_EnterMOVE.Click += (sender, e) =>
        {
            AddContentToCodeEditor("MOVE\n");
        };
        btn_EnterLEFT.Click += (sender, e) =>
        {
            AddContentToCodeEditor("LEFT\n");
        }; 
        btn_EnterRIGHT.Click += (sender, e) =>
        {
            AddContentToCodeEditor("RIGHT\n");
        };
        btn_EnterREPORT.Click += (sender, e) =>
        {
            AddContentToCodeEditor("REPORT\n");
        };
        #endregion

        #region Code Editor Control
        btn_ImportFile.Click += async (sender, e) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import Code File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Text Documents")
                    {
                        Patterns = ["*.txt"],
                        MimeTypes = ["text/plain"]
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = ["*.*"],
                    }
                }
            });

            if (files.Count > 0)
            {
                // Open reading stream from the first file
                await using var stream = await files[0].OpenReadAsync();
                using var streamReader = new StreamReader(stream);

                // Read all the content of the file as a text
                var fileContent = await streamReader.ReadToEndAsync();
                AddContentToCodeEditor(fileContent);
            }
        };

        btn_ExportFile.Click += async (sender, e) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save Code File",
                DefaultExtension = "txt"
            });

            if (file != null)
            {
                // Open writing stream from the file
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);

                // Read all the content of the file as a text
                await streamWriter.WriteLineAsync(aetxtedt_CodeEditor.Text);
            }
        };

        btn_ClearCode.Click += (sender, e) =>
        {
            aetxtedt_CodeEditor.Text = null;
            aetxtedt_CodeEditor.TextArea.Caret.Offset = 0;
        };
        #endregion

        #region IDE
        aetxtedt_CodeEditor.TextArea.TextView.LineTransformers.Add(new KeywordColorizer());
        aetxtedt_CodeEditor.TextArea.TextView.LineTransformers.Add(lineHighlighter);

        aetxtedt_CodeEditor.TextChanged += (sender, e) =>
        {
            aetxtedt_CodeEditor.TextArea.TextView.Redraw();
        };

        IDE.eh_Warning += (sender, e) =>
        {
            AddWarning(e);
        };
        IDE.eh_Error += (sender, e) =>
        {
            AddError(e);
        };
        #endregion

        #region Results
        robprv.workspaceHeightUnit = 5;
        robprv.workspaceHeightUnit = 5;
        robprv.Reload();
        #endregion

    }

    public void ResetSimulator()
    {
        sim = new()
        {
            spaceW = 5,
            spaceH = 5,
        };
        sim.SetVirtualRobot(robprv);

        sim.eh_ChangeRunLine += (sender, e) =>
        {
            lineHighlighter.runningLineId = e.RelatedCodeLineId;
            aetxtedt_CodeEditor.TextArea.TextView.Redraw();
        };

        sim.eh_FinishRunning += (sender, e) =>
        {
            StopRunningUI();
        };

        sim.eh_Warning += (sender, e) =>
        {
            AddWarning(e);
        };

        sim.eh_SendMessage += (sender, e) =>
        {
            AddOutput(e);
        };
    }

    public void StopRunningUI()
    {
        btn_RunLine.Content = "Run Line";
        btn_StopRun.IsEnabled = false;
        btn_Run.IsEnabled = true;
        btn_RunLine.IsEnabled = true;
        btn_Reset.IsEnabled = true;

        lineHighlighter.runningLineId = -1;
        aetxtedt_CodeEditor.TextArea.TextView.Redraw();
    }

    public void AddContentToCodeEditor(string content)
    {
        int caretOffset = aetxtedt_CodeEditor.TextArea.Caret.Offset;
        aetxtedt_CodeEditor.Document.Insert(caretOffset, content);
        aetxtedt_CodeEditor.TextArea.Caret.Offset = caretOffset + content.Length;
        aetxtedt_CodeEditor.TextArea.Caret.BringCaretToView();
    }

    public void AddWarning(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(errorList)) errorList += "\n";

        if(e.RelatedCodeLineId == -1)
        {
            errorList += $"Warning: {e.Message}";
        }
        else
        {
            lineHighlighter.warningLineIds.Add(e.RelatedCodeLineId);
            errorList += $"Warning (line{e.RelatedCodeLineId}): {e.Message}";
        }

        UpdateResults();
    }
    public void AddError(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(errorList)) errorList += "\n";

        if (e.RelatedCodeLineId == -1)
        {
            errorList += $"Error: {e.Message}";
        }
        else
        {
            lineHighlighter.errorLineIds.Add(e.RelatedCodeLineId);
            errorList += $"Error (line{e.RelatedCodeLineId}): {e.Message}";
        }

        UpdateResults();
    }
    public void AddOutput(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(output)) output += "\n";

        if (e.RelatedCodeLineId == -1)
        {
            output += $"Output: {e.Message}";
        }
        else
        {
            output += $"Output (line{e.RelatedCodeLineId}): {e.Message}";
        }

        UpdateResults();
    }

    public void UpdateResults()
    {
        aetxtedt_ErrorList.Text = errorList;
        aetxtedt_Output.Text = output;

        aetxtedt_CodeEditor.TextArea.TextView.Redraw();
    }

    public void ClearResult()
    {
        errorList = string.Empty;
        aetxtedt_ErrorList.Text = string.Empty;
        output = string.Empty;
        aetxtedt_Output.Text = string.Empty;
        lineHighlighter.ResetHighlight();
        aetxtedt_CodeEditor.TextArea.TextView.Redraw();
    }

}