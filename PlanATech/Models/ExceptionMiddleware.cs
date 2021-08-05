using LoggerService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PlanATech.Models
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;
        public ExceptionMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _logger = logger;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            // to show
            catch (AccessViolationException ex)
            {
                _logger.LogError($"A new violation exception has been thrown: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
            // for example other exception types ->
            //catch (FileNotFoundException ex)
            //{
            //    Console.WriteLine($"The file was not found: '{ex}'");
            //    await HandleExceptionAsync(httpContext, ex);
            //}
            //catch (DirectoryNotFoundException ex)
            //{
            //    Console.WriteLine($"The directory was not found: '{ex}'");
            //    await HandleExceptionAsync(httpContext, ex);
            //}
            //catch (IOException ex)
            //{
            //    Console.WriteLine($"The file could not be opened: '{ex}'");
            //    await HandleExceptionAsync(httpContext, ex);
            //}
            // <-
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error."
            }.ToString());
        }
    }
}
