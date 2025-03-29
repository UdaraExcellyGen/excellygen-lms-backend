using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();