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
            Console.WriteLine("OneTimeSetUp");
            executorService = new ExecutorService();
            executorService.RegisterModel<ModelExample>();
            baseUri = new Uri("http://localhost:5037/");
            Console.WriteLine(baseUri.ToString());
            executorService.Start(new string[] { "--server.urls=" + baseUri.ToString() });
            httpClient = new HttpClient();
        }

        [Test]
        public async Task Test_Rest_UnhandledUrls()
        {
            var response = await httpClient.GetAsync(baseUri + "someRandomPath");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.IsNotNull(json);
            Assert.AreEqual("Handler not found", json["error"]["message"].Value);
        }

        [Test]
        public async Task Test_Rest_ModelNotFound()
        {
            var response = await httpClient.PostAsync(baseUri + "altwalker/executeStep?modelName=invalidmodel&name=step",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            Assert.AreEqual(460, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.IsNotNull(json);
            Assert.AreEqual("No model named `invalidmodel` was registered", json["error"]["message"].Value);
        }

        [Test]
        public async Task Test_Rest_StepNotFound()
        {
            var response = await httpClient.PostAsync(baseUri + "altwalker/executeStep?modelName=ModelExample&name=invalidStep",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            Assert.AreEqual(461, (int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);
            Assert.IsNotNull(json);
            Assert.AreEqual("Method named `invalidStep` not found in class `ModelExample`.", json["error"]["message"].Value);
        }


        [Test]
        public async Task Test_Rest_InvalidHandler()
        {
            var response = await httpClient.PostAsync(baseUri + "altwalker/executeStep?modelName=ModelExample&name=invalid_handler",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.AreEqual(462, (int)response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual("InvalidHandler for `invalid_handler` in type `ModelExample`.", json["error"]["message"].Value);
        }

        public async dynamic PostExecuteStep(str url, str content) {
            var response = await httpClient.PostAsync(baseUri + url, new StringContent(content, Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(responseContent);

            return json;
        }

        [Test]
        public async Task Test_Rest_MultipleHandlers()
        {
            var json = PostExecuteStep("altwalker/executeStep?modelName=ModelExample&name=multiple_overloads", string.Empty);

            Assert.IsNotNull(json);
            Assert.AreEqual("MultipleHandlers for `multiple_overloads` in type `ModelExample`.", json["error"]["message"].Value);
        }

        [Test]
        public async Task Test_Rest_ExecuteStepWithData()
        {
            var content = @"{""data"": { ""key"":""value"" }}";
            var json = PostExecuteStep("altwalker/executeStep?modelName=ModelExample&name=handler_with_data", content);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.data["key"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["copy"].Value, Is.EqualTo("value"));
            Assert.That(json.payload.data["passed"].Value, Is.EqualTo(true));
        }

        [Test]
        public async Task Test_Rest_ExecuteStepWithContext()
        {
            // handler_with_return_value
            // return "message from the other side";
            var response = await httpClient.PostAsync(baseUri + "altwalker/executeStep?modelName=ModelExample&name=handler_with_return_value",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.That(json.payload.result.Value, Is.EqualTo("message from the other side"));
        }

        [Test]
        public async Task Test_Rest_Load()
        {
            var content = @"{""path"": ""somepath""";
            var response = await httpClient.PostAsync(baseUri + "altwalker/load",
                new StringContent(content, Encoding.UTF8, "application/json"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Test_Rest_Reset()
        {
            var content = @"{""path"": ""somepath""";
            var response = await httpClient.PutAsync(baseUri + "altwalker/reset",
                new StringContent(content, Encoding.UTF8, "application/json"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            executorService.StopAsync().GetAwaiter().GetResult();
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
