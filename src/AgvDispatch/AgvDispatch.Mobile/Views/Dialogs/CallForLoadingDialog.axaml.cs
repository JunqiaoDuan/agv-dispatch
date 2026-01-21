using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;
using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class CallForLoadingDialog : Window
{
    public CallForLoadingDialog()
    {
        InitializeComponent();
    }

    public CallForLoadingDialog(CallForLoadingDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnSelectRecommendationClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is AgvRecommendationDto recommendation && DataContext is CallForLoadingDialogViewModel viewModel)
        {
            viewModel.SelectRecommendation(recommendation);
        }
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CallForLoadingDialogViewModel viewModel)
        {
            bool success = await viewModel.ConfirmAsync();
            if (success)
            {
                Close(true);
            }
        }
    }
}
