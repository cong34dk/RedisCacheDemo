using System.Diagnostics;

namespace RedisCacheDemo.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context); // Gọi Middleware tiếp theo trong pipeline

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // Ghi log thời gian phản hồi
            _logger.LogInformation($"[{context.Request.Method}] {context.Request.Path} - {elapsedMilliseconds} ms");
        }
    }
}
