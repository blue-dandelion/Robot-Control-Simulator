using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using RobotControlSimulator.Models;
using RobotControlSimulator.ViewModels;

namespace RobotControlSimulator.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel vm = new();

    private Simulator sim;
    private LineHighlighter codeLineHighlighter = new LineHighlighter();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = vm;
        this.Focusable = true;

        #region UI
        txtbox_WorkspaceW = Utils.TextBox_Int(txtbox_WorkspaceW, int.MinValue, int.MaxValue);
        txtbox_WorkspaceH = Utils.TextBox_Int(txtbox_WorkspaceH, int.MinValue, int.MaxValue);
        txtbox_PlaceX = Utils.TextBox_Int(txtbox_PlaceX, int.MinValue, int.MaxValue);
        txtbox_PlaceY = Utils.TextBox_Int(txtbox_PlaceY, int.MinValue, int.MaxValue);
        txtbox_TimeSpan = Utils.TextBox_Double(txtbox_TimeSpan, 0, double.MaxValue);
        #endregion

        #region Simulator Control Section
        // Simulator
        sim = new(ws_Preview);

        sim.eh_ChangeRunLine += (sender, e) =>
        {
            codeLineHighlighter.runningLineId = e.RelatedCodeLineId;
            aetxtedt_CodeEditor.TextArea.TextView.Redraw();
        };

        sim.eh_FinishRunning += (sender, e) =>
        {
            StopRunningUI();
        };

        // Buttons
        btn_ReloadWorkspace.Click += (sender, e) =>
        {
            ws_Preview.Reload(int.Parse(txtbox_WorkspaceW.Text!), int.Parse(txtbox_WorkspaceH.Text!));
            sim.Reset(ws_Preview);
        };

        btn_Run.Click += (sender, e) =>
        {
            ClearResult();

            if (string.IsNullOrWhiteSpace(aetxtedt_CodeEditor.Text) || string.IsNullOrEmpty(aetxtedt_CodeEditor.Text)) return;

            btn_Run.IsEnabled = false;
            btn_RunLine.IsEnabled = false;
            txtbox_TimeSpan.IsEnabled = false;
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

                if (string.IsNullOrWhiteSpace(aetxtedt_CodeEditor.Text) || string.IsNullOrEmpty(aetxtedt_CodeEditor.Text)) return;

                btn_RunLine.Content = "Next Line";
                btn_Run.IsEnabled = false;
                txtbox_TimeSpan.IsEnabled = false;
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
        };

        btn_Reset.Click += (sender, e) =>
        {
            ClearResult();
            ws_Preview.Reset();
        };

        #endregion

        #region Editor Section

        #region Command Input
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

        #region IDE
        // Buttons
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
                SuggestedFileName = "MyCode.txt",
                FileTypeChoices = new List<FilePickerFileType>
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

        // IDE
        aetxtedt_CodeEditor.TextArea.TextView.LineTransformers.Add(new KeywordColorizer());
        aetxtedt_CodeEditor.TextArea.TextView.LineTransformers.Add(codeLineHighlighter);

        aetxtedt_CodeEditor.TextChanged += (sender, e) =>
        {
            aetxtedt_CodeEditor.TextArea.TextView.Redraw();
        };
        #endregion

        #endregion

        #region Result Section
        // Text
        aetxtedt_ErrorList.TextArea.TextView.LineTransformers.Add(new OutputKeywordColorizer());
        aetxtedt_ErrorList.TextChanged += (sender, e) =>
        {
            aetxtedt_ErrorList.TextArea.TextView.Redraw();
        };
        aetxtedt_Output.TextArea.TextView.LineTransformers.Add(new OutputKeywordColorizer());
        aetxtedt_Output.TextChanged += (sender, e) =>
        {
            aetxtedt_Output.TextArea.TextView.Redraw();
        };

        AppConsole.EA_WriteConsoleMessage.Subscribe<WriteConsoleMessage>((WriteConsoleMessage WCM) =>
        {
            if(WCM.HazzardLevel == HazLev.Normal)
            {
                aetxtedt_Output.Text += WCM.Message + "\n";
            }
            else if (WCM.HazzardLevel == HazLev.Warning) 
            {
                if (WCM.RelatedCodeLineId >= 0)
                {
                    codeLineHighlighter.warningLineIds.Add(WCM.RelatedCodeLineId);
                    aetxtedt_CodeEditor.TextArea.TextView.Redraw();
                }

                aetxtedt_ErrorList.Text += WCM.Message + "\n";
            }
            else if (WCM.HazzardLevel == HazLev.Error)
            {
                if (WCM.RelatedCodeLineId >= 0)
                {
                    codeLineHighlighter.errorLineIds.Add(WCM.RelatedCodeLineId);
                    aetxtedt_CodeEditor.TextArea.TextView.Redraw();
                }

                aetxtedt_ErrorList.Text += WCM.Message + "\n";
            }
        });

        // Preview
        btn_ZoomIn.Click += (sender, e) =>
        {
            ws_Preview.ZoomIn();
        };

        btn_ZoomOut.Click += (sender, e) =>
        {
            ws_Preview.ZoomOut();
        };
        #endregion

    }

    #region Simulator Control Section
    public void StopRunningUI()
    {
        btn_RunLine.Content = "Run Line";
        btn_StopRun.IsEnabled = false;
        btn_Run.IsEnabled = true;
        btn_RunLine.IsEnabled = true;
        btn_Reset.IsEnabled = true;
        txtbox_TimeSpan.IsEnabled = true;

        codeLineHighlighter.runningLineId = -1;
        aetxtedt_CodeEditor.TextArea.TextView.Redraw();
    }
    #endregion

    #region Editor Section
    public void AddContentToCodeEditor(string content)
    {
        int caretOffset = aetxtedt_CodeEditor.TextArea.Caret.Offset;
        aetxtedt_CodeEditor.Document.Insert(caretOffset, content);
        aetxtedt_CodeEditor.TextArea.Caret.Offset = caretOffset + content.Length;
        aetxtedt_CodeEditor.TextArea.Caret.BringCaretToView();
    }
    #endregion

    #region Result Section
    public void ClearResult()
    {
        aetxtedt_ErrorList.Text = string.Empty;
        aetxtedt_ErrorList.Text = string.Empty;
        aetxtedt_Output.Text = string.Empty;
        aetxtedt_Output.Text = string.Empty;
        codeLineHighlighter.ResetHighlight();
        aetxtedt_CodeEditor.TextArea.TextView.Redraw();
    }
    #endregion

}
