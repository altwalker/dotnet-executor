using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Altom.AltWalker
{
    /// <summary>
    /// Executes steps from registered models and setup
    /// </summary>
    public class Executor : IExecutor
    {
        readonly IDictionary<string, Type> models;
        IDictionary<Type, object> modelInstances;

        /// <summary>
        /// Creates a new executor
        /// </summary>
        /// <param name="models">The model types</param>
        /// <param name="setup">The setup class containing setUpRun and tearDownRun</param>
        public Executor(IEnumerable<Type> models, Type setup = null)
        {
            this.models = models.ToDictionary(model => model.Name);
            if (setup != null)
                this.models["Setup"] = setup;
            this.modelInstances = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Checks if a type with the given `modelName` is registered
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns>True if the type exists</returns>
        public bool HasModel(string modelName)
        {
            return GetModelType(modelName) != null;
        }

        /// <summary>
        /// Checks if the type registered for `modelName` has a public method named `stepName`
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>True if the method exists</returns>
        public bool HasStep(string modelName, string name)
        {
            var type = GetModelType(modelName);
            if (type == null)
            {
                return false;
            }

            var handlerType = TryGetStepHandler(type, name, out MethodInfo method);
            if (handlerType == StepHandlerType.Data || handlerType == StepHandlerType.NoData)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Executes the public method `name` from the type `modelName` registered.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>Trace output written during execution of method.</returns>
        public ExecuteStepResult ExecuteStep(string modelName, string name, IDictionary<string, dynamic> data = null)
        {
            data = data ?? new Dictionary<String, dynamic>();
            Type modelType = GetModelType(modelName);

            if (modelType == null)
            {
                throw new ModelNotFoundException(modelName);
            }

            var handlerType = TryGetStepHandler(modelType, name, out MethodInfo stepMethod);
            if (handlerType == StepHandlerType.NoHandler)
            {
                throw new StepNotFoundException(modelName, name);
            }
            if (handlerType == StepHandlerType.MultipleHandlers || handlerType == StepHandlerType.InvalidHandler)
            {
                throw new InvalidStepHandlerException(modelName, name, handlerType.ToString());
            }

            return ExecuteStep(modelType, stepMethod, handlerType, data);
        }

        /// <summary>
        /// Resets the model instances
        /// </summary>
        public void Reset()
        {
            modelInstances = new Dictionary<Type, object>();
        }

        private ExecuteStepResult ExecuteStep(Type model, MethodInfo stepMethod, StepHandlerType stepType, IDictionary<String, dynamic> data)
        {
            object instance = GetInstance(model);
            using (StepTraceListener stepTrace = new StepTraceListener())
            {
                var stepResult = new ExecuteStepResult();

                try
                {
                    if (stepType == StepHandlerType.Data)
                    {
                        var ret = stepMethod.Invoke(instance, new[] { data });
                        stepResult.data = data;
                        stepResult.result = ret;
                    } else if (stepType == StepHandlerType.NoData)
                    {
                        var ret = stepMethod.Invoke(instance, null);
                        stepResult.result = ret;
                    } else
                    {
                        throw new ArgumentException("Invalid StepHandlerType type.", nameof(stepType));
                    }
                } catch (TargetInvocationException tie)
                {
                    stepResult.error = new AltwalkerError
                    {
                        message = tie.InnerException.Message,
                        trace = tie.InnerException.StackTrace
                    };
                }
                stepResult.output = stepTrace.GetOutput();

                return stepResult;
            }
        }

        private Type GetModelType(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) {
                modelName = "Setup";
            }

            models.TryGetValue(modelName, out Type model);
            return model;
        }

        private List<MethodInfo> GetStepMethods(Type modelType, string stepName)
        {
            return modelType.GetMethods().Where(m => m.Name == stepName).ToList();
        }

        private object GetInstance(Type type)
        {
            if (!modelInstances.TryGetValue(type, out object instance))
            {
                instance = Activator.CreateInstance(type);
                modelInstances.Add(type, instance);
            }

            return instance;
        }

        enum StepHandlerType {
            NoData,
            Data,
            InvalidHandler,
            MultipleHandlers,
            NoHandler,
        }

        private StepHandlerType TryGetStepHandler(Type type, string stepName, out MethodInfo stepMethod) {
            stepMethod = null;
            var methods = GetStepMethods(type, stepName);

            if (methods.Count > 1) {
                return StepHandlerType.MultipleHandlers;
            }

            if (methods.Count == 0) {
                return StepHandlerType.NoHandler;
            }

            stepMethod = methods.Single();
            var parameters = stepMethod.GetParameters();

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IDictionary<string, dynamic>))
            {
                return StepHandlerType.Data;
            } else if (parameters.Length == 0)
            {
                return StepHandlerType.NoData;
            } else
            {
                stepMethod = null;
                return StepHandlerType.InvalidHandler;
            }
        }
    }

    public class ExecuteStepResult
    {
        public ExecuteStepResult()
        {
        }

        public ExecuteStepResult(string message, string trace)
        {
            error = new AltwalkerError { message = message, trace = trace };
        }

        public ExecuteStepResult(string output, IDictionary<string, dynamic> data)
        {
            this.output = output;
            this.data = data;
        }

        public string output { get; set; }
        public IDictionary<string, dynamic> data { get; set; }
        public object result { get; set; }
        public AltwalkerError error { get; set; }
    }

    public class AltwalkerError
    {
        public string message { get; set; }
        public string trace { get; set; }
    }

    public class StepNotFoundException : Exception
    {
        public StepNotFoundException(string modelName, string stepName) : base($"Method named `{stepName}` not found in class `{modelName}`.")
        {
        }
    }

    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(string modelName) : base($"No model named `{modelName}` was registered")
        {
        }
    }

    public class InvalidStepHandlerException : Exception
    {
        public InvalidStepHandlerException(string modelName, string stepName, string reason) : base($"{reason} for `{stepName}` in type `{modelName}`.")
        {
        }
    }

    // /// <summary>
    // /// Exception occurred during step execution
    // /// </summary>
    // public class StepExecutionException : Exception {
    //     public StepExecutionException (Exception innerException) : base (string.Empty, innerException) { }
    // }
    internal class StepTraceListener : IDisposable
    {
        MemoryStream memoryStream;
        TraceListener traceListener;

        public StepTraceListener()
        {
            memoryStream = new MemoryStream();
            traceListener = new TextWriterTraceListener(memoryStream);
            Trace.Listeners.Add(traceListener);
        }

        public string GetOutput()
        {
            traceListener.Flush();
            memoryStream.Position = 0;
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                return reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(traceListener);
            traceListener.Dispose();
            memoryStream.Dispose();
        }
    }
}
