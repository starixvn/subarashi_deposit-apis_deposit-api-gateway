namespace Fanex.DepositAPI.Gateway.Utils;

using System.Text;

public static class ErrorMessageUtils
{
    private const string BreakLine = "\r\n";

    public static string GenerateMessage(Exception exception, HttpContext context = null)
    {
        var messageBuilder = new StringBuilder();

        messageBuilder.AppendLine($"ErrMsg: {exception.Message}");

        if (context != null)
        {
            AddRequestInfo(context, messageBuilder);
        }

        const int limitedNumOfInnerException = 3;
        AddExceptionInfo(messageBuilder, exception, limitedNumOfInnerException);

        return messageBuilder.ToString();
    }

    private static void AddRequestInfo(HttpContext context, StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("REQUEST INFO");

        var webInfo = GetRequestInfo(context);

        foreach (var item in webInfo)
        {
            builder.AppendLine($"{item.Key}: {item.Value}");
        }
    }

    private static void AddExceptionInfo(StringBuilder builder, Exception exception, int limitedInnerException)
    {
        builder.AppendLine();
        builder.AppendLine("EXCEPTION INFO");
        builder.AppendLine($"Message: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"TargetSite: {exception.TargetSite}");
        builder.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException != null && limitedInnerException > 0)
        {
            AddExceptionInfo(builder, exception.InnerException, limitedInnerException - 1);
        }
    }

    private static IDictionary<string, object> GetRequestInfo(HttpContext context)
    {
        var request = context.Request;
        return new Dictionary<string, object>
        {
            ["Host"] = request.Host,
            ["Path"] = request.Path,
            ["Query String"] = request.QueryString,
            ["Method"] = request.Method,
            ["Remote IP"] = context.Connection?.RemoteIpAddress,
            ["Local IP"] = context.Connection?.LocalIpAddress,
            ["User Agent"] = request.Headers.UserAgent.ToString(),
            ["Content Type"] = request.ContentType,
            ["Content Length"] = request.ContentLength.HasValue ? request.ContentLength.Value.ToString() : "N/A",
            ["Request Time"] = DateTime.UtcNow.ToString("o"),
            ["Headers"] = GetHeaderValues(request),
            ["Cookies"] = GetCookiesValues(request),
            ["Body"] = GetRawBodyStringAsync(request)
        };
    }

    private static async Task<string> GetRawBodyStringAsync(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static string GetHeaderValues(HttpRequest request)
        => string.Join(BreakLine, request.Headers.Select(header => $"{header.Key}:{header.Value}"));

    private static string GetCookiesValues(HttpRequest request)
        => string.Join(BreakLine, request.Cookies.Select(header => $"{header.Key}:{header.Value}"));
}