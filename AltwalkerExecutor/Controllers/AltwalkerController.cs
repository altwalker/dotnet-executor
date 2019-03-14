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

        [HttpGet ("hasModel")]
        public ActionResult HasModel (string name) {
            var hasModel = executor.HasModel (name);
            return new JsonResult (new { hasModel = hasModel });
        }

        [HttpGet ("hasStep")]
        public ActionResult HasStep (string modelName, string name) {
            var hasStep = executor.HasStep (modelName, name);
            return new JsonResult (new { hasStep = hasStep });
        }

        [HttpPost ("executeStep")]
        public ActionResult ExecuteStep (string modelName, string name, [FromBody] Dictionary<String, dynamic> data) {
            try {
                var result = executor.ExecuteStep (modelName, name, data);
                return new JsonResult (result);
            } catch (StepExecutionException ex) {
                return new JsonResult (new { error = ex.InnerException.ToString () });
            } catch (ArgumentException ex) {
                return new JsonResult (new { error = ex.ToString () });
            }
        }

        [HttpGet ("reset")]
        public ActionResult Reset () {
            executor.Reset ();
            return new JsonResult (new { status = "ok" });
        }
    }
}