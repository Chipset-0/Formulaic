using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

//Remove escaped ! characters from connection string
var connString = builder.Configuration["DB_CONNECTION_STRING"].Replace("\\!","!");


builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
    NpgsqlDataSource.Create(connString));

var app = builder.Build();

app.MapGet("/", async (NpgsqlDataSource db) =>
{
    await using var cmd = db.CreateCommand(
        "SELECT COALESCE(jsonb_agg(to_jsonb(c)), '[]'::jsonb)::text FROM Calculations c"
    );
    
    var json = (string?)await cmd.ExecuteScalarAsync();
    return Results.Content(json ?? "[]", "application/json");
});
 

app.Run();