// ExcellyGenLMS.API/Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// Infrastructure Project Usings (for DbContext and Repository Implementations)
using ExcellyGenLMS.Infrastructure.Data;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Course; // Unified Course Repositories Namespace
using ExcellyGenLMS.Infrastructure.Data.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager;
using ExcellyGenLMS.Infrastructure.Services.Auth;
using ExcellyGenLMS.Infrastructure.Services.Common;
using ExcellyGenLMS.Infrastructure.Services.Storage;


// Core Project Usings (for Interfaces and Entities)
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;

// Application Project Usings (for Service Interfaces and Implementations)
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Application.Services.Auth;
using ExcellyGenLMS.Application.Services.Admin;
using ExcellyGenLMS.Application.Services.Course;
using ExcellyGenLMS.Application.Services.Learner;
using ExcellyGenLMS.Application.Services.ProjectManager;


// API Project Usings (for Middleware and Controllers)
using ExcellyGenLMS.API.Middleware;
using ExcellyGenLMS.API.Controllers.Admin; // For Admin controllers
using ExcellyGenLMS.API.Controllers.Course; // For Course controllers
using ExcellyGenLMS.API.Controllers.Learner; // For Learner controllers (LearnerStatsController will be here)
using ExcellyGenLMS.API.Controllers.Auth; // For Auth controllers
using ExcellyGenLMS.API.Controllers.ProjectManager; // For ProjectManager controllers


var builder = WebApplication.CreateBuilder(args);

// === DATABASE CONFIGURATION ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CORS CONFIGURATION ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policyBuilder =>
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

// === FIREBASE CONFIGURATION ===
ConfigureFirebase(builder);

// === JWT AUTHENTICATION CONFIGURATION ===
ConfigureJwtAuthentication(builder);

// === PASSWORD HASHER ===
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// === REPOSITORY REGISTRATIONS ===
RegisterRepositories(builder.Services);

// === SERVICE REGISTRATIONS ===
RegisterServices(builder.Services);

// === CONTROLLERS AND SWAGGER ===
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    // Explicitly add assemblies for where controllers reside
    // Ensure all controller namespaces are added here
    .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Admin.CourseCategoriesController).Assembly) // Example: Admin categories
    .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Admin.DashboardController).Assembly) // Admin Dashboard
    .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Course.CoursesController).Assembly) // Course management
    .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Learner.LearnerStatsController).Assembly) // NEW: Learner Stats
                                                                                                       // Continue adding other controller assemblies as needed (Auth, ProjectManager, other Learner/Admin/Course controllers)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
ConfigureSwagger(builder.Services);

// === BUILD APPLICATION ===
var app = builder.Build();

// === CONFIGURE HTTP PIPELINE ===
ConfigureHttpPipeline(app, builder);

// === RUN THE APPLICATION ===
app.Run();

// === HELPER METHODS ===

static void ConfigureFirebase(WebApplicationBuilder builder)
{
    if (FirebaseApp.DefaultInstance == null)
    {
        try
        {
            var serviceAccountKeyPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
            if (string.IsNullOrEmpty(serviceAccountKeyPath))
            {
                serviceAccountKeyPath = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");
                if (!File.Exists(serviceAccountKeyPath) && !string.IsNullOrEmpty(builder.Environment.ContentRootPath))
                {
                    serviceAccountKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
                }
            }

            Console.WriteLine($"Attempting to use service account key at: {serviceAccountKeyPath}");

            if (!File.Exists(serviceAccountKeyPath))
            {
                throw new FileNotFoundException($"Firebase service account file not found at: {serviceAccountKeyPath}");
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
                ProjectId = builder.Configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500"
            });

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);
            Console.WriteLine("Firebase Admin SDK initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Firebase Admin: {ex.Message}");

            if (builder.Environment.IsDevelopment())
            {
                try
                {
                    FirebaseApp.Create(new AppOptions { ProjectId = "excelly-lms-f3500" });
                    Console.WriteLine("Created default Firebase app for development");
                }
                catch (Exception devEx)
                {
                    Console.WriteLine($"Failed to create development Firebase app: {devEx.Message}");
                }
            }
        }
    }
}

static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
{
    var jwtKey = builder.Configuration["Jwt:Secret"] ??
        throw new InvalidOperationException("JWT Secret is not configured");
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
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });
}

static void RegisterRepositories(IServiceCollection services)
{
    // Auth Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

    // Admin Repositories
    services.AddScoped<ITechnologyRepository, TechnologyRepository>();
    services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
    services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();

    // Course Repositories
    services.AddScoped<ICourseRepository, CourseRepository>();
    services.AddScoped<ILessonRepository, LessonRepository>();
    services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();
    services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
    services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();
    services.AddScoped<ICertificateRepository, CertificateRepository>();

    // Quiz Repositories
    services.AddScoped<IQuizRepository, QuizRepository>();
    services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();

    // Learner Repositories
    services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
    services.AddScoped<IUserTechnologyRepository, UserTechnologyRepository>();
    services.AddScoped<IUserProjectRepository, UserProjectRepository>();
    services.AddScoped<IUserCertificationRepository, UserCertificationRepository>();
    services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
    services.AddScoped<IThreadCommentRepository, ThreadCommentRepository>();
    services.AddScoped<IThreadComReplyRepository, ThreadComReplyRepository>();

    // Project Manager Repositories
    services.AddScoped<IProjectRepository, ProjectRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
}

static void RegisterServices(IServiceCollection services)
{
    // Auth Services
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IEmailService, EmailService>();

    // Admin Services
    services.AddScoped<IUserManagementService, UserManagementService>();
    services.AddScoped<ITechnologyService, TechnologyService>();
    services.AddScoped<ICourseCategoryService, CourseCategoryService>();
    services.AddScoped<ICourseAdminService, CourseAdminService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IAnalyticsService, AnalyticsService>();

    // File Storage Services
    services.AddScoped<IFileStorageService, LocalFileStorageService>();
    services.AddScoped<IFileService, FileService>();

    // Course Services
    services.AddScoped<ICourseService, CourseService>();
    services.AddScoped<IEnrollmentService, EnrollmentService>();
    services.AddScoped<ILearnerCourseService, LearnerCourseService>();
    services.AddScoped<ICertificateService, CertificateService>();

    // Quiz Services
    services.AddScoped<IQuizService, QuizService>();
    services.AddScoped<IQuizAttemptService, QuizAttemptService>();

    // Learner Services
    services.AddScoped<IUserBadgeService, UserBadgeService>();
    services.AddScoped<IUserTechnologyService, UserTechnologyService>();
    services.AddScoped<IUserProjectService, UserProjectService>();
    services.AddScoped<IUserCertificationService, UserCertificationService>();
    services.AddScoped<IUserProfileService, UserProfileService>();
    services.AddScoped<IForumService, ForumService>();
    // NEW: Learner Stats Service
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.Learner.ILearnerStatsService, ExcellyGenLMS.Application.Services.Learner.LearnerStatsService>();


    // Project Manager Services
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IProjectService,
        ExcellyGenLMS.Application.Services.ProjectManager.ProjectService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IRoleService,
        ExcellyGenLMS.Application.Services.ProjectManager.RoleService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IPMTechnologyService,
        ExcellyGenLMS.Application.Services.ProjectManager.PMTechnologyService>();
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ExcellyGenLMS API",
            Version = "v1",
            Description = "Learning Management System API"
        });

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

        c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    });
}

static void ConfigureHttpPipeline(WebApplication app, WebApplicationBuilder builder)
{
    // === DEVELOPMENT/PRODUCTION PIPELINE CONFIGURATION ===
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1"));
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // === STATIC FILES CONFIGURATION ===
    ConfigureStaticFiles(app, builder);

    // === MIDDLEWARE PIPELINE ===
    app.UseCors("AllowReactApp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRoleAuthorization(); // Your custom role check middleware
    app.MapControllers();
}

static void ConfigureStaticFiles(WebApplication app, WebApplicationBuilder builder)
{
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
            Directory.CreateDirectory(Path.Combine(uploadsPath, "forum"));

            Console.WriteLine("Created upload subdirectories");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating wwwroot subdirectories: {ex.Message}");
        }
    }

    string fileStoragePath = builder.Configuration.GetValue<string>("FileStorage:LocalPath") ??
        Path.Combine(builder.Environment.ContentRootPath, "uploads");

    if (!Directory.Exists(fileStoragePath))
    {
        try
        {
            Directory.CreateDirectory(fileStoragePath);
            Console.WriteLine($"Created file storage directory: {fileStoragePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create file storage directory: {fileStoragePath}, Error: {ex.Message}");
        }
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(webRootPath),
        RequestPath = ""
    });

    var fullFileStoragePath = Path.GetFullPath(fileStoragePath);
    var fullWwwRootPath = Path.GetFullPath(webRootPath);

    if (!fullFileStoragePath.StartsWith(fullWwwRootPath, StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"Configuring static files from custom path: {fileStoragePath} mapped to /uploads");
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(fileStoragePath),
            RequestPath = "/uploads"
        });
    }
}
