using Microsoft.EntityFrameworkCore;
using Test_Api.Context;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5018");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("InternConnection");
builder.Services.AddDbContext<UserDbContext>(options =>options.UseSqlServer(connectionString));

var connectionCooking = builder.Configuration.GetConnectionString("CookingAppConnection");
builder.Services.AddDbContext<CookingDbContext>(options =>options.UseSqlServer(connectionCooking));
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
