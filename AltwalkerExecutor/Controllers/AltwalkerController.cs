using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altom.AltWalker.Controllers.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Altom.AltWalker.Controllers
{
    [Route ("[controller]")]
    [HandleException]
    public class AltwalkerController : Controller
    {
        private readonly IExecutor executor;

        public AltwalkerController (IExecutor executor)
        {
            this.executor = executor;
        }

        [HttpGet("hasModel")]
        public ActionResult HasModel (string name)
        {
            var hasModel = executor.HasModel (name);
            return new PayloadResult(new { hasModel = hasModel });
        }

        [HttpGet("hasStep")]
        public ActionResult HasStep (string modelName, string name)
        {
            var hasStep = executor.HasStep (modelName, name);
            return new PayloadResult(new { hasStep = hasStep });
        }

        [HttpPost("executeStep")]
        public ActionResult ExecuteStep (string modelName, string name, [FromBody] JObject jData)
        {
            Dictionary<string,dynamic> data = null;
            dynamic json = jData;
            if (json != null && json.data != null)
            {
                data = json.data.ToObject<Dictionary<string, dynamic>>();
            }
            var result = executor.ExecuteStep(modelName, name, data);
            return new PayloadResult(result);
        }

        [HttpPut("reset")]
        public ActionResult Reset ()
        {
            executor.Reset();
            return new StatusCodeResult(200);
        }

        [HttpPost("load")]
        public ActionResult Load()
        {
            return new StatusCodeResult(200);
        }
    }

    public class PayloadResult : JsonResult
    {
        public PayloadResult(object value) : base(new {payload=value})
        {
        }
    }
}
