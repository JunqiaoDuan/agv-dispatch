using Avalonia.Controls;
using Avalonia.Interactivity;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Components;

public partial class AgvCardView : UserControl
{
    public AgvCardView()
    {
        InitializeComponent();
    }

    private void OnShowExceptionsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 查找父级的TaskMonitorViewModel
            var parent = this.Parent;
            while (parent != null)
            {
                if (parent is Control control && control.DataContext is TaskMonitorViewModel viewModel)
                {
                    // 使用异步命令
                    _ = viewModel.ShowExceptionsCommand.ExecuteAsync(agv.AgvCode);
                    break;
                }
                parent = parent.Parent;
            }
        }
    }
}
