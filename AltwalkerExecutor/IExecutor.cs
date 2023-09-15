//    Copyright(C) 2023 Altom Consulting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Altom.AltWalker
{
    public interface IExecutor
    {
        /// <summary>
        /// Checks if a type with the given `modelName` is registered.
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns>True if the type exists</returns>
        bool HasModel(string modelName);

        /// <summary>
        /// Checks if the type registered for `modelName` has a public method named `stepName`.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>True if the method exists</returns>
        bool HasStep(string modelName, string name);

        /// <summary>
        /// Executes the public method `name` from the type `modelName` registered.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="name"></param>
        /// <returns>Trace output written during execution of method.</returns>
        ExecuteStepResult ExecuteStep(string modelName, string name, IDictionary<string, dynamic> data = null);

        /// <summary>
        /// Resets the model instances.
        /// </summary>
        void Reset();
    }
}
