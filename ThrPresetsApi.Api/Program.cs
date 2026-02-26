using ThrPresetsApi.Api.Configuration;
using ThrPresetsApi.Api.Features.Auth;
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
builder.Services.AddProblemDetails();


var app = builder.Build();

app.UseSwaggerConfiguration();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();

await app.RunAsync();
