using Microsoft.AspNetCore.Components;

namespace Ikiastrro.Web.Components;

/// <summary>
/// The person currently "opened" in the v2 shell. Scoped = one instance per Blazor Server
/// circuit, so it survives navigation between the per-person pages
/// (<c>docs/ui/wkstream_UI_v2.md</c> — "The nav tabs are hidden until a person is opened").
///
/// Every per-person page calls <see cref="Set"/> in <c>OnParametersSet</c> from its own route
/// parameter, so a cold deep link (<c>/transit-wheel/42</c>) populates the shell just as
/// picking a person on Home does. <see cref="MainLayout"/> is the only reader — it subscribes
/// to <see cref="Changed"/> and re-renders the header tabs and context band.
/// </summary>
public sealed class ActivePerson
{
    public int? Id { get; private set; }
    public string? Name { get; private set; }
    public string? BirthLine { get; private set; }

    /// <summary>Page-supplied extra content for the right side of <c>MainLayout</c>'s
    /// context band (e.g. Key Inference's master step rail). Every other page that calls
    /// <see cref="Set"/> must also call <see cref="SetHeaderExtra"/> with <c>null</c>
    /// alongside it, so this doesn't stay populated after navigating away from the page that
    /// set it.</summary>
    public RenderFragment? HeaderExtra { get; private set; }

    /// <summary>Raised only when a field actually changes, so the layout is not re-rendered on
    /// every navigation to the same person.</summary>
    public event Action? Changed;

    public void Set(int id, string name, string? birthLine = null)
    {
        if (Id == id && Name == name && BirthLine == birthLine) return;
        Id = id;
        Name = name;
        BirthLine = birthLine;
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (Id is null && Name is null && BirthLine is null && HeaderExtra is null) return;
        Id = null;
        Name = null;
        BirthLine = null;
        HeaderExtra = null;
        Changed?.Invoke();
    }

    public void SetHeaderExtra(RenderFragment? fragment)
    {
        HeaderExtra = fragment;
        Changed?.Invoke();
    }
}
