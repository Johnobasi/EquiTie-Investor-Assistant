using EquiTie.Api;
using EquiTie.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

// Register concrete classes directly — no interfaces needed for a prototype
builder.Services.AddSingleton<CsvDataRepository>();
builder.Services.AddSingleton<PortfolioService>();
builder.Services.AddSingleton<PromptBuilder>();
builder.Services.AddHttpClient();
builder.Services.AddTransient<AnthropicChatService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Warm up the data repository once at startup
app.Services.GetRequiredService<CsvDataRepository>();

app.UseCors();
app.MapControllers();
app.Run();