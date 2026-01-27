using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class ToggleCargoDialog : Window
{
    public ToggleCargoDialog()
    {
        InitializeComponent();
    }

    public ToggleCargoDialog(ToggleCargoDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ToggleCargoDialogViewModel viewModel)
        {
            var success = await viewModel.ConfirmAsync();
            if (success)
            {
                Close(true);
            }
        }
    }
}
