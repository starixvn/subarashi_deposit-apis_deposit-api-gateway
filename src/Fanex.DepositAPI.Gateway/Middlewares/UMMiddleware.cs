namespace Fanex.DepositAPI.Gateway.Middlewares;

using System.Net;
using ONEbook.UM.Services;

public class UMMiddleware(RequestDelegate next, IMaintenanceService maintenanceService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var umInfo = await maintenanceService.GetUnderMaintenanceTimeAsync();
        if (umInfo.IsUnderMaintenanceTime)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            var message = $"Sorry, the system is under maintenance from {umInfo.From} to {umInfo.To} GMT+8. Please come back later.";
            await context.Response.WriteAsync(message);
            return;
        }

        await next(context);
    }
}