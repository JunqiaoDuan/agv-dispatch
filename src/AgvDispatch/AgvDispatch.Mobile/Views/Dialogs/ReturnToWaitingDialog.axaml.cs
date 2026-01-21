using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;
using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class ReturnToWaitingDialog : Window
{
    public ReturnToWaitingDialog()
    {
        InitializeComponent();
    }

    public ReturnToWaitingDialog(ReturnToWaitingDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnSelectItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is AgvPendingItemDto item && DataContext is ReturnToWaitingDialogViewModel viewModel)
        {
            viewModel.SelectItem(item);
        }
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ReturnToWaitingDialogViewModel viewModel)
        {
            bool success = await viewModel.ConfirmAsync();
            if (success)
            {
                Close(true);
            }
        }
    }
}
