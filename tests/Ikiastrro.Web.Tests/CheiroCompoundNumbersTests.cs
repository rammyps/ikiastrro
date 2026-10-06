using Ikiastrro.Core.Numerology;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class CheiroCompoundNumbersTests
{
    [Fact]
    public void Covers_every_number_from_10_to_52()
    {
        Assert.Equal(Enumerable.Range(10, 43), CheiroCompoundNumbers.All.Select(m => m.Number));
    }

    [Fact]
    public void Every_pointer_resolves_to_an_entry_with_its_own_meaning()
    {
        foreach (var m in CheiroCompoundNumbers.All.Where(m => m.SameAs is not null))
            Assert.Null(CheiroCompoundNumbers.Get(m.SameAs!.Value)!.SameAs);
    }

    [Fact]
    public void Resolve_follows_same_as_and_out_of_range_has_no_meaning()
    {
        Assert.Equal(24, CheiroCompoundNumbers.Resolve(42)!.Number);
        Assert.Equal(43, CheiroCompoundNumbers.Resolve(52)!.Number);
        Assert.Null(CheiroCompoundNumbers.Get(9));
        Assert.Null(CheiroCompoundNumbers.Get(53));
    }
}
