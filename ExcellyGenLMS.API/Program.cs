////using Microsoft.EntityFrameworkCore;
////using Microsoft.AspNetCore.Authentication.JwtBearer;
////using Microsoft.IdentityModel.Tokens;
////using System.Text;
////using FirebaseAdmin;
////using Google.Apis.Auth.OAuth2;
////using ExcellyGenLMS.Infrastructure.Data;
////using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
////using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
////using ExcellyGenLMS.Application.Interfaces.Auth;
////using ExcellyGenLMS.Application.Services.Auth;
////using ExcellyGenLMS.Infrastructure.Services.Auth;
////using Microsoft.OpenApi.Models;
////using ExcellyGenLMS.API.Middleware;
////using ExcellyGenLMS.Application.Interfaces.Admin;
////using ExcellyGenLMS.Application.Services.Admin;
////using Microsoft.AspNetCore.Identity;
////using ExcellyGenLMS.Core.Entities.Auth;

////using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
////using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;

////// --- Add Course Module Usings ---
////using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
////using ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo; // Adjusted namespace
////using ExcellyGenLMS.Application.Interfaces.Course;
////using ExcellyGenLMS.Application.Services.CourseSvc; // Adjusted namespace
////using ExcellyGenLMS.Core.Interfaces.Infrastructure;
////using ExcellyGenLMS.Infrastructure.Services.Storage;
////using Microsoft.Extensions.FileProviders; // Required for UseStaticFiles

////var builder = WebApplication.CreateBuilder(args);

////// Add DbContext configuration
////builder.Services.AddDbContext<ApplicationDbContext>(options =>
////    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

////// Add CORS with specific localhost origins
////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowReactApp",
////        policyBuilder =>
////        {
////            policyBuilder
////                .WithOrigins(
////                    "http://localhost:5173",  // Vite development server
////                    "http://localhost:3000",  // React standard port
////                    "https://excelly-lms-f3500.web.app"  // Production
////                )
////                .AllowAnyMethod()
////                .AllowAnyHeader()
////                .AllowCredentials();
////        });
////});

////// Initialize Firebase Admin
////if (FirebaseApp.DefaultInstance == null)
////{
////    try
////    {
////        var serviceAccountKeyPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
////        if (string.IsNullOrEmpty(serviceAccountKeyPath))
////        {
////            // Fallback to default location
////            serviceAccountKeyPath = "firebase-service-account.json";
////        }

////        Console.WriteLine($"Using service account key at: {serviceAccountKeyPath}");

////        if (!System.IO.File.Exists(serviceAccountKeyPath))
////        {
////            throw new FileNotFoundException($"Service account file not found at {serviceAccountKeyPath}");
////        }

////        FirebaseApp.Create(new AppOptions
////        {
////            Credential = GoogleCredential.FromFile(serviceAccountKeyPath)
////        });

////        Console.WriteLine("Firebase Admin SDK initialized successfully.");
////    }
////    catch (Exception ex)
////    {
////        Console.WriteLine($"Error initializing Firebase Admin: {ex.Message}");
////        if (ex.InnerException != null)
////        {
////            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
////        }

////        // Instead of failing, create a dummy Firebase app for development if needed
////        if (builder.Environment.IsDevelopment())
////        {
////            Console.WriteLine("Creating default Firebase app for development...");
////            try
////            {
////                FirebaseApp.Create(new AppOptions
////                {
////                    ProjectId = "excelly-lms-f3500"
////                });
////                Console.WriteLine("Default Firebase app created for development.");
////            }
////            catch (Exception devEx)
////            {
////                Console.WriteLine($"Failed to create development Firebase app: {devEx.Message}");
////            }
////        }
////    }
////}

////// Setup JWT Authentication
////var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
////var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
////var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";

////builder.Services.AddAuthentication(options =>
////{
////    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
////    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
////})
////.AddJwtBearer(options =>
////{
////    options.TokenValidationParameters = new TokenValidationParameters
////    {
////        ValidateIssuer = true,
////        ValidateAudience = true,
////        ValidateLifetime = true,
////        ValidateIssuerSigningKey = true,
////        ValidIssuer = jwtIssuer,
////        ValidAudience = jwtAudience,
////        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
////        ClockSkew = TimeSpan.Zero // Reduce the default clock skew of 5 minutes
////    };

////    options.Events = new JwtBearerEvents
////    {
////        OnAuthenticationFailed = context =>
////        {
////            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
////            {
////                context.Response.Headers["Token-Expired"] = "true";
////            }
////            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
////            return Task.CompletedTask;
////        }
////    };
////});

////// Register IPasswordHasher service that UserService depends on
////builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

////// Register repositories
////builder.Services.AddScoped<IUserRepository, UserRepository>();
////builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

////// Register services
////builder.Services.AddScoped<IUserService, UserService>();
////builder.Services.AddScoped<IAuthService, AuthService>();
////builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
////builder.Services.AddScoped<ITokenService, TokenService>();

////// Register User Management Service
////builder.Services.AddScoped<IUserManagementService, UserManagementService>();

////// Register Tech Management Service
////builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
////builder.Services.AddScoped<ITechnologyService, TechnologyService>();

////// Register CourseCategory repositories and services
////builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
////builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();

////// Register Course Admin repositories and services
////builder.Services.AddScoped<ICourseAdminRepository, CourseAdminRepository>();
////builder.Services.AddScoped<ICourseAdminService, CourseAdminService>();

////// Register Admin Repositories & Services (keep existing - Ensure needed ones are here)
////builder.Services.AddScoped<IUserManagementService, UserManagementService>();
////builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>(); // Assumed Exists
////builder.Services.AddScoped<ITechnologyService, TechnologyService>();       // Assumed Exists
////builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>(); // Assumed Exists
////builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();   // Assumed Exists
////                                                                               // Ensure CourseAdminRepository/Service are not conflicting and registered if needed elsewhere


////// Register Course Module Repositories
////builder.Services.AddScoped<ICourseRepository, CourseRepository>();
////builder.Services.AddScoped<ILessonRepository, LessonRepository>();
////builder.Services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();

////// Register File Storage Service (Choose Local or Cloud implementation)
////builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();


////// Register Course Module Services
////builder.Services.AddScoped<ICourseService, CourseService>();

////// Add controllers
////builder.Services.AddControllers();

////// Add Swagger
////builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen(c =>
////{
////    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExcellyGenLMS API", Version = "v1" });

////    // Add JWT Authentication to Swagger
////    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
////    {
////        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
////        Name = "Authorization",
////        In = ParameterLocation.Header,
////        Type = SecuritySchemeType.ApiKey,
////        Scheme = "Bearer"
////    });

////    c.AddSecurityRequirement(new OpenApiSecurityRequirement
////    {
////        {
////            new OpenApiSecurityScheme
////            {
////                Reference = new OpenApiReference
////                {
////                    Type = ReferenceType.SecurityScheme,
////                    Id = "Bearer"
////                }
////            },
////            Array.Empty<string>()
////        }
////    });
////});

////var app = builder.Build();

////// Configure the HTTP request pipeline
////if (app.Environment.IsDevelopment())
////{
////    app.UseSwagger();
////    app.UseSwaggerUI();
////}

////app.UseHttpsRedirection();

////// Apply CORS middleware BEFORE authentication middleware
////app.UseCors("AllowReactApp");

////app.UseAuthentication();
////app.UseAuthorization();
////app.UseRoleAuthorization();
////app.MapControllers();

////string fileStoragePath = builder.Configuration.GetValue<string>("FileStorage:LocalPath") ?? Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "uploads");
////// Check if path is relative to wwwroot or absolute
////PathString requestPath;
////if (Path.IsPathRooted(fileStoragePath))
////{
////    // Absolute path outside wwwroot - needs careful configuration & security considerations
////    _logger.LogWarning("Serving static files from absolute path: {Path}. Ensure security is configured.", fileStoragePath);
////    // For absolute paths outside wwwroot, map a request path (e.g., '/uploads')
////    requestPath = "/uploads"; // The URL path users will access
////    if (!Directory.Exists(fileStoragePath)) Directory.CreateDirectory(fileStoragePath); // Ensure it exists
////    app.UseStaticFiles(new StaticFileOptions
////    {
////        FileProvider = new PhysicalFileProvider(fileStoragePath),
////        RequestPath = requestPath // Maps URL path /uploads to physical path
////    });

////}
////else if (fileStoragePath.StartsWith("wwwroot"))
////{
////    // Relative path inside wwwroot
////    string relativePath = Path.GetRelativePath(app.Environment.WebRootPath ?? "", fileStoragePath); // Get path relative to wwwroot
////    requestPath = "/" + relativePath.Replace(Path.DirectorySeparatorChar, '/'); // URL path (e.g. /uploads)
////    if (!Directory.Exists(fileStoragePath)) Directory.CreateDirectory(fileStoragePath); // Ensure it exists
////    app.UseStaticFiles(new StaticFileOptions
////    {
////        RequestPath = requestPath // Usually not needed if path is directly under wwwroot, but good practice
////    });
////}
////else
////{
////    // Path relative to content root, but outside wwwroot (less common for serving)
////    // Treat similarly to absolute path - map a RequestPath
////    requestPath = "/uploads"; // Or configure another URL path
////    _logger.LogWarning("Serving static files from content root sub-path: {Path}. Ensure it's intended.", fileStoragePath);
////    if (!Directory.Exists(fileStoragePath)) Directory.CreateDirectory(fileStoragePath);
////    app.UseStaticFiles(new StaticFileOptions
////    {
////        FileProvider = new PhysicalFileProvider(fileStoragePath),
////        RequestPath = requestPath
////    });

////}

////// Always serve standard wwwroot content
////app.UseStaticFiles();


////app.UseRouting(); // Routing must come after StaticFiles sometimes, before CORS/Auth usually. Place strategically.

////// Apply CORS middleware BEFORE Authentication/Authorization
////app.UseCors("AllowReactApp");


////app.UseAuthentication(); // Identify the user
////app.UseAuthorization(); // Check if user is allowed (based on Authorize attribute)


////// --- Custom Role Auth Middleware (keep existing) ---
////// Assuming your custom middleware handles role checking based on JWT claim
////// Place AFTER UseAuthentication & UseAuthorization
////app.UseRoleAuthorization();


////app.MapControllers(); // Map controller endpoints


////app.Run();

//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.Extensions.FileProviders; // For PhysicalFileProvider (Static Files)
//using Microsoft.AspNetCore.Identity;       // For PasswordHasher
//using Microsoft.OpenApi.Models;           // For Swagger
//using System.Text;                        // For Encoding
//using System;                            // For Exception, Path, etc.
//using System.IO;                         // For Path, Directory
//using System.Collections.Generic;        // For List<string> in Swagger security requirements
//using FirebaseAdmin;
//using Google.Apis.Auth.OAuth2;

//// Infrastructure Layer Namespaces
//using ExcellyGenLMS.Infrastructure.Data;
//using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
//using ExcellyGenLMS.Infrastructure.Data.Repositories.Admin;
//using ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo; // Correct Namespace for Course Repos
//using ExcellyGenLMS.Infrastructure.Services.Auth;
//using ExcellyGenLMS.Infrastructure.Services.Storage;

//// Application Layer Namespaces
//using ExcellyGenLMS.Application.Interfaces.Auth;
//using ExcellyGenLMS.Application.Interfaces.Admin;
//using ExcellyGenLMS.Application.Interfaces.Course;
//using ExcellyGenLMS.Application.Services.Auth;
//using ExcellyGenLMS.Application.Services.Admin;
//using ExcellyGenLMS.Application.Services.CourseSvc; // Correct Namespace for Course Service

//// Core Layer Namespaces
//using ExcellyGenLMS.Core.Entities.Auth;
//using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
//using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
//using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
//using ExcellyGenLMS.Core.Interfaces.Infrastructure;

//// API Layer Namespaces
//using ExcellyGenLMS.API.Middleware;

//var builder = WebApplication.CreateBuilder(args);
//var configuration = builder.Configuration; // Reference to configuration for convenience
//var environment = builder.Environment; // Reference to environment

//// --- Basic Logging Setup (available immediately) ---
//// Create a logger to use during startup
//var loggerFactory = LoggerFactory.Create(logBuilder =>
//{
//    logBuilder.AddConfiguration(configuration.GetSection("Logging"));
//    logBuilder.AddConsole();
//    logBuilder.AddDebug();
//});
//var logger = loggerFactory.CreateLogger<Program>();

//logger.LogInformation("Starting application configuration...");

//// --- Database Context ---
//var connectionString = configuration.GetConnectionString("DefaultConnection");
//if (string.IsNullOrEmpty(connectionString))
//{
//    logger.LogCritical("Database connection string 'DefaultConnection' is missing or empty in configuration.");
//    throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
//}
//logger.LogInformation("Configuring DbContext with SQL Server.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

//// --- CORS ---
//logger.LogInformation("Configuring CORS policy...");
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", policyBuilder =>
//    {
//        policyBuilder
//            .WithOrigins(
//                "http://localhost:5173",  // Vite development server
//                "http://localhost:3000",  // React standard port
//                "https://excelly-lms-f3500.web.app"  // Production
//            )
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials();
//    });
//});

//// --- Firebase Admin SDK ---
//logger.LogInformation("Attempting to initialize Firebase Admin SDK...");
//try
//{
//    if (FirebaseApp.DefaultInstance == null)
//    {
//        var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"] ?? "firebase-service-account.json";

//        logger.LogInformation("Using service account key path: {Path}", serviceAccountKeyPath);

//        if (File.Exists(serviceAccountKeyPath))
//        {
//            FirebaseApp.Create(new AppOptions
//            {
//                Credential = GoogleCredential.FromFile(serviceAccountKeyPath)
//            });
//            logger.LogInformation("Firebase Admin SDK initialized successfully.");
//        }
//        else if (environment.IsDevelopment())
//        {
//            logger.LogWarning("Firebase service account file not found. Creating default Firebase app for development.");
//            FirebaseApp.Create(new AppOptions
//            {
//                ProjectId = "excelly-lms-f3500"
//            });
//            logger.LogInformation("Default Firebase app created for development.");
//        }
//        else
//        {
//            logger.LogError("Firebase service account file not found at: {Path}", serviceAccountKeyPath);
//            // In production, we might want to throw here, but we'll log an error and continue
//        }
//    }
//    else
//    {
//        logger.LogInformation("Firebase Admin SDK already initialized.");
//    }
//}
//catch (Exception ex)
//{
//    logger.LogError(ex, "Error initializing Firebase Admin SDK.");
//    if (environment.IsDevelopment())
//    {
//        // In development, let's continue even if Firebase init fails
//        logger.LogWarning("Continuing startup despite Firebase initialization failure (Development mode).");
//    }
//    else
//    {
//        // In production, this might be a critical error
//        throw;
//    }
//}

//// --- JWT Authentication ---
//logger.LogInformation("Configuring JWT Bearer Authentication.");
//var jwtKey = configuration["Jwt:Secret"];
//if (string.IsNullOrEmpty(jwtKey))
//{
//    logger.LogCritical("JWT Secret is missing in configuration. Authentication will fail.");
//    throw new InvalidOperationException("JWT Secret is not configured.");
//}
//var jwtIssuer = configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
//var jwtAudience = configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtIssuer,
//        ValidAudience = jwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//        ClockSkew = TimeSpan.Zero
//    };

//    options.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            logger.LogWarning("JWT Authentication Failed: {Message}", context.Exception.Message);
//            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//            {
//                context.Response.Headers.Add("Token-Expired", "true");
//            }
//            return Task.CompletedTask;
//        }
//    };
//});

//// --- Dependency Injection Registration ---
//logger.LogInformation("Registering services and repositories...");

//// Add HttpContextAccessor
//builder.Services.AddHttpContextAccessor();

//// Register Core Authentication Services
//builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//// Register Auth Repositories
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

//// Register Auth Services
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
//builder.Services.AddScoped<ITokenService, TokenService>();

//// Register Admin Repositories and Services
//builder.Services.AddScoped<IUserManagementService, UserManagementService>();
//builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
//builder.Services.AddScoped<ITechnologyService, TechnologyService>();
//builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
//builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();

//// Register Course Repositories
//builder.Services.AddScoped<ICourseRepository, CourseRepository>();
//builder.Services.AddScoped<ILessonRepository, LessonRepository>();
//builder.Services.AddScoped<ICourseDocumentRepository, CourseDocumentRepository>();

//// Register File Storage Service
//builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

//// Register Course Services
//builder.Services.AddScoped<ICourseService, CourseService>();

//// --- Framework Services ---
//builder.Services.AddControllers();

//// Configure Swagger/OpenAPI
//logger.LogInformation("Configuring Swagger/OpenAPI documentation.");
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "ExcellyGenLMS API",
//        Version = "v1",
//        Description = "API for ExcellyGenLMS learning management system"
//    });

//    // Configure Swagger to use JWT Bearer Authentication
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new List<string>()
//        }
//    });
//});

//// --- Build the Application ---
//logger.LogInformation("Building the application...");
//var app = builder.Build();

//// --- Configure the HTTP Request Pipeline ---
//logger.LogInformation("Configuring the HTTP request pipeline...");

//// Development specific middleware
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();

//    // Swagger middleware
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcellyGenLMS API v1");
//        // Optional customization
//        c.RoutePrefix = "swagger"; // Default route prefix
//        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
//        c.EnableFilter();
//    });
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//// Force HTTPS redirection
//app.UseHttpsRedirection();

//// --- Static File Serving ---
//logger.LogInformation("Configuring static file serving...");

//// Get file storage path from configuration
//string fileStoragePath = configuration.GetValue<string>("FileStorage:LocalPath") ??
//    Path.Combine(app.Environment.ContentRootPath, "uploads");

//// Ensure directory exists
//if (!Directory.Exists(fileStoragePath))
//{
//    try
//    {
//        Directory.CreateDirectory(fileStoragePath);
//        logger.LogInformation("Created file storage directory: {Path}", fileStoragePath);
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Failed to create file storage directory: {Path}", fileStoragePath);
//    }
//}

//// Configure static file middleware
//if (Path.IsPathRooted(fileStoragePath))
//{
//    // Absolute path - needs explicit mapping
//    app.UseStaticFiles(new StaticFileOptions
//    {
//        FileProvider = new PhysicalFileProvider(fileStoragePath),
//        RequestPath = "/uploads"
//    });
//    logger.LogInformation("Configured static files from absolute path: {Path} -> /uploads", fileStoragePath);
//}
//else
//{
//    // Relative path - could be inside wwwroot
//    var wwwrootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
//    if (fileStoragePath.StartsWith(wwwrootPath))
//    {
//        // Already in wwwroot, standard static files will handle it
//        logger.LogInformation("Using wwwroot for file storage: {Path}", fileStoragePath);
//    }
//    else
//    {
//        // Outside wwwroot, need explicit mapping
//        app.UseStaticFiles(new StaticFileOptions
//        {
//            FileProvider = new PhysicalFileProvider(fileStoragePath),
//            RequestPath = "/uploads"
//        });
//        logger.LogInformation("Configured static files from path: {Path} -> /uploads", fileStoragePath);
//    }
//}

//// Serve static files from wwwroot
//app.UseStaticFiles();

//// --- Routing and Middleware Pipeline ---
//app.UseRouting();

//// CORS - must be before Auth
//app.UseCors("AllowReactApp");

//// Auth middleware
//app.UseAuthentication();
//app.UseAuthorization();

//// Custom role authorization middleware
//app.UseRoleAuthorization();

//// Map controllers
//app.MapControllers();

//logger.LogInformation("Application configured successfully. Starting...");
//app.Run();

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


// Add DbContext configuration
logger.LogInformation("Configuring DbContext.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Add CORS with specific localhost origins
logger.LogInformation("Configuring CORS.");
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
logger.LogInformation("Initializing Firebase Admin SDK.");
if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];
        if (string.IsNullOrEmpty(serviceAccountKeyPath))
        {
            serviceAccountKeyPath = "firebase-service-account.json"; // Fallback
        }
        logger.LogInformation("Using Firebase service account key at: {Path}", serviceAccountKeyPath);

        if (!File.Exists(serviceAccountKeyPath))
        {
            throw new FileNotFoundException($"Service account file not found at {serviceAccountKeyPath}");
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(serviceAccountKeyPath)
        });
        logger.LogInformation("Firebase Admin SDK initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing Firebase Admin.");
        if (ex.InnerException != null)
        {
            logger.LogError(ex.InnerException, "Inner exception during Firebase Admin initialization.");
        }

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

logger.LogInformation("Registering application services and repositories.");
// Register IPasswordHasher service
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register Auth repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register Auth services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

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

logger.LogInformation("Building the application.");
var app = builder.Build();

// Configure the HTTP request pipeline
logger.LogInformation("Configuring the HTTP request pipeline.");
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
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
}

app.UseHttpsRedirection();

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

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseRoleAuthorization(); // Custom middleware

app.MapControllers();

logger.LogInformation("Application startup complete. Running.");
app.Run();