using Microsoft.AspNetCore.Mvc;
using ThrPresetsApi.Api.Configuration;
using ThrPresetsApi.Api.Features.Auth;
using ThrPresetsApi.Api.Features.Presets;
using ThrPresetsApi.Api.Features.Users;
using ThrPresetsApi.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddJsonConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddValidation();
builder.Services.AddValidationConfiguration();
builder.Services.AddS3Infrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

app.UseSwaggerConfiguration();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPresetEndpoints();

await app.RunAsync();

public partial class Program { }