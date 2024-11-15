using Microsoft.EntityFrameworkCore;
using Task.Connector;
using Task.Integration.Data.Models;

var builder = WebApplication.CreateBuilder(args);
string postgreConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=241977;Database=testDb";
// Add services to the container.
builder.Services.AddControllers();

// Регистрируем зависимости для ConnectorDb
builder.Services.AddScoped<IConnector, ConnectorDb>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgreConnectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
