using API;
using API.Config;
using Microsoft.AspNetCore.HttpOverrides;
using Validation.Middleware;

APIInfiscialEnvironment.SetEnvironmentKeys();

var builder = WebApplication.CreateBuilder(args);

ConfigureServicesHelper helper = new ConfigureServicesHelper(builder.Services);
helper.Setup();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
