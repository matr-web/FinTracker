using FinTracker.BLL.Services;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.WebAPI.Program_Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:JWTIssuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:JWTKey"]!))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<FinTrackerDbContext>(options 
    => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

// Automatically manages the lifecycle of network connections and prevents socket leaks (Socket Exhaustion).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICashService, CashService>();
builder.Services.AddScoped<IDebtService, DebtService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

// CORS configuration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("My_CORS_Policy", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Test API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseCors("My_CORS_Policy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
