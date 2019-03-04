using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altom.Altwalker.Controllers.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Altom.Altwalker.Controllers {
    
    [Route ("[controller]")]
    [HandleException]
    public class AltwalkerController : Controller {
        private readonly Executor executor;

        public AltwalkerController (Executor executor) {
            this.executor = executor;
        }

        [HttpGet ("hasmodel")]
        public ActionResult HasModel (string model) {
            if (executor.HasModel (model))
                return new JsonResult (new { status = "ok" });
            return new JsonResult (new { status = "nok" });
        }

        [HttpGet ("hasstep")]
        public ActionResult HasStep (string model, string step) {
            if (executor.HasStep (model, step))
                return new JsonResult (new { status = "ok" });
            return new JsonResult (new { status = "nok" });
        }

        [HttpPost ("executestep")]
        public ActionResult ExecuteStep (string model, string step) {
            try {
                var result =executor.ExecuteStep (model, step);
                return new JsonResult (new{ output = result.output});
            } catch (StepExecutionException ex) {
                return new JsonResult (new { error = ex.InnerException.ToString () });
            }
        }

        [HttpGet ("restart")]
        public ActionResult Restart () {
            executor.Restart();
            return new JsonResult (new { status = "ok" });
        }
    }
}