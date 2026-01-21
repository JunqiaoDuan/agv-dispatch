using Avalonia.Controls;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views;

public partial class AgvListView : UserControl
{
    public AgvListView()
    {
        InitializeComponent();

        // 当控件加载完成时，触发数据加载
        this.AttachedToVisualTree += (s, e) =>
        {
            if (DataContext is AgvListViewModel viewModel)
            {
                viewModel.OnNavigatedTo();
            }
        };
    }
}
