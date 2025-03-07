namespace RedisCacheDemo.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Request successful";
        public T Data { get; set; }
        public long ResponseTimeMs { get; set; } // Thời gian phản hồi (ms)
        public string Source { get; set; } // Nguồn dữ liệu (Cache hoặc Database)

        public ApiResponse(T data, long responseTimeMs, string source, string message = "Request successful", bool success = true)
        {
            Data = data;
            ResponseTimeMs = responseTimeMs;
            Source = source;
            Message = message;
            Success = success;
        }
    }
}
