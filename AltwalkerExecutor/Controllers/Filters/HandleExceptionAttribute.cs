using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altom.Altwalker.Controllers.Filters {
    public class HandleExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var error = new { 
                message = context.Exception.Message,
                trace = context.Exception.StackTrace
            };
            context.Result = new JsonResult (new { error });
        }
    }
}