using Dapper;

namespace Ikiastrro.Data;

/// <summary>One relative of a person, with the role as seen from that person ("Wife", "Daughter", ...).</summary>
public sealed record FamilyMember(
    int RelativeId, string RelativeName, string? RelativeSex, string Role, string Tier, bool IsAdopted, bool IsStep, string Status);

/// <summary>A recorded married couple: the man (left) and the woman (right), by recorded sex, else the lower id first.</summary>
public sealed record FamilyCouple(int GroomId, string GroomName, int BrideId, string BrideName, string Status);

/// <summary>A pair in the Extended or Lateral layer (grandparent and grandchild, or siblings): the elder on the left.
/// <see cref="Relation"/> says who the left person is to the right ("Paternal Grandfather", "Siblings").</summary>
public sealed record FamilyExtendedPair(int LeftId, string LeftName, int RightId, string RightName, string Relation, string Layer);

/// <summary>
/// Reads vw_PersonFamily (migration 168): the core family from the stored SPOUSE and PARENT_OF edges
/// and siblings derived from a shared parent. Writes only direct edges; nothing else is stored, so a
/// tier or a sibling can never go stale.
/// </summary>
public sealed class FamilyRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public FamilyRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <summary>Every relative of the person, Core first, then Extended, then Lateral, then by role and name.</summary>
    public IReadOnlyList<FamilyMember> GetFor(int personId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<FamilyMember>("""
            SELECT RelativeId, RelativeName, RelativeSex, Role, Tier, IsAdopted, IsStep, Status
            FROM dbo.vw_PersonFamily
            WHERE PersonId = @PersonId
            ORDER BY CASE Tier WHEN 'Core' THEN 0 WHEN 'Extended' THEN 1 ELSE 2 END, Role, RelativeName
            """, new { PersonId = personId }).ToList();
    }

    /// <summary>Records a married couple (stored once, lower id first). No-op if already recorded.</summary>
    public void AddSpouses(int personId, int otherId)
    {
        var (a, b) = personId < otherId ? (personId, otherId) : (otherId, personId);
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("""
            IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Person_Relationship WHERE PersonAId = @A AND PersonBId = @B AND RelationTypeCode = 'SPOUSE')
                INSERT dbo.tbl_Person_Relationship (PersonAId, PersonBId, RelationTypeCode) VALUES (@A, @B, 'SPOUSE');
            """, new { A = a, B = b });
    }

    /// <summary>Records <paramref name="parentId"/> as a parent of <paramref name="childId"/>. No-op if already recorded.</summary>
    public void AddParent(int parentId, int childId, bool isAdopted = false, bool isStep = false)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("""
            IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Person_Relationship WHERE PersonAId = @Parent AND PersonBId = @Child AND RelationTypeCode = 'PARENT_OF')
                INSERT dbo.tbl_Person_Relationship (PersonAId, PersonBId, RelationTypeCode, IsAdopted, IsStep)
                VALUES (@Parent, @Child, 'PARENT_OF', @IsAdopted, @IsStep);
            """, new { Parent = parentId, Child = childId, IsAdopted = isAdopted, IsStep = isStep });
    }

    /// <summary>Every recorded SPOUSE edge, with the man on the left and the woman on the right.</summary>
    public IReadOnlyList<FamilyCouple> GetCouples()
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<(int AId, string AName, string? ASex, int BId, string BName, string Status)>("""
            SELECT r.PersonAId AS AId, a.Name AS AName, a.Sex AS ASex, r.PersonBId AS BId, b.Name AS BName, r.Status
            FROM dbo.tbl_Person_Relationship r
            JOIN dbo.tbl_BirthDetails a ON a.Id = r.PersonAId
            JOIN dbo.tbl_BirthDetails b ON b.Id = r.PersonBId
            WHERE r.RelationTypeCode = 'SPOUSE'
            ORDER BY a.Name, b.Name
            """).Select(r => string.Equals(r.ASex, "Female", StringComparison.OrdinalIgnoreCase)
                ? new FamilyCouple(r.BId, r.BName, r.AId, r.AName, r.Status)
                : new FamilyCouple(r.AId, r.AName, r.BId, r.BName, r.Status)).ToList();
    }

    /// <summary>Grandparent-grandchild and sibling pairs, one row per pair, the elder on the left.</summary>
    public IReadOnlyList<FamilyExtendedPair> GetExtendedPairs()
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var rows = connection.Query<(int PersonId, string PersonName, DateTime PersonDob, int RelativeId, string RelativeName, DateTime RelativeDob, string Role, string Tier)>("""
            SELECT f.PersonId, p.Name AS PersonName, p.DateOfBirth AS PersonDob,
                   f.RelativeId, f.RelativeName, r.DateOfBirth AS RelativeDob, f.Role, f.Tier
            FROM dbo.vw_PersonFamily f
            JOIN dbo.tbl_BirthDetails p ON p.Id = f.PersonId
            JOIN dbo.tbl_BirthDetails r ON r.Id = f.RelativeId
            WHERE f.Tier IN ('Extended', 'Lateral')
            """).ToList();

        var result = new List<FamilyExtendedPair>();
        foreach (var r in rows)
        {
            if (r.Role.Contains("Grandfather") || r.Role.Contains("Grandmother"))
                result.Add(new FamilyExtendedPair(r.RelativeId, r.RelativeName, r.PersonId, r.PersonName, r.Role, r.Tier));
            else if (r.Tier == "Lateral" && r.PersonId < r.RelativeId)
            {
                var personFirst = r.PersonDob < r.RelativeDob;
                result.Add(personFirst
                    ? new FamilyExtendedPair(r.PersonId, r.PersonName, r.RelativeId, r.RelativeName, "Siblings", r.Tier)
                    : new FamilyExtendedPair(r.RelativeId, r.RelativeName, r.PersonId, r.PersonName, "Siblings", r.Tier));
            }
        }
        return result.OrderBy(x => x.Relation == "Siblings").ThenBy(x => x.LeftName).ThenBy(x => x.RightName).ToList();
    }
}

