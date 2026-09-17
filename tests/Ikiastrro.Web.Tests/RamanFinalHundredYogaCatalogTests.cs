using Ikiastrro.Core.Engines.Yoga;
namespace Ikiastrro.Web.Tests;
// 245-263 (the Raja Yoga cluster) was transcribed out of this catalog on 2026-09-17 into its
// own RamanRajaYogaEvaluator (see RamanRajaYogaEvaluatorTests.cs) — the catalog now covers the
// remaining 81 of the 100 numbers in 201-300.
public sealed class RamanFinalHundredYogaCatalogTests
{
 [Fact] public void Catalog_Contains_The_Remaining_Eighty_One_Numbers_Exactly_Once()
 {
  var rows=RamanFinalHundredCatalog.Entries();
  Assert.Equal(81,rows.Count);
  Assert.Equal(81,rows.Select(x=>x.SourceVariantCode).Distinct().Count());
  var expected=Enumerable.Range(201,100).Where(n=>n is < 245 or > 263).Select(n=>$"RAMAN_300_{n:000}");
  Assert.Equal(expected,rows.Select(x=>x.SourceVariantCode));
 }
 [Fact] public void Catalog_Does_Not_Report_Unimplemented_Rules_As_Absent()
 {
  Assert.All(RamanFinalHundredCatalog.Entries(),row=>{Assert.Null(row.Present);Assert.Equal("NOT_EVALUATED",row.EvaluationStatus);});
 }
 [Fact] public void Catalog_No_Longer_Carries_The_Transcribed_Raja_Cluster()
 {
  Assert.DoesNotContain(RamanFinalHundredCatalog.Entries(),x=>x.YogaCode=="YOGA_RAJA");
 }
}
