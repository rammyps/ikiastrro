using System.Globalization;
using System.Text.Json;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ikiastrro.Web.Components.Charts.SindUni;

public partial class SindUniGrid
{
    /// <summary>The chart to draw — <see cref="SindUniBuilder.Build"/>.</summary>
    [Parameter, EditorRequired] public SindUniChart Chart { get; set; } = null!;

    /// <summary>Which view. The named wrappers (SindUni1Grid / 2 / 3) fix this.</summary>
    [Parameter] public SindUniView View { get; set; } = SindUniView.Reading;

    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Note { get; set; }
    /// <summary>Centre meta line; defaults to the Lagna and Moon with their nakshatra codes.</summary>
    [Parameter] public string? Meta { get; set; }
    [Parameter] public string AriaLabel { get; set; } = "South Indian chart";

    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowKey { get; set; } = true;
    /// <summary>Spl Lagnas view: multi-select special lagnas drawn as colour bars (Micro only).</summary>
    [Parameter] public bool ShowSpecialLagnas { get; set; }

    /// <summary>When set, these special-lagna codes are drawn instead of the grid's own toggles — the Spl Lagnas page
    /// drives them from its dropdowns (pair with <see cref="HideLagnaToolbar"/>).</summary>
    [Parameter] public IReadOnlySet<string>? ActiveLagnas { get; set; }
    /// <summary>Hide the grid's own row of special-lagna chips (the page supplies a dropdown instead).</summary>
    [Parameter] public bool HideLagnaToolbar { get; set; }
    /// <summary>Special points (upagrahas, sphuṭas, sahams …) already placed on this chart's signs, drawn as small
    /// coloured chips in each cell (Micro only) and listed in full on the centre card when a sign is hovered or pinned.</summary>
    [Parameter] public IReadOnlyList<PlacedPoint>? ExtraPoints { get; set; }

    /// <summary>Pinned sign (two-way). Hover previews; click pins; clicking the pinned sign unpins.</summary>
    [Parameter] public ZodiacName? PinnedSign { get; set; }
    [Parameter] public EventCallback<ZodiacName?> PinnedSignChanged { get; set; }
    [Parameter] public EventCallback<ZodiacName?> PreviewSignChanged { get; set; }

    /// <summary>Signs to outline as relevant (Key Inference: the houses a question reads).</summary>
    [Parameter] public IReadOnlySet<ZodiacName>? RelevantSigns { get; set; }
    /// <summary>Arudha codes to emphasise among the tags.</summary>
    [Parameter] public IReadOnlySet<string>? FocusCodes { get; set; }
    /// <summary>Planets to ring as karakas of the question.</summary>
    [Parameter] public IReadOnlySet<string>? KarakaPlanets { get; set; }
    [Parameter] public ZodiacName? TrackSign { get; set; }
    [Parameter] public string? TrackTag { get; set; }
    [Parameter] public string? TrackLabel { get; set; }

    /// <summary>localStorage key suffix for this viewer's layer / reference / lens choices;
    /// null keeps them for this render only.</summary>
    [Parameter] public string? PrefsKey { get; set; }

    protected sealed record LayerDef(string Key, string Icon, string Label, bool TextIcon);

    private static readonly LayerDef[] AllLayers =
    [
        new("bm", "BEN", "Benefic / malefic", true),
        new("sav", "28", "Ashtakavarga", true),
        new("attr", "\U0001F702", "Sign nature", false),
        new("el", "▬", "Element colour", true),
        new("nak", "ASW", "Nakshatras", true),
        new("pnak", "✦", "Planet pada", false),
        new("deg", "0°", "Degrees", true),
        new("orb", "", "Planet images", false),
        new("kar", "AK", "Karakas", true),
        new("sp", "AL", "Arudhas", true),
    ];

    private static readonly Dictionary<SindUniView, string[]> LayersByView = new()
    {
        [SindUniView.Compact] = ["bm", "sav", "attr", "el", "nak", "deg"],
        [SindUniView.Reading] = ["bm", "sav", "attr", "el", "nak", "pnak", "deg", "orb", "kar", "sp"],
        [SindUniView.Micro] = ["bm", "orb"],
    };

    private static readonly Dictionary<SindUniView, string[]> DefaultLayers = new()
    {
        [SindUniView.Compact] = ["bm", "sav", "attr", "el", "nak", "pnak", "deg"],
        [SindUniView.Reading] = ["bm", "sav", "attr", "el", "nak", "pnak", "deg", "orb", "sp"],
        [SindUniView.Micro] = ["bm", "orb"],
    };

    protected static readonly Dictionary<string, string[]> Presets = new()
    {
        ["Calm"] = ["bm", "attr", "el", "deg"],
        ["Reading"] = ["bm", "sav", "attr", "el", "nak", "pnak", "deg", "orb", "sp"],
        ["Everything"] = ["bm", "sav", "attr", "el", "nak", "pnak", "deg", "orb", "kar", "sp"],
    };

    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    private HashSet<string> _layers = [];
    private HashSet<string> _lagnas = [];
    private string _ref = "Moon";
    private SindUniLens _lens = SindUniLens.Off;
    private string? _preset;
    private ZodiacName? _hover;
    private bool _initialised;

    private IReadOnlySet<string> LagnaSet => ActiveLagnas ?? _lagnas;

    /// <summary>Chips a cell can hold with no planets; each pair of planets in the cell takes two chips' room (the "+n" chip covers the rest).</summary>
    private const int MaxChips = 8;
    private IReadOnlyList<PlacedPoint>? _extraSource;
    private IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlacedPoint>> _extraBySign = new Dictionary<ZodiacName, IReadOnlyList<PlacedPoint>>();

    private IReadOnlyList<PlacedPoint> ExtraIn(ZodiacName sign)
    {
        if (!ReferenceEquals(_extraSource, ExtraPoints))
        {
            _extraSource = ExtraPoints;
            _extraBySign = (ExtraPoints ?? [])
                .GroupBy(p => p.Sign)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<PlacedPoint>)g.OrderBy(p => p.Def.Group).ThenBy(p => p.Def.Longitude).ToList());
        }
        return _extraBySign.GetValueOrDefault(sign) ?? [];
    }

    private static string PointTitle(PlacedPoint p) =>
        $"{p.Def.Name} · {KeyInfoFormat.Degree(p.Def.Longitude)} {KeyInfoFormat.Sign(p.Def.Longitude)} (D1) · {p.Def.Meaning}";

    protected IReadOnlyList<LayerDef> Layers => LayersByView[View].Select(k => AllLayers.First(l => l.Key == k)).ToList();

    private string ViewName => View.ToString().ToLowerInvariant();

    private string LayerClasses => string.Join(" ", _layers.Select(l => "L-" + l));

    private string RootStyle => "";

    protected override void OnParametersSet()
    {
        if (_initialised) return;
        _initialised = true;
        _layers = [.. DefaultLayers[View]];
        _lagnas = [.. SindUniGlyphs.SpecialLagnas.Where(k => k.OnByDefault).Select(k => k.Code)];
        _preset = View == SindUniView.Reading ? "Reading" : null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || PrefsKey is null) return;
        try
        {
            var json = await JS.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json)) return;
            var prefs = JsonSerializer.Deserialize<Prefs>(json);
            if (prefs is null) return;
            if (prefs.Layers is { } l) _layers = [.. l.Where(LayersByView[View].Contains).Concat(_layers.Where(x => !LayersByView[View].Contains(x)))];
            if (prefs.Ref is { } r && SindUniGlyphs.HouseReferences.Contains(r)) _ref = r;
            if (prefs.Lens is { } lens && Enum.TryParse<SindUniLens>(lens, out var parsed)) _lens = parsed;
            if (prefs.Lagnas is { } sl) _lagnas = [.. sl];
            _preset = Presets.FirstOrDefault(p => p.Value.ToHashSet().SetEquals(_layers.Where(LayersByView[View].Contains))).Key;
            StateHasChanged();
        }
        catch (JSException) { }
        catch (JsonException) { }
        catch (InvalidOperationException) { }
    }

    private sealed record Prefs(string[]? Layers, string? Ref, string? Lens, string[]? Lagnas);

    private string StorageKey => $"ikiastrro-sinduni-{ViewName}-{PrefsKey}";

    private async Task SavePrefs()
    {
        if (PrefsKey is null) return;
        try
        {
            var json = JsonSerializer.Serialize(new Prefs([.. _layers], _ref, _lens.ToString(), [.. _lagnas]));
            await JS.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (JSException) { }
        catch (InvalidOperationException) { }
    }

    private Task ToggleLayer(string key)
    {
        if (!_layers.Remove(key)) _layers.Add(key);
        _preset = null;
        return SavePrefs();
    }

    private Task ApplyPreset(string preset)
    {
        _layers = [.. Presets[preset]];
        _preset = preset;
        return SavePrefs();
    }

    private Task ToggleLagna(string code)
    {
        if (!_lagnas.Remove(code)) _lagnas.Add(code);
        return SavePrefs();
    }

    private Task OnRefChanged(ChangeEventArgs e)
    {
        _ref = e.Value?.ToString() ?? "Moon";
        return SavePrefs();
    }

    private async Task SetLens(SindUniLens lens)
    {
        _lens = lens;
        if (lens != SindUniLens.Off && PinnedSign is null)
            await PinnedSignChanged.InvokeAsync(Chart.Ascendant);
        await SavePrefs();
    }

    /// <summary>A Micro chart is static (All Charts cards) unless the page binds the pinned sign
    /// (Key Inference, Spl Lagnas): then it pins and shows the sign card in its centre too.</summary>
    private bool Interactive => View != SindUniView.Micro || PinnedSignChanged.HasDelegate;

    private async Task Pin(ZodiacName sign)
    {
        if (!Interactive) return;
        var next = PinnedSign == sign && _lens == SindUniLens.Off ? (ZodiacName?)null : sign;
        if (PinnedSignChanged.HasDelegate) await PinnedSignChanged.InvokeAsync(next);
        else PinnedSign = next;
    }

    private async Task Hover(ZodiacName sign)
    {
        _hover = sign;
        await PreviewSignChanged.InvokeAsync(sign);
    }

    private async Task ClearHover()
    {
        _hover = null;
        await PreviewSignChanged.InvokeAsync(null);
    }

    // ---- lens ----

    private readonly record struct LensMark(string Text, string Title, string Color);

    private LensMark? LensSourceFor(ZodiacName sign)
    {
        if (_lens == SindUniLens.Off || PinnedSign is not ZodiacName target || View == SindUniView.Micro) return null;
        if (_lens == SindUniLens.Argala)
        {
            var src = Chart[target].ArgalaSources.FirstOrDefault(s => s.SourceSign(target) == sign);
            if (src is null) return null;
            var kind = src.IsArgala ? "Argala" : "Virodhargala";
            return new LensMark($"{(src.IsArgala ? "+" : "−")}{src.Offset}",
                $"{kind} from the {Ordinal(src.Offset)}: {string.Join(", ", src.Planets)}", $"var(--{(src.IsArgala ? "arg" : "vir")})");
        }
        var aspecting = Chart[target].Aspectors.Where(p => Chart.PlanetSigns.GetValueOrDefault(p) == sign && Chart.PlanetSigns.ContainsKey(p)).ToList();
        return aspecting.Count == 0 ? null
            : new LensMark("ASP", $"Aspects {SindUniGlyphs.SignName(target)}: {string.Join(", ", aspecting)}", "var(--cosmos-accent)");
    }

    private IReadOnlySet<string> HotPlanets
    {
        get
        {
            if (_lens == SindUniLens.Off || PinnedSign is not ZodiacName target) return new HashSet<string>();
            return _lens == SindUniLens.Aspects
                ? Chart[target].Aspectors.ToHashSet()
                : Chart[target].ArgalaSources.SelectMany(s => s.Planets).ToHashSet();
        }
    }

    private IEnumerable<(ZodiacName From, string Color, string Dash)> LensLines(ZodiacName target)
    {
        if (_lens == SindUniLens.Argala)
            return Chart[target].ArgalaSources.Select(s => (s.SourceSign(target), s.IsArgala ? "arg" : "vir", s.IsArgala ? "" : "2 1.6"));
        return Chart[target].Aspectors
            .Where(p => Chart.PlanetSigns.ContainsKey(p))
            .Select(p => Chart.PlanetSigns[p]).Distinct()
            .Where(s => s != target)
            .Select(s => (s, "cosmos-accent", ""));
    }

    private string LensSummary(ZodiacName target) => _lens == SindUniLens.Argala
        ? $"{SindUniGlyphs.SignName(target)}: {Chart[target].ArgalaNet ?? "no Argala"}"
        : $"{SindUniGlyphs.SignName(target)} aspected by {(Chart[target].Aspectors.Count == 0 ? "nobody" : string.Join(", ", Chart[target].Aspectors.Select(SindUniGlyphs.PlanetCode)))}";

    private static (double, double, double, double) Segment(ZodiacName from, ZodiacName to)
    {
        static (double X, double Y) Centre(ZodiacName s) =>
            ((SindUniGlyphs.Position[s].Col - 0.5) * 25, (SindUniGlyphs.Position[s].Row - 0.5) * 25);
        var (sx, sy) = Centre(from);
        var (tx, ty) = Centre(to);
        var dx = tx - sx; var dy = ty - sy; var len = Math.Max(Math.Sqrt(dx * dx + dy * dy), 1);
        return (sx + dx / len * 5, sy + dy / len * 5, tx - dx / len * 6, ty - dy / len * 6);
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    // ---- text helpers ----

    private static string HouseLord(ZodiacName sign) => HouseEngine.GetSignLord(sign);

    private static string VerdictWord(string verdict) => verdict switch { "BEN" => "benefic", "MAL" => "malefic", _ => "mixed" };

    private static string Ordinal(int n) => n switch { 2 => "2nd", 3 => "3rd", _ => $"{n}th" };

    private static string SignSummary(ZodiacName s) =>
        $"{SindUniGlyphs.SignName(s)} · {SindUniGlyphs.ElementOf(s)} · {SindUniGlyphs.ModalityOf(s)} · {(SindUniGlyphs.IsOdd(s) ? "Odd" : "Even")} · Lord {HouseLord(s)}";

    private static string NakshatraSpan(ZodiacName s) => string.Join(", ", SindUniGlyphs.PadasIn(s)
        .GroupBy(p => p.NakshatraId)
        .Select(g => $"{SindUniGlyphs.NakshatraCode(g.Key)} {(g.Count() == 1 ? g.First().Pada.ToString(CultureInfo.InvariantCulture) : $"{g.First().Pada}–{g.Last().Pada}")}"));

    /// <summary>Points (the Lagna and planets) sitting in this nakshatra pada of the sign.</summary>
    private List<string> PointsIn(SindUniSign s, int nakshatraId, int pada)
    {
        var who = s.Planets.Where(p => p.NakshatraId == nakshatraId && p.Pada == pada).Select(p => p.Planet).ToList();
        if (s.Sign == Chart.Ascendant && Chart.AscendantNakshatraId == nakshatraId && Chart.AscendantPada == pada) who.Insert(0, "Lagna");
        return who;
    }

    private string DefaultMeta
    {
        get
        {
            var lagna = $"Lagna {SindUniGlyphs.SignName(Chart.Ascendant)} {Chart.AscendantDegree}";
            var moon = Chart.PlanetSigns.TryGetValue("Moon", out var m)
                ? $" · Moon {SindUniGlyphs.SignName(m)}{MoonNakshatra()}" : "";
            return lagna + moon;
        }
    }

    private string MoonNakshatra()
    {
        var moon = Chart.Signs.Values.SelectMany(s => s.Planets).FirstOrDefault(p => p.Planet == "Moon");
        return moon?.NakshatraId is int n ? $" · {SindUniGlyphs.NakshatraCode(n)} {moon.Pada}" : "";
    }

    private string CellLabel(SindUniSign s)
    {
        var parts = new List<string> { $"{SindUniGlyphs.SignName(s.Sign)}, house {s.HouseFromLagna}" };
        if (s.Verdict is not null) parts.Add(VerdictWord(s.Verdict));
        if (s.Planets.Count > 0) parts.Add(string.Join(", ", s.Planets.Select(p => p.Planet + (p.IsRetrograde && p.Planet is not ("Rahu" or "Ketu") ? " retrograde" : ""))));
        if (s.Sav is int sav) parts.Add($"{sav} bindus");
        return string.Join("; ", parts);
    }
}
