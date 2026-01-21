using Avalonia.Controls;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Dialogs;

public partial class AgvExceptionsDialog : Window
{
    public AgvExceptionsDialog()
    {
        InitializeComponent();
    }

    public AgvExceptionsDialog(AgvExceptionsDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
