using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Components;

public partial class AgvDetailCard : UserControl
{
    public AgvDetailCard()
    {
        InitializeComponent();
    }

    private void OnExceptionTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 获取 ViewModel
            var viewModel = GetTaskMonitorViewModel();
            if (viewModel != null && agv.UnresolvedExceptionCount > 0)
            {
                viewModel.ShowAgvExceptionsCommand.Execute(agv.AgvCode);
            }
        }
    }

    private void OnCargoTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AgvMonitorItemDto agv)
        {
            // 获取 ViewModel
            var viewModel = GetTaskMonitorViewModel();
            if (viewModel != null)
            {
                viewModel.ShowToggleCargoCommand.Execute(agv);
            }
        }
    }

    private TaskMonitorViewModel? GetTaskMonitorViewModel()
    {
        // 从父控件层级查找 TaskMonitorViewModel
        var parent = this.Parent;
        while (parent != null)
        {
            if (parent is UserControl userControl && userControl.DataContext is TaskMonitorViewModel vm)
            {
                return vm;
            }
            parent = parent.Parent;
        }
        return null;
    }
}
