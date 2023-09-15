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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Altom.AltWalker;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Integration
{
    [TestFixture]
    public class TestRestApi
    {
        Uri baseUri;
        ExecutorService executorService;
        HttpClient httpClient;

        [OneTimeSetUp]
        public void Init()
        {
            executorService = new ExecutorService();
            executorService.RegisterModel<ModelExample>();

            baseUri = new Uri("http://localhost:5037/");
            Console.WriteLine(baseUri.ToString());

            executorService.Start(new string[] { "--server.urls=" + baseUri.ToString() });
            httpClient = new HttpClient();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            executorService.StopAsync().GetAwaiter().GetResult();
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await httpClient.GetAsync(baseUri + url);
        }

        public async Task<HttpResponseMessage> Post(string url, string content)
        {
            return await httpClient.PostAsync(baseUri + url, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> Put(string url, string content)
        {
            return await httpClient.PutAsync(baseUri + url, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public async Task<dynamic> GetJSON(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);

            return json;
        }

        [Test]
        public async Task TestUnhandledUrls()
        {
            var response = await Get("someRandomPath");
            var json = await GetJSON(response);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("Handler not found", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestModelNotFound()
        {
            var response = await Post("altwalker/executeStep?modelName=InvalidModel&name=step", string.Empty);
            var json = await GetJSON(response);

            Assert.AreEqual(460, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("No model named `InvalidModel` was registered", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestStepNotFound()
        {
            var response = await Post("altwalker/executeStep?modelName=ModelExample&name=invalidStep", string.Empty);
            var json = await GetJSON(response);

            Assert.AreEqual(461, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("Method named `invalidStep` not found in class `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestInvalidHandler()
        {
            var response = await Post("altwalker/executeStep?modelName=ModelExample&name=invalid_handler", string.Empty);
            var json = await GetJSON(response);

            Assert.AreEqual(462, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("InvalidHandler for `invalid_handler` in type `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestMultipleHandlers()
        {
            var json = await GetJSON(await Post("altwalker/executeStep?modelName=ModelExample&name=multiple_overloads", string.Empty));

            Assert.IsNotNull(json);
            Assert.AreEqual("MultipleHandlers for `multiple_overloads` in type `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestExecuteStepWithData()
        {
            var content = @"{""data"": { ""key"":""value"" }}";
            var response = await Post("altwalker/executeStep?modelName=ModelExample&name=handler_with_data", content);
            var json = await GetJSON(response);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.data["key"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["copy"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["passed"].Value, Is.EqualTo(true));
        }

        [Test]
        public async Task TestExecuteStepWithContext()
        {
            var response = await Post("altwalker/executeStep?modelName=ModelExample&name=handler_with_return_value", string.Empty);
            var json = await GetJSON(response);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.result.Value, Is.EqualTo("message from the other side"));
        }

        [Test]
        public async Task TestLoad()
        {
            var content = @"{""path"": ""somepath""";
            var response = await Post("altwalker/load", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestReset()
        {
            var content = @"{""path"": ""somepath""";
            var response = await Put("altwalker/reset", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }

    public class ModelExample
    {
        public void vertex_one()
        {
        }

        public void invalid_handler(string expecting_something)
        {
        }

        public void multiple_overloads()
        {
        }

        public void multiple_overloads(int x)
        {
        }

        public void handler_with_data(IDictionary<string, object> data)
        {
            data["passed"] = true;
            data["copy"] = data["key"];
        }

        public string handler_with_return_value()
        {
            return "message from the other side";
        }
    }
}
