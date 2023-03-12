using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DemoApp.ViewModels;
using DemoApp.Views;
using ReactiveUI;
using Avalonia.ReactiveUI;
namespace DemoApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // suspension, see https://habr.com/ru/post/462307/ and https://docs.avaloniaui.net/guides/deep-dives/reactiveui/data-persistence
        //// Create the AutoSuspendHelper.
        var suspension = new AutoSuspendHelper(ApplicationLifetime);
        RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new SuspensionDriver());
        suspension.OnFrameworkInitializationCompleted();

        //// Load the saved view model state.
        var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
        //RxApp.SuspensionHost.AutoPersist()

        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = state,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
