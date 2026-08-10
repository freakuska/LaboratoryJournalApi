using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LaboratoryJournal.Data;
using LaboratoryJournal.Models;
using LaboratoryJournal.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление конфигурации из appsettings.json
var configuration = builder.Configuration;

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Регистрация Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    
    options.SignIn.RequireConfirmedEmail = false;
    
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT конфигурация
var jwtSecret = configuration["Jwt:Secret"] ?? "your-secret-key-that-is-at-least-32-characters-long-for-security";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "LaboratoryJournal";
var jwtAudience = configuration["Jwt:Audience"] ?? "LaboratoryJournalUsers";

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Добавить после конфигурации JWT
builder.Services.AddScoped<JwtTokenGenerator>(sp => 
    new JwtTokenGenerator(jwtSecret, jwtIssuer, jwtAudience)
);

// Добавление сервисов приложения
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Laboratory Journal API",
        Version = "v1",
        Description = "API для управления лабораторным журналом, экспериментами и результатами"
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Автоматическое применение миграций и создание ролей при запуске
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Создание ролей по умолчанию
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("Researcher"))
        {
            await roleManager.CreateAsync(new IdentityRole("Researcher"));
        }
        if (!await roleManager.RoleExistsAsync("Moderator"))
        {
            await roleManager.CreateAsync(new IdentityRole("Moderator"));
        }

        // Создание демо-пользователя
        var demoUser = await userManager.FindByEmailAsync("demo@lab.com");
        if (demoUser == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = "demo",
                Email = "demo@lab.com",
                EmailConfirmed = true,
                FullName = "Демо Пользователь",
                Position = "Исследователь",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(newUser, "Demo@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, "Researcher");
                Console.WriteLine("✓ Демо-пользователь создан: demo@lab.com / Demo@123456");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при инициализации базы данных.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
