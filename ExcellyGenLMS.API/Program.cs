// Necessary using statements
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders; // For PhysicalFileProvider (Static Files)
using Microsoft.AspNetCore.Identity;       // For PasswordHasher
using Microsoft.OpenApi.Models;           // For Swagger
using System.Text;                        // For Encoding
using System;                               // For Exception, Path, etc.
using System.IO;                            // For Path, Directory
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
<<<<<<< HEAD

// Infrastructure Layer Namespaces
using ExcellyGenLMS.Infrastructure.Data;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo;
using ExcellyGenLMS.Infrastructure.Services.Auth;
using ExcellyGenLMS.Infrastructure.Services.Storage;

// Application Layer Namespaces
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Application.Services.Auth;
using ExcellyGenLMS.Application.Services.Admin;
using ExcellyGenLMS.Application.Services.CourseSvc;

// Core Layer Namespaces
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;

// API Layer Namespaces
using ExcellyGenLMS.API.Middleware; // Assuming this contains UseRoleAuthorization extension
=======
using ExcellyGenLMS.Infrastructure.Data; // Base DbContext namespace
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth; // Auth Repo implementations
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Application.Services.Auth; // Auth Service implementations
using ExcellyGenLMS.Infrastructure.Services.Auth; // Infrastructure Services (e.g., Firebase Auth Service)
using Microsoft.OpenApi.Models;
using ExcellyGenLMS.API.Middleware; // Your custom middleware namespace
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Services.Admin; // Admin Service implementations
using Microsoft.AspNetCore.Identity;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin; // Admin Repo implementations
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Infrastructure.Services.Common; // Common Services (e.g., FileService)
using Microsoft.Extensions.FileProviders;
using System.IO;

// Learner Module Interface imports (Core and Application)
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;

// Project Manager Module Interface imports
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;

// Learner Module Implementation imports (Infrastructure and Application)
using ExcellyGenLMS.Infrastructure.Data.Repositories.Learner;
using ExcellyGenLMS.Application.Services.Learner;

// Project Manager Module Implementation imports
using ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager;
using ExcellyGenLMS.Application.Services.ProjectManager;

// Added for Analytics functionality
using System.Data;
using Microsoft.Data.SqlClient;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Application.Services.Course;
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // For convenience
var environment = builder.Environment; // For convenience

// --- Basic Logging Setup ---
var loggerFactory = LoggerFactory.Create(logBuilder =>
{
    logBuilder.AddConfiguration(configuration.GetSection("Logging"));
    logBuilder.AddConsole();
    logBuilder.AddDebug();
});
var logger = loggerFactory.CreateLogger<Program>();


<<<<<<< HEAD
// Add DbContext configuration
logger.LogInformation("Configuring DbContext.");
=======
// --- Add DbContext configuration ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

<<<<<<< HEAD
// Add CORS with specific localhost origins
logger.LogInformation("Configuring CORS.");
=======
// --- Add IDbConnection for Dapper ---
builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Add CORS ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
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

<<<<<<< HEAD
// Initialize Firebase Admin
logger.LogInformation("Initializing Firebase Admin SDK.");
=======
// --- Enhanced Firebase Admin Configuration ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];
        if (string.IsNullOrEmpty(serviceAccountKeyPath))
        {
<<<<<<< HEAD
            serviceAccountKeyPath = "firebase-service-account.json"; // Fallback
=======
            serviceAccountKeyPath = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");
            if (!System.IO.File.Exists(serviceAccountKeyPath) && !string.IsNullOrEmpty(builder.Environment.ContentRootPath))
            {
                serviceAccountKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
            }
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        }
        logger.LogInformation("Using Firebase service account key at: {Path}", serviceAccountKeyPath);

<<<<<<< HEAD
        if (!File.Exists(serviceAccountKeyPath))
=======
        Console.WriteLine($"Attempting to use service account key at: {serviceAccountKeyPath}");

        if (!System.IO.File.Exists(serviceAccountKeyPath))
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        {
            throw new FileNotFoundException($"Firebase service account file not found at the resolved path: {serviceAccountKeyPath}. Ensure the file exists or the 'Firebase:ServiceAccountKeyPath' in appsettings.json is correct.");
        }

        // Initialize Firebase Admin with enhanced configuration
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
            ProjectId = builder.Configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500"
        });
<<<<<<< HEAD
        logger.LogInformation("Firebase Admin SDK initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing Firebase Admin.");
=======

        Console.WriteLine("Firebase Admin SDK initialized successfully.");

        // Set up default application credentials for Google Cloud Storage
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);
        Console.WriteLine($"Set GOOGLE_APPLICATION_CREDENTIALS environment variable: {serviceAccountKeyPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"CRITICAL: Error initializing Firebase Admin: {ex.Message}");
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        if (ex.InnerException != null)
        {
            logger.LogError(ex.InnerException, "Inner exception during Firebase Admin initialization.");
        }
<<<<<<< HEAD

        if (environment.IsDevelopment())
        {
            logger.LogWarning("Creating default Firebase app for development due to initialization error...");
            try
            {
                FirebaseApp.Create(new AppOptions { ProjectId = "excelly-lms-f3500" });
                logger.LogInformation("Default Firebase app created for development.");
            }
            catch (Exception devEx)
            {
                logger.LogError(devEx, "Failed to create development Firebase app.");
            }
        }
    }
}

// Setup JWT Authentication
logger.LogInformation("Configuring JWT Authentication.");
var jwtKey = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
var jwtAudience = configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";
=======
    }
}


// --- Setup JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93

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
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            logger.LogWarning(context.Exception, "JWT Authentication failed.");
            return Task.CompletedTask;
        }
    };
});

<<<<<<< HEAD
logger.LogInformation("Registering application services and repositories.");
// Register IPasswordHasher service
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register Auth repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register Auth services
=======
// --- Register IPasswordHasher ---
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// --- Register repositories ---
// Auth repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Admin repositories
builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
builder.Services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();

// Learner repositories
builder.Services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
builder.Services.AddScoped<IUserTechnologyRepository, UserTechnologyRepository>();
builder.Services.AddScoped<IUserProjectRepository, UserProjectRepository>();
builder.Services.AddScoped<IUserCertificationRepository, UserCertificationRepository>();
builder.Services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
builder.Services.AddScoped<IThreadCommentRepository, ThreadCommentRepository>();
builder.Services.AddScoped<IThreadComReplyRepository, ThreadComReplyRepository>();

// Course repositories
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// ProjectManager repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// --- Register services ---
// Auth services
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

<<<<<<< HEAD
// Register Admin Services and Repositories
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
builder.Services.AddScoped<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
// If ICourseAdminRepository and ICourseAdminService were intended, they should be here.
// e.g., builder.Services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();
// e.g., builder.Services.AddScoped<ICourseAdminService, CourseAdminService>();


// Register Course Module Repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();

// Register File Storage Service
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Register Course Module Services
builder.Services.AddScoped<ICourseService, CourseService>();

// Add controllers
//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // You might also want to ensure camelCase for property names if not already set:
        // options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();


// Add Swagger
logger.LogInformation("Configuring Swagger.");
=======
// Admin services
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
builder.Services.AddScoped<ICourseAdminService, CourseAdminService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add Analytics Service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Course services
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Learner services
builder.Services.AddScoped<IUserBadgeService, UserBadgeService>();
builder.Services.AddScoped<IUserTechnologyService, UserTechnologyService>();
builder.Services.AddScoped<IUserProjectService, UserProjectService>();
builder.Services.AddScoped<IUserCertificationService, UserCertificationService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<IFileService, FileService>();

// ProjectManager services
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IProjectService, ExcellyGenLMS.Application.Services.ProjectManager.ProjectService>();
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IRoleService, ExcellyGenLMS.Application.Services.ProjectManager.RoleService>();
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IPMTechnologyService, ExcellyGenLMS.Application.Services.ProjectManager.PMTechnologyService>();

// --- Required for accessing HttpContext ---
builder.Services.AddHttpContextAccessor();

// --- Add controllers ---
builder.Services.AddControllers();

// --- Add Swagger ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExcellyGenLMS API", Version = "v1" });

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

    // FIX for Schema ID collision: Use full type name for schema IDs.
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

<<<<<<< HEAD
logger.LogInformation("Building the application.");
var app = builder.Build();

// Configure the HTTP request pipeline
logger.LogInformation("Configuring the HTTP request pipeline.");
=======
// --- Build the Application ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
<<<<<<< HEAD
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
=======
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1"));
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
}
else
{
    app.UseExceptionHandler("/error"); // Consider adding a real error handling endpoint
    app.UseHsts();
}


app.UseHttpsRedirection();

<<<<<<< HEAD
// --- Static File Serving ---
logger.LogInformation("Configuring static file serving.");
string fileStoragePath = configuration.GetValue<string>("FileStorage:LocalPath") ??
    Path.Combine(environment.ContentRootPath, "uploads");

if (!Directory.Exists(fileStoragePath))
{
    try
    {
        Directory.CreateDirectory(fileStoragePath);
        logger.LogInformation("Created file storage directory: {Path}", fileStoragePath);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create file storage directory: {Path}", fileStoragePath);
    }
}

var wwwrootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
var fullFileStoragePath = Path.GetFullPath(fileStoragePath);
var fullWwwRootPath = Path.GetFullPath(wwwrootPath);

if (!fullFileStoragePath.StartsWith(fullWwwRootPath, StringComparison.OrdinalIgnoreCase))
{
    logger.LogInformation("Configuring static files from custom path: {Path} mapped to /uploads", fileStoragePath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fileStoragePath),
        RequestPath = "/uploads"
    });
}
else
{
    logger.LogInformation("Custom file storage path {Path} is within wwwroot. Standard static file serving will apply.", fileStoragePath);
}

app.UseStaticFiles(); // Serves files from wwwroot

// --- Middleware Order ---
app.UseRouting();

=======
// --- Ensure wwwroot directory exists and configure static files ---
var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    try
    {
        Directory.CreateDirectory(webRootPath);
        Console.WriteLine($"Created wwwroot directory at: {webRootPath}");

        var uploadsPath = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        Directory.CreateDirectory(Path.Combine(uploadsPath, "avatars"));
        Directory.CreateDirectory(Path.Combine(uploadsPath, "badges"));
        Directory.CreateDirectory(Path.Combine(uploadsPath, "certifications"));
        Directory.CreateDirectory(Path.Combine(uploadsPath, "forum")); // Optional forum image folder

        Console.WriteLine("Created upload subdirectories");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating wwwroot subdirectories: {ex.Message}");
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = ""
});

// --- Apply Middleware ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

<<<<<<< HEAD
app.UseRoleAuthorization(); // Custom middleware

app.MapControllers();

logger.LogInformation("Application startup complete. Running.");
=======
app.UseRoleAuthorization(); // Your custom role check middleware

app.MapControllers();

// --- Run the Application ---
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
app.Run();