using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altom.AltWalker.Controllers.Filters
{
    public class HandleExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var error = new {
                message = context.Exception.Message,
                trace = context.Exception.StackTrace
            };
            context.HttpContext.Response.StatusCode=500;

            if (context.Exception.GetType() == typeof(ModelNotFoundException))
            {
                context.HttpContext.Response.StatusCode = 460;
            }
            if (context.Exception.GetType() == typeof(StepNotFoundException))
            {
                context.HttpContext.Response.StatusCode = 461;
            }
            if (context.Exception.GetType() == typeof(InvalidStepHandlerException))
            {
                context.HttpContext.Response.StatusCode = 462;
            }

            context.Result = new JsonResult(new { error });
        }
    }
}
