using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Mobile.ViewModels;

namespace AgvDispatch.Mobile.Views.Components;

public partial class TaskCard : UserControl
{
    private IBrush? _originalBackground;

    public TaskCard()
    {
        InitializeComponent();
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is TaskListItemDto task)
        {
            // 获取 ViewModel
            var viewModel = GetTaskMonitorViewModel();
            if (viewModel != null)
            {
                viewModel.ShowTaskDetailCommand.Execute(task);
            }
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            _originalBackground = border.Background;
            border.Background = new SolidColorBrush(Color.Parse("#F5F5F5"));
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border && _originalBackground != null)
        {
            border.Background = _originalBackground;
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
