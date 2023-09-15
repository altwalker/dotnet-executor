using Altom.AltWalker;
using NUnit.Framework;
using Moq;
using Altom.AltWalker.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Tests.Unit.Controllers
{
    [TestFixture]
    public class TestAltwalkerController
    {
        AltwalkerController controller;
        Mock<IExecutor> executorMock;

        [SetUp]
        public void SetUpTest()
        {
            executorMock = new Mock<IExecutor>();
            controller = new AltwalkerController(executorMock.Object);
        }

        [Test]
        public void HasStep()
        {
            //Mock.Get(executorMock).Setup(m=> m.HasStep(It.Is<string>(modelName=>modelName == "MyModel"), It.Is<string>(name=>name == "myStep"))).Returns(true);
            executorMock
                .Setup(m => m.HasStep(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((modelName, name) => modelName == "MyModel" && name == "myStep");

            dynamic data = ((JsonResult)controller.HasStep("MyModel", "myStep")).Value;
            Assert.That(data.payload.hasStep, Is.True);

            data = ((JsonResult)controller.HasStep("MyModel", "inexistentStep")).Value;
            Assert.That(data.payload.hasStep, Is.False);
        }


        [Test]
        public void HasModel()
        {
            //Mock.Get(executorMock).Setup(m=> m.HasStep(It.Is<string>(modelName=>modelName == "MyModel"), It.Is<string>(name=>name == "myStep"))).Returns(true);
            executorMock
                .Setup(m => m.HasModel(It.IsAny<string>()))
                .Returns(true)
                .Verifiable();

            dynamic data = ((JsonResult)controller.HasModel("MyModel")).Value;
            Assert.That(data.payload.hasModel, Is.True);
        }

        [Test]
        public void ExecuteStep()
        {
            ExecuteStepResult result = new ExecuteStepResult
            {
                output = "output",
                data = new Dictionary<string, dynamic>() { { "key", "value" } },
                error = new AltwalkerError { message = "message", trace = "trace" }
            };
            executorMock
                .Setup(m => m.ExecuteStep(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, dynamic>>()))
                .Returns(result)
                .Verifiable();

            dynamic data = ((JsonResult)controller.ExecuteStep("model", "step", null)).Value;
            Assert.That(data.payload.output, Is.EqualTo("output"));
            Assert.That(data.payload.data["key"], Is.EqualTo("value"));
            Assert.That(data.payload.error.message, Is.EqualTo("message"));
            Assert.That(data.payload.error.trace, Is.EqualTo("trace"));

            executorMock.Verify();
        }

        [Test]
        public void Reset()
        {
            executorMock
                .Setup(m => m.Reset()).Verifiable();

            var result = (StatusCodeResult)controller.Reset();
            Assert.That(result.StatusCode, Is.EqualTo(200));
            executorMock.Verify();
        }
    }
}
