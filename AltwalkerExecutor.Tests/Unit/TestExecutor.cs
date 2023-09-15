using System;
using System.Collections.Generic;
using System.Diagnostics;
using Altom.AltWalker;
using NUnit.Framework;

namespace Tests.Unit
{
    [TestFixture]
    public class TestExecutor
    {
        Executor _executor;
        Executor _executorWithStartup;

        [SetUp]
        public void Setup()
        {
            List<Type> types = new List<Type>() { typeof(ModelExample) };
            _executor = new Executor(types);
            _executorWithStartup = new Executor(new List<Type> { }, typeof(Startup));
        }

        [Test]
        public void HasModel()
        {
            Assert.IsTrue(_executor.HasModel("ModelExample"));
            Assert.IsFalse(_executor.HasModel("InvalidModel"));
        }

        [Test]
        public void HasStep()
        {
            Assert.IsTrue(_executor.HasStep("ModelExample", "Success_NoTrace"));
            Assert.IsFalse(_executor.HasStep("InvalidModel", "Success_NoTrace"));
            Assert.IsFalse(_executor.HasStep("ModelExample", "InvalidStep"));
        }

        [Test]
        public void HasStep_HasSetupRun()
        {
            Assert.IsTrue(_executorWithStartup.HasStep(null, "SetupRun"));
            Assert.IsFalse(_executor.HasStep(null, "SetupRun"));
        }

        [Test]
        public void ExecuteStep()
        {
            var result = _executor.ExecuteStep("ModelExample", "Success_NoTrace");
            Assert.AreEqual(string.Empty, result.output);

            result = _executor.ExecuteStep("ModelExample", "Success_WithTrace");
            Assert.AreEqual("Output" + System.Environment.NewLine, result.output);

            result = _executor.ExecuteStep("ModelExample", "Fail");
            Assert.That(result.error.message, Is.EqualTo("Throwing exception from step named Fail"));
            Assert.That(result.error.trace, Does.Contain("Tests.Unit.ModelExample.Fail"));
        }


        [Test]
        public void ExecuteStep_WithData()
        {
            var data = new Dictionary<string, dynamic>();
            var result = _executor.ExecuteStep("ModelExample", "MethodSetData", data);
            Assert.AreEqual(true, result.data["passed"]);
        }

        [Test]
        public void ExecuteStep_NoDataParameter()
        {
            var result = _executor.ExecuteStep("ModelExample", "MethodNoData", new Dictionary<string, dynamic>());
            Assert.AreEqual(string.Empty, result.output);
        }

        [Test]
        public void ExecuteStep_InexistentModel()
        {
            var ex = Assert.Throws<ModelNotFoundException>(() => _executor.ExecuteStep("Inexistent", "MyStep"));

            Assert.That(ex.Message, Is.EqualTo("No model named `Inexistent` was registered"));
        }

        [Test]
        public void ExecuteStep_InvalidStep()
        {
            var ex = Assert.Throws<StepNotFoundException>(() => _executor.ExecuteStep("ModelExample", "StepDoesNotExist"));

            Assert.That(ex.Message, Is.EqualTo("Method named `StepDoesNotExist` not found in class `ModelExample`."));
        }

        [Test]
        public void ExecuteStep_ReturnValue()
        {
            var stepResult = _executor.ExecuteStep("ModelExample", "MethodReturnValue");
            Assert.That(stepResult.result, Is.EqualTo(100));
        }

        [Test]
        public void ExecuteStep_NoReturnValue()
        {
            var stepResult = _executor.ExecuteStep("ModelExample", "MethodNoReturnValue");
            Assert.That(stepResult.result, Is.Null);
        }
    }

    public class ModelExample
    {
        public void Success_NoTrace()
        {
        }

        public void Success_WithTrace()
        {
            Trace.WriteLine("Output");
        }

        public void Fail()
        {
            Trace.WriteLine("Fail");
            throw new Exception("Throwing exception from step named Fail");
        }

        public void MethodSetData(IDictionary<string, dynamic> data)
        {
            data["passed"] = true;
        }

        public void MethodNoData()
        {
        }

        public int MethodReturnValue()
        {
            return 100;
        }

        public void MethodNoReturnValue()
        {
        }
    }

    public class Startup
    {
        public void SetupRun()
        {
        }
    }
}
