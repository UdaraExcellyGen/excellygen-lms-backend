using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
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

// Added for File Storage Service
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using ExcellyGenLMS.Infrastructure.Services.Storage;

// Corrected Course module imports
using ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo;  // Note the "Repo" suffix
using ExcellyGenLMS.Application.Services.CourseSvc;  // Note the "Svc" suffix

var builder = WebApplication.CreateBuilder(args);

// --- Add DbContext configuration ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Add IDbConnection for Dapper ---
builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Add CORS ---
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

// --- Enhanced Firebase Admin Configuration ---
if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        var serviceAccountKeyPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
        if (string.IsNullOrEmpty(serviceAccountKeyPath))
        {
            serviceAccountKeyPath = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");
            if (!System.IO.File.Exists(serviceAccountKeyPath) && !string.IsNullOrEmpty(builder.Environment.ContentRootPath))
            {
                serviceAccountKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
            }
        }

        Console.WriteLine($"Attempting to use service account key at: {serviceAccountKeyPath}");

        if (!System.IO.File.Exists(serviceAccountKeyPath))
        {
            throw new FileNotFoundException($"Firebase service account file not found at the resolved path: {serviceAccountKeyPath}. Ensure the file exists or the 'Firebase:ServiceAccountKeyPath' in appsettings.json is correct.");
        }

        // Initialize Firebase Admin with enhanced configuration
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
            ProjectId = builder.Configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500"
        });

        Console.WriteLine("Firebase Admin SDK initialized successfully.");

        // Set up default application credentials for Google Cloud Storage
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);
        Console.WriteLine($"Set GOOGLE_APPLICATION_CREDENTIALS environment variable: {serviceAccountKeyPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"CRITICAL: Error initializing Firebase Admin: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }

        // Try to create a default Firebase app for development
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


// --- Setup JWT Authentication ---
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

// Course repositories - with correct implementation classes
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// Learner repositories
builder.Services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
builder.Services.AddScoped<IUserTechnologyRepository, UserTechnologyRepository>();
builder.Services.AddScoped<IUserProjectRepository, UserProjectRepository>();
builder.Services.AddScoped<IUserCertificationRepository, UserCertificationRepository>();
builder.Services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
builder.Services.AddScoped<IThreadCommentRepository, ThreadCommentRepository>();
builder.Services.AddScoped<IThreadComReplyRepository, ThreadComReplyRepository>();

// ProjectManager repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// --- Register services ---
// Auth services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Admin services
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
builder.Services.AddScoped<ICourseAdminService, CourseAdminService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add Analytics Service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Course services - with correct implementation class
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// File storage services - crucial for your Firebase implementation
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IFileService, FileService>();

// Learner services
builder.Services.AddScoped<IUserBadgeService, UserBadgeService>();
builder.Services.AddScoped<IUserTechnologyService, UserTechnologyService>();
builder.Services.AddScoped<IUserProjectService, UserProjectService>();
builder.Services.AddScoped<IUserCertificationService, UserCertificationService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IForumService, ForumService>();

// ProjectManager services
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IProjectService, ExcellyGenLMS.Application.Services.ProjectManager.ProjectService>();
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IRoleService, ExcellyGenLMS.Application.Services.ProjectManager.RoleService>();
builder.Services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IPMTechnologyService, ExcellyGenLMS.Application.Services.ProjectManager.PMTechnologyService>();

// --- Required for accessing HttpContext ---
builder.Services.AddHttpContextAccessor();

// --- Add controllers with JSON enum serialization ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// --- Add Swagger ---
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

// --- Build the Application ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1"));
}
else
{
    app.UseExceptionHandler("/error"); // Consider adding a real error handling endpoint
    app.UseHsts();
}

app.UseHttpsRedirection();

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

// Set up static file serving for both wwwroot and any custom file paths
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

// If uploads directory is outside wwwroot, configure it separately
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

// --- Apply Middleware ---
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseRoleAuthorization(); // Your custom role check middleware

app.MapControllers();

// --- Run the Application ---
app.Run();