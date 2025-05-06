using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ExcellyGenLMS.Infrastructure.Data;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Application.Services.Auth;
using ExcellyGenLMS.Infrastructure.Services.Auth;
using Microsoft.OpenApi.Models;
using ExcellyGenLMS.API.Middleware;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Services.Admin;
using Microsoft.AspNetCore.Identity;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Infrastructure.Services.Common;
using Microsoft.Extensions.FileProviders;
using System.IO;

// Learner Module imports
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Application.Services.Learner;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS with specific localhost origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policyBuilder =>
        {
            policyBuilder
                .WithOrigins(
                    "http://localhost:5173",  // Vite development server
                    "http://localhost:3000",  // React standard port
                    "https://excelly-lms-f3500.web.app"  // Production
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Initialize Firebase Admin
if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        var serviceAccountKeyPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
        if (string.IsNullOrEmpty(serviceAccountKeyPath))
        {
            // Fallback to default location
            serviceAccountKeyPath = "firebase-service-account.json";
        }

        Console.WriteLine($"Using service account key at: {serviceAccountKeyPath}");

        if (!System.IO.File.Exists(serviceAccountKeyPath))
        {
            throw new FileNotFoundException($"Service account file not found at {serviceAccountKeyPath}");
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(serviceAccountKeyPath)
        });

        Console.WriteLine("Firebase Admin SDK initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing Firebase Admin: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }

        // Instead of failing, create a dummy Firebase app for development if needed
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("Creating default Firebase app for development...");
            try
            {
                FirebaseApp.Create(new AppOptions
                {
                    ProjectId = "excelly-lms-f3500"
                });
                Console.WriteLine("Default Firebase app created for development.");
            }
            catch (Exception devEx)
            {
                Console.WriteLine($"Failed to create development Firebase app: {devEx.Message}");
            }
        }
    }
}

// Setup JWT Authentication
var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Reduce the default clock skew of 5 minutes
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// Register IPasswordHasher service that UserService depends on
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Register User Management Service
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Register Tech Management Service
builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
builder.Services.AddScoped<ITechnologyService, TechnologyService>();

// Register CourseCategory repositories and services
builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();

// Register Course Admin repositories and services
builder.Services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();
builder.Services.AddScoped<ICourseAdminService, CourseAdminService>();

// Register Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register Learner repositories
builder.Services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
builder.Services.AddScoped<IUserTechnologyRepository, UserTechnologyRepository>();
builder.Services.AddScoped<IUserProjectRepository, UserProjectRepository>();
builder.Services.AddScoped<IUserCertificationRepository, UserCertificationRepository>();

// Register Learner services
builder.Services.AddScoped<IUserBadgeService, UserBadgeService>();
builder.Services.AddScoped<IUserTechnologyService, UserTechnologyService>();
builder.Services.AddScoped<IUserProjectService, UserProjectService>();
builder.Services.AddScoped<IUserCertificationService, UserCertificationService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Register the FileService
builder.Services.AddScoped<IFileService, FileService>();

// Required for file uploads and URL generation
builder.Services.AddHttpContextAccessor();

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExcellyGenLMS API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure wwwroot directory exists
var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
    Console.WriteLine($"Created wwwroot directory at: {webRootPath}");

    // Create uploads subdirectories
    var uploadsPath = Path.Combine(webRootPath, "uploads");
    Directory.CreateDirectory(uploadsPath);

    // Create specific upload folders
    Directory.CreateDirectory(Path.Combine(uploadsPath, "avatars"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "badges"));
    Directory.CreateDirectory(Path.Combine(uploadsPath, "certifications"));

    Console.WriteLine("Created upload subdirectories");
}

// Configure static files options
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = ""
});

// Apply CORS middleware BEFORE authentication middleware
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();
app.UseRoleAuthorization();
app.MapControllers();

app.Run();