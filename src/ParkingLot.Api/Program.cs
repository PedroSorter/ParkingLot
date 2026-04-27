using System.Text.Json.Serialization;
using Npgsql;
using ParkingLot.Core;
using ParkingLot.Infrastructure.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ParkingLotWeb", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("ParkingLot")
    ?? throw new InvalidOperationException("Connection string 'ParkingLot' is required.");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<IParkingLotRepository, PostgresParkingLotRepository>();

var app = builder.Build();

await PostgresDatabaseMigrator.ApplyMigrationsAsync(app.Services.GetRequiredService<NpgsqlDataSource>());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ParkingLotWeb");

app.MapControllers();

app.Run();
