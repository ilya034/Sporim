using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Sporim.Api.Auth;
using Sporim.Api.Contracts;
using Sporim.Api.Endpoints;
using Sporim.Api.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<MockStore>();
builder.Services.AddSingleton<MockTokenService>();

var app = builder.Build();

app.UseCors();

// Convert malformed JSON body exceptions into unified ApiError(VALIDATION_ERROR)
app.Use(async (context, next) =>
{
    try
    {
        if (context.Request.Headers.TryGetValue("X-Mock-Delay", out var delayValues) &&
            int.TryParse(delayValues.FirstOrDefault(), out var delayMs) &&
            delayMs > 0)
        {
            await Task.Delay(Math.Min(delayMs, 5000), context.RequestAborted);
        }

        await next(context);
    }
    catch (BadHttpRequestException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new ApiError(
            ErrorCode.ValidationError,
            $"Некорректный формат запроса: {ex.Message}"));
    }
});

app.MapOpenApi("/openapi/v1.json");
app.MapMockApiEndpoints();

app.Run();

public partial class Program;
