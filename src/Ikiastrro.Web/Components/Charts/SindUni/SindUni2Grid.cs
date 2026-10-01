namespace Ikiastrro.Web.Components.Charts.SindUni;

/// <summary>SIND-UNI-2: the reading view of <see cref="SindUniGrid"/> (spec_SIND-UNI_GridChart.md).</summary>
public sealed class SindUni2Grid : SindUniGrid
{
    protected override void OnParametersSet()
    {
        View = SindUniView.Reading;
        base.OnParametersSet();
    }
}
