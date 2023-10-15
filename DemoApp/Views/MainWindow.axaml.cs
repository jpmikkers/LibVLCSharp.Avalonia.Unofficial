using Avalonia.ReactiveUI;
using DemoApp.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using LibVLCSharp.Avalonia.Unofficial;
using Avalonia.Controls;

namespace DemoApp.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	private VideoView _videoViewer;
	//public MainWindowViewModel viewModel;

	public MainWindow()
    {
        this.InitializeComponent(true,false);
        this.WhenActivated(d => d(ViewModel!.InteractionOpenFile.RegisterHandler(DoShowOpenFileDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.InteractionShowError.RegisterHandler(DoShowErrorAsync)));
		//this.Title = $"{Assembly.GetEntryAssembly()!.GetName().Version}";

		_videoViewer = this.Get<VideoView>("VideoViewer");

        Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object? sender, System.EventArgs e)
	{
		if(_videoViewer != null && _videoViewer.PlatformHandle!=null && ViewModel!.MediaPlayer != null)
		{
			_videoViewer.MediaPlayer = ViewModel.MediaPlayer;
			_videoViewer.MediaPlayer.SetHandle(_videoViewer.PlatformHandle);
			// or
			//_videoViewer.MediaPlayer.Hwnd = _videoViewer.PlatformHandle.Handle;

			// Set VideoView Content property by code
			//var tmp = new PlayerControls();
			//_videoViewer.SetContent(tmp._playerControl);                 
		}
	}

	private async Task DoShowErrorAsync(InteractionContext<string, Unit> ic)
    {
        await ErrorDialog.ShowErrorDialog(this, ic.Input);
        ic.SetOutput(Unit.Default);
    }

    private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit,System.Uri?> ic)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { 
                AllowMultiple = false, 
                FileTypeFilter = new[] { FilePickerFileTypes.All },
                Title = "Select file to play.." 
            }
        );

        if(files.Count > 0)
        {
            ic.SetOutput(files[0].Path);
        }
        else
        {
            ic.SetOutput(null);
        }
    }
}
