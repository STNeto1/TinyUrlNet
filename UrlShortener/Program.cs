using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using UrlShortener.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       throw new InvalidOperationException("Connection string 'Default' not found.");
builder.Services.AddDbContext<DatabaseContext>(options => options.UseMySQL(connectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));


// Add services to the container.

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToAccessDenied = c =>
    {
        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.FromResult<object>(null);
    };
    options.Events.OnRedirectToLogin = c =>
    {
        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.FromResult<object>(null);
    };
});


builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    /*options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Cookie,
        Name = CookieAuthenticationDefaults.CookiePrefix,
        Type = SecuritySchemeType.ApiKey,
    });*/

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseStatusCodePages();

app.MapControllers();

app.Run();