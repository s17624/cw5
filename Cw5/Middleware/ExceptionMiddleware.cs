using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Cw5.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch(Exception exp)
            {
                await HandleExceptionAsync(context, exp);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exp)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsync(new Model.ErrorDetail
            {
                statusCode = StatusCodes.Status500InternalServerError,
                message = "Blad: " + exp

            }.ToString());
        }
    }
}
