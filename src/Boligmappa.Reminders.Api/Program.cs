using System.Text.Json.Serialization;
using Boligmappa.Reminders.Api.Api.Endpoints;
using Boligmappa.Reminders.Api.Domain;
using Boligmappa.Reminders.Api.Infrastructure.Auth;
using Boligmappa.Reminders.Api.Infrastructure.Diagnostics;
using Boligmappa.Reminders.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services
    .AddAuthentication(AuthSchemes.XUserIdHeader)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, StubHeaderAuthenticationHandler>(
        AuthSchemes.XUserIdHeader, _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.DocumentOwner, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.XUserIdHeader);
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new DocumentOwnerRequirement());
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, DocumentOwnerAuthorizationHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(FrontendCorsPolicy);

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var clock = scope.ServiceProvider.GetRequiredService<IClock>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db, clock);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapPropertyEndpoints();
app.MapDocumentEndpoints();
app.MapHealthChecks("/health");

app.Run();
