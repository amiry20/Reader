namespace TestReader.DataBase
{
    using Dapper;
    using Microsoft.Data.Sqlite;

    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            conn.Execute(@"
    CREATE TABLE IF NOT EXISTS Readings (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        DeviceId TEXT NOT NULL,
        Metric TEXT NOT NULL,
        TsMs INTEGER NOT NULL,                  
        Value REAL NOT NULL,
        Seq INTEGER NOT NULL,
        IsAcceptable INTEGER NOT NULL,
        ViolatedRuleId TEXT,
        ViolationReason TEXT,
        UNIQUE(DeviceId, Metric, TsMs, Seq)
    );

    CREATE INDEX IF NOT EXISTS idx_readings_agg 
        ON Readings(DeviceId, Metric, IsAcceptable, TsMs);

    CREATE TABLE IF NOT EXISTS Alerts (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        RuleId TEXT NOT NULL,
        DeviceId TEXT NOT NULL,
        Metric TEXT NOT NULL,
        StartTsMs INTEGER NOT NULL,             
        EndTsMs INTEGER NOT NULL,               
        PeakValue REAL NOT NULL,
        UNIQUE(RuleId, DeviceId, Metric, StartTsMs)
    );
");
        }
    }
}
