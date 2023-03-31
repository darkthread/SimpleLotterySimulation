using System.Text;
using LotteryWebApi;
using LotteryWebApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dbDir);
var dbPath = Path.Combine(dbDir, "regdb.sqlite");
var cs = $"data source={dbPath}";

builder.Services.AddDbContext<RegDbContext>(options => options.UseSqlite(cs));
builder.Services.AddScoped<LotteryValidator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegDbContext>();
    db.Init();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
