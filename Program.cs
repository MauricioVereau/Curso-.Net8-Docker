using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Models;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. 
//cadena de conexion
var dbConString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConString));

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024;
    options.UseCaseSensitivePaths = true;
});


builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


//Autenticacion JWT
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey") ?? throw new InvalidOperationException("La secret key no está configurada.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddControllers(  option =>
{
  option.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
  option.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecommerce",
      Description = "API Ecommerce ASP.NET Core Web API",
      TermsOfService = new Uri("https://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com"),
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("https://example.com/license"),
      }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
      Version = "v2",
      Title = "API Ecommerce",
      Description = "API Ecommerce ASP.NET Core Web API",
      TermsOfService = new Uri("https://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com"),
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("https://example.com/license"),
      }
    });
  }
);

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.ReportApiVersions = true;
  //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));  //?api-version
});

apiVersioningBuilder.AddApiExplorer(options =>
{
  options.GroupNameFormat = "'v'VVV"; // ejemplos: v1, v2, v3
  options.SubstituteApiVersionInUrl = true; // Reemplaza la versión en la URL api/v{version}/controller
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("*")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI( options =>
    {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
      options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseHttpsRedirection();
//app.UseCors("AllowSpecificOrigin");
app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
