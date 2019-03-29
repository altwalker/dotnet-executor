using System.Collections.Generic;

namespace Altom.Altwalker
{
    public interface IExecutor {
        /// <summary>
        /// Checks if a type with the given `modelName` is registered
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns>True if the type exists</returns>
        bool HasModel (string modelName);

        /// <summary>
        /// Checks if the type registered for `modelName` has a public method named `stepName`
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>True if the method exists</returns>
        bool HasStep (string modelName, string name);

        /// <summary>
        /// Executes the public method `name` from the type `modelName` registered.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>Trace output written during execution of method.</returns>
        ExecuteStepResult ExecuteStep (string modelName, string name, IDictionary<string, dynamic> data = null);

        /// <summary>
        /// Resets the model instances
        /// </summary>
        void Reset ();
    }
}