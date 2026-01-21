using Avalonia.Controls;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views;

public partial class TaskMonitorView : UserControl
{
    public TaskMonitorView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is TaskMonitorViewModel viewModel)
        {
            viewModel.OnNavigatedTo();
        }
    }
}
