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

        public async Task Get(str url)
        {
            return await httpClient.GetAsync(baseUri + uri);
        }

        public async Task Post(str url, str content)
        {
            return await httpClient.PostAsync(baseUri + url, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public async Task Put(str url, str content)
        {
            return await httpClient.PutAsync(baseUri + url, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public async dynamic GetJSON(Task response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);

            return json;
        }

        [Test]
        public async Task TestUnhandledUrls()
        {
            var response = Get("someRandomPath");
            var json = GetJSON(response);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("Handler not found", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestModelNotFound()
        {
            var response = Post("altwalker/executeStep?modelName=InvalidModel&name=step", string.Empty);
            var json = GetJSON(response);

            Assert.AreEqual(460, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("No model named `InvalidModel` was registered", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestStepNotFound()
        {
            var response = Post("altwalker/executeStep?modelName=ModelExample&name=invalidStep", string.Empty);
            var json = GetJSON(response);

            Assert.AreEqual(461, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("Method named `invalidStep` not found in class `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestInvalidHandler()
        {
            var response = Post("altwalker/executeStep?modelName=ModelExample&name=invalid_handler", string.Empty);
            var json = GetJSON(response);

            Assert.AreEqual(462, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("InvalidHandler for `invalid_handler` in type `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestMultipleHandlers()
        {
            var json = GetJSON(Post("altwalker/executeStep?modelName=ModelExample&name=multiple_overloads", string.Empty));

            Assert.IsNotNull(json);
            Assert.AreEqual("MultipleHandlers for `multiple_overloads` in type `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task TestExecuteStepWithData()
        {
            var content = @"{""data"": { ""key"":""value"" }}";
            var response = Post("altwalker/executeStep?modelName=ModelExample&name=handler_with_data", content);
            var json = GetJSON(response);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.data["key"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["copy"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["passed"].Value, Is.EqualTo(true));
        }

        [Test]
        public async Task TestExecuteStepWithContext()
        {
            var response = Post("altwalker/executeStep?modelName=ModelExample&name=handler_with_return_value", string.Empty);
            var json = GetJSON(response);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.result.Value, Is.EqualTo("message from the other side"));
        }

        [Test]
        public async Task TestLoad()
        {
            var content = @"{""path"": ""somepath""";
            var response = Post("altwalker/load", content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestReset()
        {
            var content = @"{""path"": ""somepath""";
            var response = Put("altwalker/reset", content);

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
