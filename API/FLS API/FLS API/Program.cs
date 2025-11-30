using FLS_API.DL;
using FLS_API.BL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseServiceRoleKey = builder.Configuration["Supabase:ServiceRoleKey"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];
var supabaseOptions = new FLS_API.Models.SupabaseOptions
{
    Url = supabaseUrl,
    ServiceRoleKey = supabaseServiceRoleKey,
    AnonKey = supabaseAnonKey
};
builder.Services.AddSingleton(supabaseOptions);
builder.Services.AddScoped<SupabaseService>();

// Using real ChatbotService
builder.Services.AddScoped<IChatbotService, ChatbotService>();
<<<<<<< HEAD
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
=======
>>>>>>> 7de9a22b8b04ecf3b9c589f93a5d0147d2b2bb40
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FLS API",
        Version = "v1"
    });
    
    // Add support for file uploads (multipart/form-data)
    c.OperationFilter<FLS_API.Utilities.FileUploadOperationFilter>();
    
    // Map IFormFile to binary schema
    c.MapType<Microsoft.AspNetCore.Http.IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

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

app.UseAuthorization();

app.MapControllers();

app.Run();
