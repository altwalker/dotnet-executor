using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;

namespace Altom.Altwalker {
    /// <summary>
    /// Provides methods to start and stop Altwalker executor host
    /// </summary>
    public class ExecutorService {
        IWebHost host = null;
        HashSet<Type> models = new HashSet<Type> ();
        Type setup = null;

        
        /// <summary>
        /// Starts listening on the address provided in args
        /// </summary>
        public async Task StartAsync (string[] args) {
            InitHost(args);
            await host.StartAsync();
        }

        /// <summary>
        /// Starts listening on the address provided in args
        /// </summary>
        public void Start (string[] args) {
            StartAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the altwalker executor http server and returns a task that only completes when the shutdown is triggered
        /// </summary>
        public async Task RunAsync (string[] args) {
            InitHost(args);
            await host.RunAsync();
        }

        /// <summary>
        /// Runs the altwalker executor and blocks the current trhread until host shutdown
        /// </summary>
        public void Run (string[] args) {
            RunAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Register setup Type 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterSetup<T> () {
            RegisterSetup (typeof (T));
        }

        /// <summary>
        /// Register setup Type 
        /// </summary>
        public void RegisterSetup (Type t) {
            RequireServiceNotStarted ();
            setup = t;
        }

        /// <summary>
        /// Register model type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterModel<T> () {
            RegisterModels (typeof (T));
        }

        /// <summary>
        /// Register model types
        /// </summary>
        /// <param name="models"></param>
        public void RegisterModels (params Type[] models) {
            RequireServiceNotStarted ();
            foreach (var model in models)
                this.models.Add (model);
        }

        /// <summary>
        /// Attempt to gracefully stop the host
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync () {          

            await host.StopAsync ();
        }

        private void RequireServiceNotStarted () {
            if (host != null)
                throw new Exception ("Service already started. Register the models before starting the service.");
        }

        private void InitHost(string[] args)
        {
            string url = GetUrl(args);
            this.host = new WebHostBuilder ()
                .UseKestrel ()
                .UseUrls(url)                
                .ConfigureServices (services => {
                    services.AddSingleton (typeof (IExecutor), new Executor (models, setup));
                    services.AddMvc ();
                })
                .Configure (app => {
                    app.UseMvc();
                    app.Use(async (context, next) => {
                        await next();
                        if (context.Response.StatusCode == 404)
                        {
                            var err = new { error = new {message="Handler not found" }};
                            
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(err));
                        }
                    });
                })
                .Build ();
        }
        private string GetUrl(string [] args)
        {
            string url = "http://localhost:5000";
            if ( args.Length > 0)
            {
                var arg = args.FirstOrDefault(a=> a.StartsWith("--server.urls="));
                url = arg.Split('=').Last();
            }
            return url;
        }
    }
}