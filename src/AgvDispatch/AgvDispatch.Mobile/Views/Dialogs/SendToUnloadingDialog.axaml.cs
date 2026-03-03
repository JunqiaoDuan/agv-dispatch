using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class SendToUnloadingDialog : Window
{
    private bool _isConfirming = false;

    public SendToUnloadingDialog()
    {
        InitializeComponent();
    }

    public SendToUnloadingDialog(SendToUnloadingDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnSelectItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is SelectableAgvPendingItem item && DataContext is SendToUnloadingDialogViewModel viewModel)
        {
            viewModel.SelectItem(item);
        }
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        // 防抖：如果正在处理中，直接返回
        if (_isConfirming)
        {
            return;
        }

        if (DataContext is SendToUnloadingDialogViewModel viewModel)
        {
            try
            {
                _isConfirming = true;
                bool success = await viewModel.ConfirmAsync();
                if (success)
                {
                    Close(true);
                }
            }
            finally
            {
                _isConfirming = false;
            }
        }
    }

    private void OnToggleShowAllClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SendToUnloadingDialogViewModel viewModel)
        {
            viewModel.ToggleShowAll();

            // 更新按钮文本和图标
            if (ToggleButtonText != null && ToggleIcon != null)
            {
                ToggleButtonText.Text = viewModel.ShowAllItems ? "折叠" : "展开";
                ToggleIcon.Text = viewModel.ShowAllItems ? "▲" : "▼";
            }
        }
    }
}
