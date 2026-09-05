using Microsoft.Data.SqlClient;

namespace Ikiastrro.Data;

public class SqlConnectionFactory
{
    private const string DefaultDb = "ikiastrro";

    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    /// <summary>
    /// Resolves the connection string, in order:
    /// 1. <paramref name="connectionString"/> if given (Web passes ConnectionStrings:Ikiastrro);
    /// 2. env var <c>IKIASTRRO_CONNECTION</c> (stage/uat/prod);
    /// 3. a Windows-Auth string against <c>localhost\SQLSERVER2025</c> (this dev machine's actual
    ///    instance name — plain <c>localhost</c> only resolves the *default* instance, and this
    ///    machine's SQL Server is a named one; found 2026-09-05 when the Web app 500'd on every
    ///    page with "Error Number:2" the moment ConnectionStrings:Ikiastrro fell back to this
    ///    default, e.g. under IIS Express/Visual Studio's F5 rather than the CLI's own override),
    ///    catalog = <paramref name="dbNameOverride"/> (CLI <c>--db</c>) ?? env <c>IKIASTRRO_DB</c>
    ///    ?? <c>ikiastrro</c>.
    /// No environment token ever appears in a schema object name — only the catalog / server
    /// differs per environment (see INFRASTRUCTURE.md).
    /// </summary>
    public static SqlConnectionFactory Create(string? connectionString = null, string? dbNameOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            return new SqlConnectionFactory(connectionString);

        var env = Environment.GetEnvironmentVariable("IKIASTRRO_CONNECTION");
        if (!string.IsNullOrWhiteSpace(env))
            return new SqlConnectionFactory(env);

        var db = dbNameOverride
                 ?? Environment.GetEnvironmentVariable("IKIASTRRO_DB")
                 ?? DefaultDb;
        return new SqlConnectionFactory(
            $@"Server=localhost\SQLSERVER2025;Database={db};Integrated Security=True;TrustServerCertificate=True;");
    }

    /// <summary>Back-compat: the historical default (Windows Auth, localhost\SQLSERVER2025, <c>ikiastrro</c>).</summary>
    public static SqlConnectionFactory CreateDefault() => Create();

    public SqlConnection CreateOpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
