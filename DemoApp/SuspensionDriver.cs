using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using DemoApp.ViewModels;
using System.Text.Json;
using System.IO;
namespace DemoApp;

public class SuspensionDriver : ISuspensionDriver
{
    string ApplicationName = "Avalonia11VLCSharpDemoApp";
    string DemoMedia = @"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

    private string GetLocalApplicationFolder()
    {
        var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), ApplicationName);
        if(!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        return result;
    }

	// on linux: ~/.local/share/Avalonia11VLCSharpDemoApp/State.json
	// on windows: %LocalAppData%\Avalonia11VLCSharpDemoApp\State.json
	private string StatePath => Path.Combine(GetLocalApplicationFolder(), "State.json");

    public IObservable<Unit> InvalidateState()
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState()
    {
        try
        {
            using(var stream = File.OpenRead(StatePath))
            {
                var state = JsonSerializer.Deserialize<MySavedState>(stream);

                if(state != null)
                {
                    return Observable.Return(new MainWindowViewModel()
                    {
                        MediaUri = string.IsNullOrWhiteSpace(state.MediaUri) ? DemoMedia : state.MediaUri,
                    });
                }
            }
        }
        catch
        {
        }

        return Observable.Return(
            new MainWindowViewModel() { 
                MediaUri = DemoMedia 
            }
        );
    }

    public IObservable<Unit> SaveState(object state)
    {
        try
        {
            if(state is MainWindowViewModel viewModel)
            {
                using(var stream = File.Create(StatePath))
                {
                    JsonSerializer.Serialize(stream,
                        new MySavedState
                        {
                            MediaUri = viewModel.MediaUri,
                        },
                        new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }
        catch
        {
        }
        return Observable.Return(Unit.Default);
    }
}
