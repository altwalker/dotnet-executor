//    Copyright(C) 2023 Altom Consulting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program. If not, see <https://www.gnu.org/licenses/>.

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
            var error = new
            {
                message = context.Exception.Message,
                trace = context.Exception.StackTrace
            };
            context.HttpContext.Response.StatusCode = 500;

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
