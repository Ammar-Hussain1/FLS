using FLS_API.BL;
using FLS_API.DL;
using FLS_API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Supabase Client
builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("Supabase"));

// Register SupabaseService
builder.Services.AddSingleton<SupabaseService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWPF", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWPF");

app.MapControllers();

app.Run();
