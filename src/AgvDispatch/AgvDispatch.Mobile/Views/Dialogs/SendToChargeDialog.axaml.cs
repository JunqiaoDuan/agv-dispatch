using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class SendToChargeDialog : Window
{
    public SendToChargeDialog(SendToChargeDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
