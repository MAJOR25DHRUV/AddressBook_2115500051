using System.Text;
using AutoMapper;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using BusinessLayer.Validation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepositoryLayer.Context;
using RepositoryLayer.Helper;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register AutoMapper
builder.Services.AddSingleton<IMapper>(sp =>
{
    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
    });

    return config.CreateMapper();
});

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AddressBookValidator>());

// Add OpenAPI (Swagger) support with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Address Book API",
        Version = "v1",
        Description = "API for Address Book Application with JWT Authorization"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure Database Context
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AddressBookContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Register Services
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<ResetTokenHelper>();
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IUserRL, UserRL>();
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SMTP"));
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<RedisCacheHelper>();
builder.Services.AddSingleton<RabbitMQProducer>();
builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Address Book API v1");
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
