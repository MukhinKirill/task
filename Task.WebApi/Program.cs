using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Task.DbModule.Data;

namespace Task.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDbContext<BaseContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnect"),
					b => b.MigrationsAssembly("Task.WebApi")));
			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

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
		}
	}
}
