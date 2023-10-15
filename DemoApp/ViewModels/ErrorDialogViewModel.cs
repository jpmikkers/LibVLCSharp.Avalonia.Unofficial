using System;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Avalonia.Threading;

namespace DemoApp.ViewModels;

public class ErrorDialogViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> CommandOk { get; }
    public Action OkAction { get; set; } = () => { };

    private string _errorText = string.Empty;

    public string ErrorText
    {
        get => _errorText;
        set => this.RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ErrorDialogViewModel()
    {
        CommandOk = ReactiveCommand.CreateFromTask(DoOk);
    }

    private async Task DoOk()
    {
        OkAction();
        await Task.CompletedTask;
    }
}
