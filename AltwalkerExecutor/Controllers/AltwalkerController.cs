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
    [Route("[controller]")]
    [HandleException]
    public class AltwalkerController : Controller
    {
        private readonly IExecutor executor;

        public AltwalkerController(IExecutor executor)
        {
            this.executor = executor;
        }

        [HttpGet("hasModel")]
        public ActionResult HasModel(string name)
        {
            var hasModel = executor.HasModel(name);
            return new PayloadResult(new { hasModel = hasModel });
        }

        [HttpGet("hasStep")]
        public ActionResult HasStep(string modelName, string name)
        {
            var hasStep = executor.HasStep(modelName, name);
            return new PayloadResult(new { hasStep = hasStep });
        }

        [HttpPost("executeStep")]
        public ActionResult ExecuteStep(string modelName, string name, [FromBody] JObject jData)
        {
            Dictionary<string, dynamic> data = null;
            dynamic json = jData;
            if (json != null && json.data != null)
            {
                data = json.data.ToObject<Dictionary<string, dynamic>>();
            }
            var result = executor.ExecuteStep(modelName, name, data);
            return new PayloadResult(result);
        }

        [HttpPut("reset")]
        public ActionResult Reset()
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
        public PayloadResult(object value) : base(new { payload = value })
        {
        }
    }
}
