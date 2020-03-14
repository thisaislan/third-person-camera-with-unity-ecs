using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    public abstract class RunStep
    {
        /// <summary>
        /// The name of this <see cref="RunStep"/>.
        /// </summary>
        public string Name => GetName(GetType());

        /// <summary>
        /// Category name of this <see cref="RunStep"/> displayed in the searcher menu.
        /// </summary>
        public string Category => GetCategory(GetType());

        /// <summary>
        /// Determine if this <see cref="RunStep"/> should be displayed in the inspector and searcher menu.
        /// </summary>
        public bool IsShown => GetIsShown(GetType());

        public virtual bool CanRun(BuildConfiguration config, out string reason)
        {
            reason = null;
            return true;
        }

        public abstract RunStepResult Start(BuildConfiguration config);

        public RunStepResult Success(BuildConfiguration config, IRunInstance instance) => RunStepResult.Success(config, this, instance);

        public RunStepResult Failure(BuildConfiguration config, string message) => RunStepResult.Failure(config, this, message);

        /// <summary>
        /// Get the name of a <see cref="RunStep"/> type.
        /// </summary>
        /// <param name="type">The <see cref="RunStep"/> type.</param>
        /// <returns>The <see cref="RunStep"/>'s name.</returns>
        public static string GetName(Type type)
        {
            CheckTypeAndThrowIfInvalid<RunStep>(type);
            return type.GetCustomAttribute<RunStepAttribute>()?.Name ?? type.Name;
        }

        /// <summary>
        /// Get the name of a <see cref="RunStep"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="RunStep"/> type.</typeparam>
        /// <returns>The <see cref="RunStep"/>'s name.</returns>
        public static string GetName<T>() where T : RunStep => GetName(typeof(T));

        /// <summary>
        /// Get the category name displayed in the searcher menu of a <see cref="RunStep"/> type.
        /// </summary>
        /// <param name="type">The <see cref="RunStep"/> type.</param>
        /// <returns>The <see cref="RunStep"/>'s category name.</returns>
        public static string GetCategory(Type type)
        {
            CheckTypeAndThrowIfInvalid<RunStep>(type);
            return type.GetCustomAttribute<RunStepAttribute>()?.Category ?? string.Empty;
        }

        /// <summary>
        /// Get the category name displayed in the searcher menu of a <see cref="RunStep"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="RunStep"/> type.</typeparam>
        /// <returns>The <see cref="RunStep"/>'s category name.</returns>
        public static string GetCategory<T>() where T : RunStep => GetCategory(typeof(T));

        /// <summary>
        /// Determine if a <see cref="RunStep"/> type should be displayed in the inspector and searcher menu.
        /// </summary>
        /// <param name="type">The <see cref="RunStep"/> type.</param>
        /// <returns><see langword="true"/> if the <see cref="RunStep"/> is shown, <see langword="false"/> otherwise.</returns>
        public static bool GetIsShown(Type type)
        {
            CheckTypeAndThrowIfInvalid<RunStep>(type);
            return type.GetCustomAttribute<HideInInspector>() == null;
        }

        /// <summary>
        /// Determine if a <see cref="RunStep"/> type should be displayed in the inspector and searcher menu.
        /// </summary>
        /// <typeparam name="T">The <see cref="RunStep"/> type.</typeparam>
        /// <returns><see langword="true"/> if the <see cref="RunStep"/> is shown, <see langword="false"/> otherwise.</returns>
        public static bool GetIsShown<T>() where T : RunStep => GetIsShown(typeof(T));

        internal static string Serialize(RunStep step)
        {
            return step?.GetType().GetFullyQualifedAssemblyTypeName();
        }

        internal static RunStep Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            if (TypeConstruction.TryConstructFromAssemblyQualifiedTypeName<RunStep>(json, out var step))
            {
                return step;
            }

            return null;
        }

        internal static IEnumerable<Type> GetAvailableTypes(Func<Type, bool> filter = null)
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<RunStep>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (filter != null && !filter(type))
                {
                    continue;
                }

                yield return type;
            }
        }

        static void CheckTypeAndThrowIfInvalid<T>(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type == typeof(object))
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be 'object'.");
            }

            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{nameof(type)} must derive from '{typeof(T).FullName}'.");
            }
        }
    }
}
