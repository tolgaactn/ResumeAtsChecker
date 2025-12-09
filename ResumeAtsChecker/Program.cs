using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ResumeAtsChecker.Data;
using ResumeAtsChecker.Services;
using ResumeAtsChecker.Services.Concretes;
using ResumeAtsChecker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// CORS ekle (Frontend için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IAiService, OpenAiService>();
builder.Services.AddScoped<IPdfService, PdfService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS'u kullan
app.UseCors("AllowFrontend");

// Static files ekle (YENI SATIRLAR)
app.UseDefaultFiles();  // index.html'i otomatik açar
app.UseStaticFiles();   // wwwroot'daki dosyaları serve eder

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();