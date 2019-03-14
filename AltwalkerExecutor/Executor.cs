using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Altom.Altwalker {
    /// <summary>
    /// Executes steps from registered models and setup
    /// </summary>
    public class Executor {
        readonly IDictionary<string, Type> models;
        IDictionary<Type, object> modelInstances;

        /// <summary>
        /// Creates a new executor
        /// </summary>
        /// <param name="models">The model types</param>
        /// <param name="setup">The setup class containing setUpRun and tearDownRun</param>
        public Executor(IEnumerable<Type> models, Type setup=null)
        {
            this.models = models.ToDictionary(model => model.Name);
            if ( setup != null)
                this.models["Setup"] = setup;
            this.modelInstances = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Checks if a type with the given `modelName` is registered
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns>True if the type exists</returns>
        public bool HasModel (string modelName) {
            return GetModelType (modelName) != null;
        }


        /// <summary>
        /// Checks if the type registered for `modelName` has a public method named `stepName`
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>True if the method exists</returns>
        public bool HasStep (string modelName, string name) {
            var type = GetModelType (modelName);
            if (type == null)
                return false;

            if (GetMethodInfo (type, name) == null)
                return false;

            return true;
        }

        /// <summary>
        /// Executes the public method `name` from the type `modelName` registered.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>Trace output written during execution of method.</returns>
        public ExecuteStepResult ExecuteStep (string modelName, string name, IDictionary<string, dynamic> data=null) {
            data = data ?? new Dictionary<String,dynamic>();
            Type modelType = GetModelType (modelName);
            if (modelType == null)
            
                throw new ArgumentException (
                    $"No model named `{modelName}` was registered in the executor service." + 
                    $"Consider using ExecutorService.RegisterModel<{modelName}>() or ExecutorService.RegisterSetup<T>(). ", nameof(modelName));

            MethodInfo stepMethod = GetMethodInfo (modelType, name);
            if (stepMethod == null)
                throw new ArgumentException (
                    $"No public method named `{name}` was found in class `{modelName}`. "+
                    $"Check that the model is registered and the public method `{name}` exists. ", nameof(name)
                );

            return ExecuteStep(modelType, stepMethod, data);
        }

        public void Reset()
        {
            modelInstances = new Dictionary<Type, object>();
        }

        private ExecuteStepResult ExecuteStep(Type model, MethodInfo stepMethod, IDictionary<String,dynamic> data)
        {
            object instance = GetInstance(model);
            using (StepTraceListener stepTrace = new StepTraceListener ()) {
                var result = new ExecuteStepResult();
                
                try {
                    var parameters = stepMethod.GetParameters();
                    if ( parameters.Length == 1 && parameters[0].ParameterType == typeof (IDictionary<string, dynamic>))
                    {
                        result.data = data;
                        stepMethod.Invoke(instance,new object[] {data});
                    }
                    else
                        stepMethod.Invoke (instance, null);
                }
                catch (TargetInvocationException tie) {
                    throw new StepExecutionException(tie.InnerException);
                }
                result.output = stepTrace.GetOutput();

                return result;
            }
        }

        private Type GetModelType (string modelName) {
            
            if ( string.IsNullOrEmpty(modelName))
                modelName = "Setup";
            models.TryGetValue(modelName, out Type model);
            return model;
        }

        private MethodInfo GetMethodInfo (Type modelType, string stepName) {
            return modelType.GetMethods( ).SingleOrDefault (m => m.Name == stepName);
        }

        private object GetInstance(Type type)
        {
            if ( !modelInstances.TryGetValue(type, out object instance)) 
            {
                instance = Activator.CreateInstance (type);
                modelInstances.Add(type, instance);
            }    
            
            return instance;
        }
    }

    public class ExecuteStepResult {
        public string output { get; set; }
        public IDictionary<string, dynamic> data {get;set;}
    }


    /// <summary>
    /// Exception occurred during step execution
    /// </summary>
    public class StepExecutionException : Exception
    {
        public StepExecutionException(Exception innerException) : base(string.Empty, innerException)
        {
        }
    }
    internal class StepTraceListener : IDisposable {
        MemoryStream memoryStream;
        TraceListener traceListener;
        public StepTraceListener () {
            memoryStream = new MemoryStream ();
            traceListener = new TextWriterTraceListener (memoryStream);
            Trace.Listeners.Add (traceListener);
        }

        public string GetOutput () {
            traceListener.Flush ();
            memoryStream.Position = 0;
            using (StreamReader reader = new StreamReader (memoryStream)) {
                return reader.ReadToEnd ();
            }
        }
        public void Dispose () {
            Trace.Listeners.Remove (traceListener);
            traceListener.Dispose ();
            memoryStream.Dispose ();
        }
    }
}