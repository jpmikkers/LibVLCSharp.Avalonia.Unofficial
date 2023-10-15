using System;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using LibVLCSharp.Shared;
using Avalonia.Threading;

namespace DemoApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	LibVLC? _libVLC;
	public MediaPlayer? MediaPlayer;

    private string _status = " ";
    private double _progress = 0.0;
    private string _localFile = "";

    public double Progress 
	{ 
		get => _progress; 
		set => this.RaiseAndSetIfChanged(ref _progress, value); 
	}

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

	[Required(AllowEmptyStrings = false, ErrorMessage = "This field is required")]
	public string MediaUri
	{
		get => _localFile;
		set => this.RaiseAndSetIfChanged(ref _localFile, value);
	}

	public ReactiveCommand<Unit, Unit> CommandSelectFile { get; }
	public ReactiveCommand<Unit, Unit> CommandPlay { get; }
	public ReactiveCommand<Unit, Unit> CommandStop { get; }
	public ReactiveCommand<Unit, Unit> CommandPause { get; }

	public Interaction<Unit, Uri?> InteractionOpenFile { get; }
    public Interaction<string, Unit> InteractionShowError { get; }

	private async Task DoPlay()
	{
        if(string.IsNullOrWhiteSpace(MediaUri))
        {
            await InteractionShowError.Handle("Please select a file first");
            return;
        }

        if(_libVLC != null && MediaPlayer != null)
		{
			//string[] Media_AdditionalOptions = {
			//    $":avcodec-hw=any"
			//};
			string[] Media_AdditionalOptions = { };

			using var media = new Media(
				_libVLC,
				new Uri(MediaUri), //new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"),
				Media_AdditionalOptions
				);

			MediaPlayer.Play(media);
			media.Dispose();
		}
	}

	private async Task DoPause()
	{
		if(MediaPlayer != null && MediaPlayer.CanPause)
		{
			MediaPlayer.Pause();
		}
		await Task.CompletedTask;
	}

	private async Task DoStop()
    {
		if(MediaPlayer != null)
		{
			MediaPlayer.Stop();
		}
		await Task.CompletedTask;
	}

    private async Task DoSelectFile()
    {
        var fn = await InteractionOpenFile.Handle(Unit.Default);
        if(fn != null)
        {
            MediaUri = fn.ToString();
        }
    }

    public MainWindowViewModel()
    {
        CommandStop = ReactiveCommand.CreateFromTask(DoStop);
        CommandSelectFile = ReactiveCommand.CreateFromTask(DoSelectFile);
        CommandPlay = ReactiveCommand.CreateFromTask(DoPlay);
		CommandPause = ReactiveCommand.CreateFromTask(DoPause);
		InteractionOpenFile = new Interaction<Unit, Uri?>();
        InteractionShowError = new Interaction<string, Unit>();

		if(!Avalonia.Controls.Design.IsDesignMode)
		{
			//var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem;
			//if (os == OperatingSystemType.WinNT)
			//{
			//    var libVlcDirectoryPath = Path.Combine(Environment.CurrentDirectory, "libvlc", IsWin64() ? "win-x64" : "win-x86");
			//    Core.Initialize(libVlcDirectoryPath);
			//}
			//else
			//{
			//	Core.Initialize();
			//}

			_libVLC = new LibVLC(
				enableDebugLogs: true
				);
			//_libVLC.Log += VlcLogger_Event;

			MediaPlayer = new MediaPlayer(_libVLC) { };
			Status = MediaPlayer.State.ToString();

			Observable.FromEventPattern<MediaPlayerPositionChangedEventArgs>(
				handler => MediaPlayer.PositionChanged += handler,
				handler => MediaPlayer.PositionChanged -= handler)
				.Sample(TimeSpan.FromSeconds(0.1))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x => { Progress = Math.Clamp(100.0 * x.EventArgs.Position, 0, 100.0); });

			Observable.FromEventPattern<EventArgs>(
				handler => MediaPlayer.Playing += handler,
				handler => MediaPlayer.Playing -= handler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x => { Status = MediaPlayer.State.ToString(); });

			Observable.FromEventPattern<EventArgs>(
				handler => MediaPlayer.Stopped += handler,
				handler => MediaPlayer.Stopped -= handler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x => { Status = MediaPlayer.State.ToString(); });

			Observable.FromEventPattern<EventArgs>(
				handler => MediaPlayer.Paused += handler,
				handler => MediaPlayer.Paused -= handler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(x => { Status = MediaPlayer.State.ToString(); });
		}
	}
}
