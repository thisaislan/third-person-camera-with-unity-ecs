using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// Base class for build steps that are executed througout a <see cref="BuildPipeline"/>.
    /// </summary>
    public abstract class BuildStep : IBuildStep
    {
        /// <summary>
        /// The name of this <see cref="BuildStep"/>.
        /// </summary>
        public string Name => GetName(GetType());

        /// <summary>
        /// Description of this <see cref="BuildStep"/> displayed in build progress reporting.
        /// </summary>
        public string Description => GetDescription(GetType());

        /// <summary>
        /// Category name of this <see cref="BuildStep"/> displayed in the searcher menu.
        /// </summary>
        public string Category => GetCategory(GetType());

        /// <summary>
        /// Determine if this <see cref="BuildStep"/> should be displayed in the inspector and searcher menu.
        /// </summary>
        public bool IsShown => GetIsShown(GetType());

        /// <summary>
        /// List of <see cref="IBuildComponent"/> derived types that are required for this <see cref="BuildStep"/>.
        /// </summary>
        public virtual Type[] RequiredComponents { get; }

        /// <summary>
        /// List of <see cref="IBuildComponent"/> derived types that are optional for this <see cref="BuildStep"/>.
        /// </summary>
        public virtual Type[] OptionalComponents { get; }

        /// <summary>
        /// Determine if this <see cref="BuildStep"/> will be executed or not.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns><see langword="true"/> if enabled, <see langword="false"/> otherwise.</returns>
        public virtual bool IsEnabled(BuildContext context) => true;

        /// <summary>
        /// Run this <see cref="BuildStep"/>.
        /// If a previous <see cref="BuildStep"/> fails, this <see cref="BuildStep"/> will not run.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>The result of running this <see cref="BuildStep"/>.</returns>
        public abstract BuildStepResult RunBuildStep(BuildContext context);

        /// <summary>
        /// Cleanup this <see cref="BuildStep"/>.
        /// Cleanup will only be called if this <see cref="BuildStep"/> ran.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        public virtual BuildStepResult CleanupBuildStep(BuildContext context) => throw new InvalidOperationException(nameof(CleanupBuildStep));

        /// <summary>
        /// Determine if a required <see cref="Type"/> component is stored in <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="RequiredComponents"/> list.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the required component.</param>
        /// <returns><see langword="true"/> if the required component type is found, <see langword="false"/> otherwise.</returns>
        public bool HasRequiredComponent(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (RequiredComponents == null || !RequiredComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(RequiredComponents)} list.");
            }
            return context.BuildConfiguration.HasComponent(type);
        }

        /// <summary>
        /// Determine if a required <typeparamref name="T"/> component is stored in <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="RequiredComponents"/> list.
        /// </summary>
        /// <typeparam name="T">Type of the required component.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns><see langword="true"/> if the required component type is found, <see langword="false"/> otherwise.</returns>
        public bool HasRequiredComponent<T>(BuildContext context) where T : IBuildComponent => HasRequiredComponent(context, typeof(T));

        /// <summary>
        /// Get the value of a required <see cref="Type"/> component from <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="RequiredComponents"/> list.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the required component.</param>
        /// <returns>The value of the required component.</returns>
        public IBuildComponent GetRequiredComponent(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (RequiredComponents == null || !RequiredComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(RequiredComponents)} list.");
            }
            return context.BuildConfiguration.GetComponent(type);
        }

        /// <summary>
        /// Get the value of a required <typeparamref name="T"/> component from <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="RequiredComponents"/> list.
        /// </summary>
        /// <typeparam name="T">Type of the required component.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>The value of the required component.</returns>
        public T GetRequiredComponent<T>(BuildContext context) where T : IBuildComponent => (T)GetRequiredComponent(context, typeof(T));

        /// <summary>
        /// Get all required components from <see cref="BuildConfiguration"/>.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>List of required components.</returns>
        public IEnumerable<IBuildComponent> GetRequiredComponents(BuildContext context)
        {
            if (RequiredComponents == null)
            {
                return Enumerable.Empty<IBuildComponent>();
            }

            var lookup = new Dictionary<Type, IBuildComponent>();
            foreach (var requiredComponent in RequiredComponents)
            {
                lookup[requiredComponent] = context.BuildConfiguration.GetComponent(requiredComponent);
            }
            return lookup.Values;
        }

        /// <summary>
        /// Get all required components from <see cref="BuildConfiguration"/>, that matches <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the components.</param>
        /// <returns>List of required components.</returns>
        public IEnumerable<IBuildComponent> GetRequiredComponents(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (RequiredComponents == null || !RequiredComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(RequiredComponents)} list.");
            }

            var lookup = new Dictionary<Type, IBuildComponent>();
            foreach (var requiredComponent in RequiredComponents)
            {
                if (!type.IsAssignableFrom(requiredComponent))
                {
                    continue;
                }
                lookup[requiredComponent] = context.BuildConfiguration.GetComponent(requiredComponent);
            }
            return lookup.Values;
        }

        /// <summary>
        /// Get all required components from <see cref="BuildConfiguration"/>, that matches <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>List of required components.</returns>
        public IEnumerable<T> GetRequiredComponents<T>(BuildContext context) where T : IBuildComponent => GetRequiredComponents(context, typeof(T)).Cast<T>();

        /// <summary>
        /// Determine if an optional <see cref="Type"/> component is stored in <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="OptionalComponents"/> list.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the optional component.</param>
        /// <returns><see langword="true"/> if the optional component type is found, <see langword="false"/> otherwise.</returns>
        public bool HasOptionalComponent(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (OptionalComponents == null || !OptionalComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(OptionalComponents)} list.");
            }
            return context.BuildConfiguration.HasComponent(type);
        }

        /// <summary>
        /// Determine if an optional <typeparamref name="T"/> component is stored in <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="OptionalComponents"/> list.
        /// </summary>
        /// <typeparam name="T">Type of the optional component.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns><see langword="true"/> if the optional component type is found, <see langword="false"/> otherwise.</returns>
        public bool HasOptionalComponent<T>(BuildContext context) where T : IBuildComponent => HasOptionalComponent(context, typeof(T));

        /// <summary>
        /// Get the value of an optional <see cref="Type"/> component from <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="OptionalComponents"/> list.
        /// If the component is not found in <see cref="BuildConfiguration"/>, a new instance of type <see cref="Type"/> is returned.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the optional component.</param>
        /// <returns>The value of the optional component.</returns>
        public IBuildComponent GetOptionalComponent(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (OptionalComponents == null || !OptionalComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(OptionalComponents)} list.");
            }

            if (context.BuildConfiguration.HasComponent(type))
            {
                return context.BuildConfiguration.GetComponent(type);
            }

            return TypeConstruction.Construct<IBuildComponent>(type);
        }

        /// <summary>
        /// Get the value of an optional <typeparamref name="T"/> component from <see cref="BuildConfiguration"/>.
        /// The component <see cref="Type"/> must exist in the <see cref="OptionalComponents"/> list.
        /// If the component is not found in <see cref="BuildConfiguration"/>, a new instance of type <typeparamref name="T"/> is returned.
        /// </summary>
        /// <typeparam name="T">Type of the optional component.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>The value of the optional component.</returns>
        public T GetOptionalComponent<T>(BuildContext context) where T : IBuildComponent => (T)GetOptionalComponent(context, typeof(T));

        /// <summary>
        /// Get all optional components from <see cref="BuildConfiguration"/>.
        /// Optional component types not found in <see cref="BuildConfiguration"/> will be set to a new instance of that type.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>List of optional components.</returns>
        public IEnumerable<IBuildComponent> GetOptionalComponents(BuildContext context)
        {
            if (OptionalComponents == null)
            {
                return Enumerable.Empty<IBuildComponent>();
            }

            var lookup = new Dictionary<Type, IBuildComponent>();
            foreach (var type in OptionalComponents)
            {
                if (!context.BuildConfiguration.TryGetComponent(type, out var component))
                {
                    component = TypeConstruction.Construct<IBuildComponent>(type);
                }
                lookup[type] = component;
            }
            return lookup.Values;
        }

        /// <summary>
        /// Get all optional components from <see cref="BuildConfiguration"/>, that matches <see cref="Type"/>.
        /// Optional component types not found in <see cref="BuildConfiguration"/> will be set to a new instance of that type.
        /// </summary>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <param name="type">Type of the components.</param>
        /// <returns>List of optional components.</returns>
        public IEnumerable<IBuildComponent> GetOptionalComponents(BuildContext context, Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildComponent>(type);
            if (OptionalComponents == null || !OptionalComponents.Contains(type))
            {
                throw new InvalidOperationException($"Component type '{type.FullName}' is not in the {nameof(OptionalComponents)} list.");
            }

            var lookup = new Dictionary<Type, IBuildComponent>();
            foreach (var optionalComponentType in OptionalComponents)
            {
                if (!type.IsAssignableFrom(optionalComponentType))
                {
                    continue;
                }

                if (!context.BuildConfiguration.TryGetComponent(optionalComponentType, out var component))
                {
                    component = TypeConstruction.Construct<IBuildComponent>(optionalComponentType);
                }
                lookup[optionalComponentType] = component;
            }
            return lookup.Values;
        }

        /// <summary>
        /// Get all optional components from <see cref="BuildConfiguration"/>, that matches <typeparamref name="T"/>.
        /// Optional component types not found in <see cref="BuildConfiguration"/> will be set to a new instance of that type.
        /// </summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <param name="context">The <see cref="BuildContext"/> used by the execution of this <see cref="BuildStep"/>.</param>
        /// <returns>List of optional components.</returns>
        public IEnumerable<T> GetOptionalComponents<T>(BuildContext context) => GetOptionalComponents(context, typeof(T)).Cast<T>();

        /// <summary>
        /// Construct <see cref="BuildStepResult"/> from this <see cref="BuildStep"/> that represent a successful execution.
        /// </summary>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public BuildStepResult Success() => BuildStepResult.Success(this);

        /// <summary>
        /// Construct <see cref="BuildStepResult"/> from this <see cref="BuildStep"/> that represent a failed execution.
        /// </summary>
        /// <param name="message">Message that explain why the <see cref="BuildStep"/> execution failed.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public BuildStepResult Failure(string message) => BuildStepResult.Failure(this, message);

        /// <summary>
        /// Get the name of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <param name="type">The <see cref="BuildStep"/> type.</param>
        /// <returns>The <see cref="BuildStep"/>'s name.</returns>
        public static string GetName(Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildStep>(type);
            return type.GetCustomAttribute<BuildStepAttribute>()?.Name ?? type.Name;
        }

        /// <summary>
        /// Get the name of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="BuildStep"/> type.</typeparam>
        /// <returns>The <see cref="BuildStep"/>'s name.</returns>
        public static string GetName<T>() where T : IBuildStep => GetName(typeof(T));

        /// <summary>
        /// Get the description displayed in build progress reporting of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <param name="type">The <see cref="BuildStep"/> type.</param>
        /// <returns>The <see cref="BuildStep"/>'s description.</returns>
        public static string GetDescription(Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildStep>(type);
            return type.GetCustomAttribute<BuildStepAttribute>()?.Description ?? $"Running {GetName(type)}";
        }

        /// <summary>
        /// Get the description displayed in build progress reporting of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="BuildStep"/> type.</typeparam>
        /// <returns>The <see cref="BuildStep"/>'s description.</returns>
        public static string GetDescription<T>() where T : IBuildStep => GetDescription(typeof(T));

        /// <summary>
        /// Get the category name displayed in the searcher menu of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <param name="type">The <see cref="BuildStep"/> type.</param>
        /// <returns>The <see cref="BuildStep"/>'s category name.</returns>
        public static string GetCategory(Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildStep>(type);
            return type.GetCustomAttribute<BuildStepAttribute>()?.Category ?? string.Empty;
        }

        /// <summary>
        /// Get the category name displayed in the searcher menu of a <see cref="BuildStep"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="BuildStep"/> type.</typeparam>
        /// <returns>The <see cref="BuildStep"/>'s category name.</returns>
        public static string GetCategory<T>() where T : IBuildStep => GetCategory(typeof(T));

        /// <summary>
        /// Determine if a <see cref="BuildStep"/> type should be displayed in the inspector and searcher menu.
        /// </summary>
        /// <param name="type">The <see cref="BuildStep"/> type.</param>
        /// <returns><see langword="true"/> if the <see cref="BuildStep"/> is shown, <see langword="false"/> otherwise.</returns>
        public static bool GetIsShown(Type type)
        {
            CheckTypeAndThrowIfInvalid<IBuildStep>(type);
            return type.GetCustomAttribute<HideInInspector>() == null ||
#pragma warning disable 618
                (type.GetCustomAttribute<BuildStepAttribute>()?.flags.HasFlag(BuildStepAttribute.Flags.Hidden) ?? false);
#pragma warning restore 618
        }

        /// <summary>
        /// Determine if a <see cref="BuildStep"/> type should be displayed in the inspector and searcher menu.
        /// </summary>
        /// <typeparam name="T">The <see cref="BuildStep"/> type.</typeparam>
        /// <returns><see langword="true"/> if the <see cref="BuildStep"/> is shown, <see langword="false"/> otherwise.</returns>
        public static bool GetIsShown<T>() where T : IBuildStep => GetIsShown(typeof(T));

        internal static string Serialize(IBuildStep step)
        {
            if (step == null)
            {
                return null;
            }

            if (step is BuildPipeline pipeline)
            {
                return GlobalObjectId.GetGlobalObjectIdSlow(pipeline).ToString();
            }
            else
            {
                return step.GetType().GetFullyQualifedAssemblyTypeName();
            }
        }

        internal static IBuildStep Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            if (GlobalObjectId.TryParse(json, out var id))
            {
                if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) is BuildPipeline pipeline)
                {
                    return pipeline;
                }
            }
            else
            {
                if (TypeConstruction.TryConstructFromAssemblyQualifiedTypeName<IBuildStep>(json, out var step))
                {
                    return step;
                }
            }

            return null;
        }

        internal static IEnumerable<Type> GetAvailableTypes(Func<Type, bool> filter = null)
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<IBuildStep>())
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
