using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace Ikiastrro.Web.Tests;

/// <summary>
/// Text of rendered markup as a reader sees it: tags removed, entities decoded, and every run of whitespace (the newlines and
/// indentation Razor keeps from the .razor source) collapsed to one space. Assert on this, never on raw <c>TextContent</c> or
/// <c>Markup</c>, whenever the expected text spans words that the component may wrap across lines or put in different
/// elements (<c>&lt;b&gt;Name&lt;/b&gt; (detail)</c>): wrapping and inline tags are layout details, not behaviour.
/// <code>Assert.Contains("is Scorpio", cut.Find(".lead").Normalized());</code>
/// <code>Assert.Contains("Ashtottari dasa (PVR ch.17)", cut.Markup.Normalized());</code>
/// </summary>
public static class MarkupText
{
    public static string Normalized(this IElement element) => Collapse(element.TextContent);

    public static string Normalized(this string markupOrText) =>
        Collapse(WebUtility.HtmlDecode(Regex.Replace(markupOrText, "<[^>]+>", " ")));

    private static string Collapse(string text) => Regex.Replace(text, @"\s+", " ").Trim();
}
