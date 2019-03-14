using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Altom.Altwalker {
    public class ExecutorService {
        IWebHost host = null;
        HashSet<Type> models = new HashSet<Type> ();
        Type setup = null;
        public void Start (string[] args) {

            var config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();
            host = new WebHostBuilder ()
                .UseKestrel ()
                .UseConfiguration(config)
                .ConfigureServices (services => {
                    services.AddSingleton (typeof (Executor), new Executor (models, setup));
                    services.AddMvc ();
                })
                .Configure (applicationBuilder => {
                    applicationBuilder.UseMvc();
                })
                .Build ();

            host.Run ();
        }

        public void RegisterSetup<T> () {
            RegisterSetup (typeof (T));
        }

        public void RegisterSetup (Type t) {
            RequireServiceNotStarted ();
            setup = t;
        }

        public void RegisterModel<T> () {
            RegisterModels (typeof (T));
        }

        public void RegisterModels (params Type[] models) {
            RequireServiceNotStarted ();
            foreach (var model in models)
                this.models.Add (model);
        }

        public async Task StopAsync () {
            await host.StopAsync ();
        }

        private void RequireServiceNotStarted () {
            if (host != null)
                throw new Exception ("Service already started. Register the models before starting the service.");
        }
    }
}