using Microsoft.EntityFrameworkCore;
using Test_Api.Context;

var builder = WebApplication.CreateBuilder(args);

// Gi? nguyên dòng này ?? Railway có th? map c?ng (port)
builder.WebHost.UseUrls("http://0.0.0.0:5018");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- S?A T?I ?ÂY ---

// 1. K?t n?i InternConnection (Chuy?n sang PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("InternConnection");
builder.Services.AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

// 2. K?t n?i CookingAppConnection (Chuy?n sang PostgreSQL)
var connectionCooking = builder.Configuration.GetConnectionString("CookingAppConnection");
builder.Services.AddDbContext<CookingDbContext>(options => options.UseNpgsql(connectionCooking));

var app = builder.Build();

// 3. Cho phép ch?y Swagger c? trên môi tr??ng Production (?? b?n d? test trên web)
// Xóa ho?c comment dòng check Environment.IsDevelopment
app.UseSwagger();
app.UseSwaggerUI();

// 4. T? ??NG T?O B?NG (MIGRATE) KHI CH?Y TRÊN SERVER
using (var scope = app.Services.CreateScope())
{
    // T?o b?ng cho UserDbContext
    var userDb = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    userDb.Database.Migrate();

    // T?o b?ng cho CookingDbContext
    var cookingDb = scope.ServiceProvider.GetRequiredService<CookingDbContext>();
    cookingDb.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();