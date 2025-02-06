using Fanex.DepositAPI.Gateway.DelegatingHandlers;
using Fanex.DepositAPI.Gateway.Middlewares;
using Fanex.DepositAPI.Gateway.Startup;
using Ocelot.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var ocelotConfig = builder.Configuration.GetSection("Ocelot");
builder.Services
    .AddOcelot(ocelotConfig)
    .AddDelegatingHandler<NoGzipDelegatingHandler>(true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services
    .AddDependencyInjection(builder.Configuration)
    .LoggingConfigure(builder.Configuration);

var app = builder.Build();

app.UseWhen(
    context => !context.Request.Path.Equals("/") && !context.Request.Path.ToString().Contains("."),
    config => config.UseHttpLogging());

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();

app.UseRouting();

app.MapGet("/", () => "Hi, I'm Deposit API Gateway!");

app.UseEndpoints(_ => { });

app.UseOcelot(builder.Configuration);

app.Run();