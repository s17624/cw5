using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw5.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        public LoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if(context.Request != null)
            {
                String path = context.Request.Path;
                String method = context.Request.Method;
                String qString = context.Request.QueryString.ToString();
                String bString = "";

                using(var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bString = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                String file = @"requestLog.txt";
                if(!File.Exists(file))
                {
                    File.Create(file);
                }

                using (var sw = new StreamWriter(file, true))
                {
                    sw.WriteLine(String.Format("{0} LoggingMiddleware: Method = {1}; Path = {2}; Body = {3}, Query = {4}", DateTime.Now, method, path, bString, qString));
                }
            }
            if(next != null)
            {
                await next(context);
            }
        }
    }
}
