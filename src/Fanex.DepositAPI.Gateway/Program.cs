using Fanex.DepositAPI.Gateway.Handlers;
using Fanex.DepositAPI.Gateway.Middlewares;
using Fanex.DepositAPI.Gateway.Startup;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var ocelotConfig = builder.Configuration.GetSection("Ocelot");
builder.Services
    .AddOcelot(ocelotConfig)
    .AddDelegatingHandler<NoGzipDelegatingHandler>(true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services
    .AddDependencyInjection(builder.Configuration)
    .LoggingConfigure(builder.Configuration);

var app = builder.Build();

app.UseWhen(
    context => !context.Request.Path.Equals("/") && !context.Request.Path.ToString().Contains("."),
    builder => builder.UseHttpLogging());

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();

app.UseRouting();

app.MapGet("/", () => "Hi, I'm Deposit API Gateway!");

app.UseEndpoints(_ => { });

app.UseMiddleware<ApiKeyMiddleware>();

app.UseOcelot().Wait();

app.Run();