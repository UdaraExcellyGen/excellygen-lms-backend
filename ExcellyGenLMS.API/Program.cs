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
using ExcellyGenLMS.Infrastructure.Data;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager;
using ExcellyGenLMS.Infrastructure.Services.Auth;
using ExcellyGenLMS.Infrastructure.Services.Common;
using ExcellyGenLMS.Infrastructure.Services.Storage;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
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
using ExcellyGenLMS.API.Middleware;
using ExcellyGenLMS.API.Controllers;
using ExcellyGenLMS.API.Controllers.Admin;
using ExcellyGenLMS.API.Controllers.Course;
using ExcellyGenLMS.API.Controllers.Learner;
using ExcellyGenLMS.API.Controllers.Auth;
using ExcellyGenLMS.API.Controllers.ProjectManager;

var builder = WebApplication.CreateBuilder(args);

try
{
    ConfigureDatabase(builder);
    ConfigureCors(builder);
    ConfigureAuthentication(builder);
    ConfigureFirebase(builder);
    RegisterRepositories(builder.Services);
    RegisterApplicationServices(builder.Services);
    ConfigureControllers(builder);
    ConfigureSwagger(builder);

    var app = builder.Build();
    ConfigureMiddlewarePipeline(app);

    Console.WriteLine("ExcellyGenLMS API is starting...");
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine($"URLs: {string.Join(", ", app.Urls)}");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex.Message}");
    throw;
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 2,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }),
        ServiceLifetime.Scoped);

    builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));
    Console.WriteLine("Database configuration completed with performance optimizations");
}

static void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "https://excelly-lms-f3500.web.app"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
    Console.WriteLine("CORS configuration completed");
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtKey = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT Secret is not configured");
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

    builder.Services.AddAuthorization();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    Console.WriteLine("Authentication configuration completed");
}

static void ConfigureFirebase(WebApplicationBuilder builder)
{
    if (FirebaseApp.DefaultInstance != null)
    {
        Console.WriteLine("Firebase already initialized");
        return;
    }
    try
    {
        var serviceAccountKeyPath = GetFirebaseServiceAccountPath(builder);
        if (File.Exists(serviceAccountKeyPath))
        {
            var projectId = builder.Configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500";
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountKeyPath),
                ProjectId = projectId
            });
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);
            Console.WriteLine($"Firebase initialized with project: {projectId}");
        }
        else
        {
            throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountKeyPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Firebase initialization failed: {ex.Message}");
        if (builder.Environment.IsDevelopment())
        {
            try
            {
                FirebaseApp.Create(new AppOptions { ProjectId = "excelly-lms-f3500" });
                Console.WriteLine("Using default Firebase configuration for development");
            }
            catch (Exception devEx)
            {
                Console.WriteLine($"Development Firebase setup failed: {devEx.Message}");
            }
        }
        else
        {
            throw;
        }
    }
}

static string GetFirebaseServiceAccountPath(WebApplicationBuilder builder)
{
    var configPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
    if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath)) return configPath;
    var baseDirectory = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");
    if (File.Exists(baseDirectory)) return baseDirectory;
    var contentRoot = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
    return contentRoot;
}

static void ConfigureControllers(WebApplicationBuilder builder)
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(Program).Assembly)
        .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Admin.CourseCategoriesController).Assembly)
        .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Course.CoursesController).Assembly)
        .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Learner.CvController).Assembly)
        .AddApplicationPart(typeof(ExcellyGenLMS.API.Controllers.Auth.AuthController).Assembly)
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    Console.WriteLine("Controllers configuration completed");
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ExcellyGenLMS API",
            Version = "v1.0",
            Description = "Learning Management System API",
            Contact = new OpenApiContact { Name = "ExcellyGen Team", Email = "support@excellygen.com" }
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    });
    Console.WriteLine("Swagger configuration completed");
}

static void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    services.AddScoped<ITechnologyRepository, TechnologyRepository>();
    services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
    services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();
    services.AddScoped<ICourseRepository, CourseRepository>();
    services.AddScoped<ILessonRepository, LessonRepository>();
    services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();
    services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
    services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();
    services.AddScoped<ICertificateRepository, CertificateRepository>();
    services.AddScoped<IExternalCertificateRepository, ExternalCertificateRepository>();
    services.AddScoped<IQuizRepository, QuizRepository>();
    services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
    services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
    services.AddScoped<IUserTechnologyRepository, UserTechnologyRepository>();
    services.AddScoped<IUserProjectRepository, UserProjectRepository>();
    services.AddScoped<IUserCertificationRepository, UserCertificationRepository>();
    services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
    services.AddScoped<IThreadCommentRepository, ThreadCommentRepository>();
    services.AddScoped<IThreadComReplyRepository, ThreadComReplyRepository>();
    services.AddScoped<ILearnerNotificationRepository, LearnerNotificationRepository>();
    services.AddScoped<IProjectRepository, ProjectRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IPMEmployeeAssignmentRepository, PMEmployeeAssignmentRepository>();

    // NEW REPOSITORY REGISTRATION
    services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

    Console.WriteLine("Repository registrations completed with External Certificate support");
}

static void RegisterApplicationServices(IServiceCollection services)
{
    services.AddHttpClient();
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1000;
        options.CompactionPercentage = 0.25;
    });

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IUserManagementService, UserManagementService>();
    services.AddScoped<ITechnologyService, TechnologyService>();
    services.AddScoped<ICourseCategoryService, CourseCategoryService>();
    services.AddScoped<ICourseAdminService, CourseAdminService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IAnalyticsService, AnalyticsService>();
    services.AddScoped<IFileStorageService, FirebaseStorageService>();
    services.AddScoped<IFileService, FileService>();
    services.AddScoped<ICourseService, CourseService>();
    services.AddScoped<IEnrollmentService, EnrollmentService>();
    services.AddScoped<ILearnerCourseService, LearnerCourseService>();
    services.AddScoped<ICertificateService, CertificateService>();
    services.AddScoped<ICourseCoordinatorAnalyticsService, CourseCoordinatorAnalyticsService>();
    services.AddScoped<IExternalCertificateService, ExternalCertificateService>();
    services.AddScoped<IQuizService, QuizService>();
    services.AddScoped<IQuizAttemptService, QuizAttemptService>();
    services.AddScoped<IUserBadgeService, UserBadgeService>();
    services.AddScoped<IUserTechnologyService, UserTechnologyService>();
    services.AddScoped<IUserProjectService, UserProjectService>();
    services.AddScoped<IUserCertificationService, UserCertificationService>();
    services.AddScoped<IUserProfileService, UserProfileService>();
    services.AddScoped<IForumService, ForumService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.Learner.ILearnerStatsService, ExcellyGenLMS.Application.Services.Learner.LearnerStatsService>();
    services.AddScoped<ICvService, CvService>();
    services.AddScoped<ILearnerNotificationService, LearnerNotificationService>();
    services.AddScoped<ILeaderboardService, LeaderboardService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IProjectService, ExcellyGenLMS.Application.Services.ProjectManager.ProjectService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IRoleService, ExcellyGenLMS.Application.Services.ProjectManager.RoleService>();
    services.AddScoped<ExcellyGenLMS.Application.Interfaces.ProjectManager.IPMTechnologyService, ExcellyGenLMS.Application.Services.ProjectManager.PMTechnologyService>();
    services.AddScoped<IEmployeeAssignmentService, EmployeeAssignmentService>();

    Console.WriteLine("Application services registration completed with caching optimization");
}

static void ConfigureMiddlewarePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1.0");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "ExcellyGenLMS API Documentation";
        });
        Console.WriteLine("Development middleware configured");
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
        Console.WriteLine("Production middleware configured");
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowReactApp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRoleAuthorization();
    app.MapControllers();
    Console.WriteLine("Middleware pipeline configured successfully with optimizations");
}