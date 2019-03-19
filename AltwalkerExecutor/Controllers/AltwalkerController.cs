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
        private readonly IExecutor executor;

        public AltwalkerController (IExecutor executor) {
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
            var result = executor.ExecuteStep (modelName, name, data);
            return new JsonResult (result);
        }

        [HttpGet ("reset")]
        public ActionResult Reset () {
            executor.Reset ();
            return new JsonResult (new { status = "ok" });
        }
    }
}