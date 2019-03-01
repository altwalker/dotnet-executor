﻿

using Altom.Altwalker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Altwalker.Csharp.Example {
    public class Program {
        public static void Main (string[] args) {
            ExecutorService service = new ExecutorService();
            service.RegisterModel<ExampleModel>();
            service.RegisterSetup<Setup>();
            service.Start(args);
        }
    }
}