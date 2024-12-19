
using Microsoft.AspNetCore.Mvc;

namespace PractissApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState.Values
						   .SelectMany(x => x.Errors)
						   .Select(x => x.ErrorMessage);
					return new BadRequestObjectResult(errors);
				};
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowSpecificOrigin",
					builder => builder.WithOrigins("https://localhost:7091", "https://practissweb.azurewebsites.net") // Specify the allowed origin
									  .AllowAnyMethod()
									  .AllowAnyHeader());
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}


			app.UseCors("AllowSpecificOrigin");

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
