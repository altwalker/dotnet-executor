using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Altwalker.Executor
{
    public class ExampleModel
    {
        public void vertex_1()
        {
            Trace.WriteLine("vertex_1");
            Assert.IsTrue(true);
        }

        public void edge_1()
        {
            Trace.WriteLine("edge_1");
            Assert.IsTrue(true);
        }

        public void vertex_2()
        {
            Trace.WriteLine("vertex_2");
            Assert.IsTrue(true);
        }
    }
    public class Setup
    {
        public void SetupRun()
        {}
    }
}