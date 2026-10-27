using System.Text.Json;
using TestReader.Api.Endpoints;
using TestReader.Application;
using TestReader.DataBase;
using TestReader.Domain.Entities;
using TestReader.Domain.Services;
using TestReader.Domain.Validation;
using TestReader.Infrastructure.FileSystem;
using TestReader.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();


var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=sensor.db";


builder.Services.AddSingleton<IReadingRepository>(_ => new SqliteReadingRepository(cs));
builder.Services.AddSingleton<IJsonlReader, JsonlReader>();
builder.Services.AddSingleton<ReadingValidator>();
builder.Services.AddSingleton<RuleEngine>();
builder.Services.AddSingleton<SustainedAboveEngine>();
builder.Services.AddSingleton<IngestionService>();

var app = builder.Build();


DatabaseInitializer.Initialize(cs);


var rulesPath = Path.Combine(AppContext.BaseDirectory, "rules.json");
var rules = JsonSerializer.Deserialize<List<Rule>>(File.ReadAllText(rulesPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];


var readingsPath = Path.Combine(AppContext.BaseDirectory, "readings.jsonl");
var ingestion = app.Services.GetRequiredService<IngestionService>();
var report = await ingestion.ProcessFileAsync(readingsPath, rules);


Console.WriteLine($"Total: {report.TotalLinesRead}, Parsed: {report.ParsedReadings}, Stored: {report.StoredReadings}");
Console.WriteLine($"Acceptable: {report.AcceptableReadings}, Unacceptable: {report.UnacceptableReadings}, Alerts: {report.AlertsGenerated}");



app.MapAggregate(cs);
app.MapDBCheck(cs);

app.Run();