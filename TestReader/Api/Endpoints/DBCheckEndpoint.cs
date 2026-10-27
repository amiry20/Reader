using Dapper;
using Microsoft.Data.Sqlite;

namespace TestReader.Api.Endpoints;

public static class DBCheckEndpoint
{
    public static void MapDBCheck(this WebApplication app, string cs)
    {
        app.MapGet("/debug/count", () =>
        {
            using var conn = new SqliteConnection(cs);
            conn.Open();
            var total = conn.ExecuteScalar<long>("SELECT COUNT(*) FROM Readings");
            var acceptable = conn.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM Readings WHERE IsAcceptable = 1");
            return Results.Ok(new { total, acceptable });
        });
    }
}
