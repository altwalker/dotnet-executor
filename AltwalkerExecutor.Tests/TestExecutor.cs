using System;
using System.Collections.Generic;
using System.Diagnostics;
using Altom.Altwalker;
using NUnit.Framework;

namespace Tests {
    public class TestExecutor {
        Executor _executor;
        Executor _executorWithStartup;
        [SetUp]
        public void Setup () {
            List<Type> types = new List<Type> () { typeof (ModelExample) };
            _executor = new Executor (types);
            _executorWithStartup = new Executor(new List<Type>{}, typeof(Startup));
        }

        [Test]
        public void HasModel () {
            Assert.IsTrue(_executor.HasModel("ModelExample"));
            Assert.IsFalse(_executor.HasModel("InvalidModel"));
        }

        [Test]
        public void HasStep () {
            Assert.IsTrue(_executor.HasStep("ModelExample", "Success_NoTrace"));
            Assert.IsFalse(_executor.HasStep("InvalidModel", "Success_NoTrace"));
            Assert.IsFalse(_executor.HasStep("ModelExample","InvalidStep"));
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
            Assert.AreEqual("Output\n",result.output);

            result = _executor.ExecuteStep("ModelExample", "Fail");
            Assert.That(result.error.message, Is.EqualTo("Throwing exception from step named Fail"));
            Assert.That(result.error.trace, Does.Contain("Tests.ModelExample.Fail"));
        }

        
        [Test]
        public void ExecuteStep_WithData()
        {
            var data =  new Dictionary<string,dynamic>();
            var result = _executor.ExecuteStep("ModelExample", "MethodSetData", data);
            Assert.AreEqual(true, result.data["passed"]);
        }

        [Test]
        public void ExecuteStep_NoDataParameter()
        {
            var result = _executor.ExecuteStep("ModelExample", "MethodNoData", new Dictionary<string,dynamic>());
            Assert.AreEqual(string.Empty, result.output);
        }

        [Test]
        public void ExecuteStep_InexistentModel()
        {
            var result = _executor.ExecuteStep("Inexistent", "MyStep");

            Assert.That(result.error.message, Is.EqualTo("No model named `Inexistent` was registered in the executor service."+
            " Consider using ExecutorService.RegisterModel<Inexistent>() or ExecutorService.RegisterSetup<T>(). "));
        }

        [Test]
        public void ExecuteStep_InvalidStep()
        {
            var result = _executor.ExecuteStep("ModelExample", "StepDoesNotExist");

            Assert.That(result.error.message, Is.EqualTo("No public method named `StepDoesNotExist` was found in class `ModelExample`. "+
            "Check that the model is registered and the public method `StepDoesNotExist` exists."));
        }
    }

    public class ModelExample {
        public void Success_NoTrace () {

        }
        public void Success_WithTrace () {
            Trace.WriteLine("Output");
        }

        public void Fail () {
            Trace.WriteLine("Fail");
            throw new Exception("Throwing exception from step named Fail");
        }

        public void MethodSetData(IDictionary<string, dynamic> data)
        {
            data["passed"]=true;
        }

        public void MethodNoData()
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