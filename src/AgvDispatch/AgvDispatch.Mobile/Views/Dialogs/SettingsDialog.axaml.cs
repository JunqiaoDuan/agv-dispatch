using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    public SettingsDialog(SettingsDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
