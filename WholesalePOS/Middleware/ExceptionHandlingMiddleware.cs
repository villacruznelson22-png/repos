using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Domain.Exceptions;

namespace WholesalePOS.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            try
            {
                await _next(context);
            }

            catch (Exception ex)
            {
                var statusCode = ex switch
                {

                    DomainException => StatusCodes.Status400BadRequest,

                    NotFoundException => StatusCodes.Status404NotFound,

                    ConflictException => StatusCodes.Status409Conflict,

                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;

                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = GetTitle(statusCode),
                    Detail = ex.Message
                };


                await context.Response.WriteAsJsonAsync(problem);
            }
        }

        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status404NotFound => "Resource Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status400BadRequest => "Validation Failed",
                _ => "Server Error"
            };
        }
    }
}
