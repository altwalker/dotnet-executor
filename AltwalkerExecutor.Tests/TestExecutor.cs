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
        public void ExectuorWithStartup_HasSetupRun()
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

            StepExecutionException ex = Assert.Throws<StepExecutionException>(
                () => _executor.ExecuteStep("ModelExample", "Fail"));

            Assert.That(ex.InnerException.Message, Is.EqualTo("Exception message"));
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
            throw new Exception("Exception message");
        }
    }

    public class Startup
    {
        public void SetupRun()
        {
        }
    }
}