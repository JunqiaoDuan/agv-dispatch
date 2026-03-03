using CommunityToolkit.Mvvm.ComponentModel;
using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 可选择的待处理小车项
/// </summary>
public partial class SelectableAgvPendingItem : ObservableObject
{
    public AgvPendingItemDto Dto { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableAgvPendingItem(AgvPendingItemDto dto)
    {
        Dto = dto;
    }
}
