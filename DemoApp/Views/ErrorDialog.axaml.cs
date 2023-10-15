using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DemoApp.ViewModels;
using System.Threading.Tasks;

namespace DemoApp;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
    }

    public static async Task ShowErrorDialog(Window owner, string errorText)
    {
        var vm = new ErrorDialogViewModel
        {
            ErrorText = errorText
        };

        var dialog = new ErrorDialog() { DataContext = vm };

        vm.OkAction = () =>
        {
            dialog.Close();
        };

        await dialog.ShowDialog(owner);
    }
}
