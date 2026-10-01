namespace Ikiastrro.Web.Components.Charts.SindUni;

/// <summary>SIND-UNI-1: the compact view of <see cref="SindUniGrid"/> (spec_SIND-UNI_GridChart.md).</summary>
public sealed class SindUni1Grid : SindUniGrid
{
    protected override void OnParametersSet()
    {
        View = SindUniView.Compact;
        base.OnParametersSet();
    }
}
