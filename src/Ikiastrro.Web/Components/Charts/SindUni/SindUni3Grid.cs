namespace Ikiastrro.Web.Components.Charts.SindUni;

/// <summary>SIND-UNI-3: the micro view of <see cref="SindUniGrid"/> (spec_SIND-UNI_GridChart.md).</summary>
public sealed class SindUni3Grid : SindUniGrid
{
    protected override void OnParametersSet()
    {
        View = SindUniView.Micro;
        base.OnParametersSet();
    }
}
