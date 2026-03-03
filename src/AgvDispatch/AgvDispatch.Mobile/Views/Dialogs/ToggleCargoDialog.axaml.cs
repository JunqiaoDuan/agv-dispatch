using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class ToggleCargoDialog : Window
{
    private bool _isConfirming = false;

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
        // 防抖：如果正在处理中，直接返回
        if (_isConfirming)
        {
            return;
        }

        if (DataContext is ToggleCargoDialogViewModel viewModel)
        {
            try
            {
                _isConfirming = true;
                var success = await viewModel.ConfirmAsync();
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
}
