using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerceAPI.Models
{
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; } //Data type is object so that it can accept any kind of Errors

        public APIResponse(T data, string message ="", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Success = true;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Error = null;
        }

        public APIResponse(HttpStatusCode statusCode ,string message, object error = null)
        {
            Success = false;
            StatusCode = statusCode;
            Message = message;
            Data = default(T);
            Error = null;
        }
    }
}
