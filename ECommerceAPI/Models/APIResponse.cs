using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerceAPI.Models
{

    //This class returns the Same API response for both Succssful or Unsuccessful result.
    //This is a generic class so it can accept all type Data Type.
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; } //Data type is object so that it can accept any kind of Errors

        //Constructor for Successful reponse
        public APIResponse(T data, string message ="", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Success = true;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Error = null;
        }

        //Constructor for UnSuccessful reponse
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
