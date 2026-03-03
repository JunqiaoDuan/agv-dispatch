using CommunityToolkit.Mvvm.ComponentModel;
using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Mobile.ViewModels;

/// <summary>
/// 可选择的 AGV 推荐项
/// </summary>
public partial class SelectableAgvRecommendation : ObservableObject
{
    public AgvRecommendationDto Dto { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableAgvRecommendation(AgvRecommendationDto dto)
    {
        Dto = dto;
    }
}
