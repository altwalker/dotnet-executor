

using System;
using Altom.Altwalker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Altwalker.Executor {
    public class Program {
        public static void Main (string[] args) {
            ExecutorService service = new ExecutorService();
            service.RegisterModel<WalletModel>();
            service.RegisterSetup<Setup>();
            service.Run(args);
        }
    }
}