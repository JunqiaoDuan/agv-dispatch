using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class TaskDetailDialog : Window
{
    public TaskDetailDialog()
    {
        InitializeComponent();
    }

    public TaskDetailDialog(TaskDetailDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
