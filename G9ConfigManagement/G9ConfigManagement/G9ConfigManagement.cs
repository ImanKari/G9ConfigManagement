using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using G9AssemblyManagement;
using G9AssemblyManagement.Enums;
using G9AssemblyManagement.Interfaces;
using G9ConfigManagement.Abstract;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;
using G9ConfigManagement.Helper;

namespace G9ConfigManagement
{
    /// <summary>
    ///     Static config management helper
    /// </summary>
    /// <typeparam name="TConfigDataType">
    ///     Specifies a custom config structure type that must be inherited from '
    ///     <see cref="G9AConfigStructure{TConfigType}" />'
    /// </typeparam>
    internal static class G9ConfigManagement<TConfigDataType>
        where TConfigDataType : G9AConfigStructure<TConfigDataType>, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     A category for storing the latest instance of the config.
        /// </summary>
        private static readonly Dictionary<int, InitializeConfigFile<TConfigDataType>> ConfigsInformation
            = new Dictionary<int, InitializeConfigFile<TConfigDataType>>();

        /// <summary>
        ///     Object for managing locks
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object ObjectLock = new object();

        #endregion

        #region Methods

        /// <summary>
        ///     Method to create or get a config structure.
        /// </summary>
        public static TConfigDataType GetOrCreate()
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                // If the instance of the config exists, the core returns it.
                if (ConfigsInformation.ContainsKey(configCode))
                    return ConfigsInformation[configCode].ConfigObject;
            }

            // Prepare default data
            PrepareRequiredData(out var instance, out var initializationData);

            try
            {
                lock (ObjectLock)
                {
                    // Initialize or restore config file
                    var newInitialize =
                        new InitializeConfigFile<TConfigDataType>(initializationData, instance.ConfigVersion,
                            false);
                    instance = newInitialize.ConfigObject;
                    ConfigsInformation.Add(typeof(TConfigDataType).GetHashCode(), newInitialize);

                    // Add basis data
                    AddConfigBasisData(instance, initializationData.ConfigOptions.ConfigFileName,
                        initializationData.ConfigOptions.ConfigFileExtension,
                        initializationData.ConfigOptions.ConfigFileLocation);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An exception occurred when the core tried to work with config structure '{instance.GetType().FullName}'.",
                    ex);
            }

            // return config
            return instance;
        }

        /// <summary>
        /// Method to update a config structure.
        /// </summary>
        /// <param name="newConfigValue">Specifies a new structure for config.</param>
        public static void Update(TConfigDataType newConfigValue)
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                if (!ConfigsInformation.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                ConfigsInformation[configCode].UpdateConfig(newConfigValue);
            }
        }

        /// <summary>
        /// Method to restore a config structure to the default value.
        /// </summary>
        public static void RestoreToDefault()
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            // Prepare default data
            PrepareRequiredData(out var _, out var initializationData);

            lock (ObjectLock)
            {
                if (!ConfigsInformation.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                ConfigsInformation[configCode].UpdateConfig(initializationData.ConfigInitialization);
            }
        }

        /// <summary>
        /// Method to restore a config structure to the default value that would be gotten from the config file.
        /// </summary>
        public static void RestoreToDefaultByConfigFile()
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();
            
            lock (ObjectLock)
            {
                if (!ConfigsInformation.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                ConfigsInformation[configCode].RestoreConfigByConfigFile();
            }
        }

        /// <summary>
        ///     Helper method to check the necessary validations
        /// </summary>
        private static void CheckValidations(TConfigDataType instance)
        {
            // Validation for version
            if (instance.ConfigVersion == null)
                throw new NullReferenceException(
                    $"The specified property '{nameof(instance.ConfigVersion)}' in class '{typeof(TConfigDataType).FullName}' for getting the version of config can't be uninitialized or null.");

            var members = G9Assembly.ObjectAndReflectionTools.GetFieldsOfObject(instance, G9EAccessModifier.Public,
                    s => s.GetCustomAttributes(typeof(G9AttrRequiredAttribute), true).Any())
                .Select(s => (G9IMember)s)
                .Concat(G9Assembly.ObjectAndReflectionTools.GetPropertiesOfObject(instance, G9EAccessModifier.Public,
                        s => s.GetCustomAttributes(typeof(G9AttrRequiredAttribute), true).Any())
                    .Select(s => (G9IMember)s)).ToArray();

            if (!members.Any()) return;
            NoNullAllowedException ex = null;
            foreach (var member in members)
                if (IsDefault(member.GetValue(), member.MemberType))
                    ex = ex == null
                        ? new NoNullAllowedException(
                            $"The member '{member.Name}' in the structure '{typeof(TConfigDataType).FullName}' has the attribute '{nameof(G9AttrRequiredAttribute)}.' So, it can't be null or default.")
                        : new NoNullAllowedException(
                            $"The member '{member.Name}' in the structure '{typeof(TConfigDataType).FullName}' has the attribute '{nameof(G9AttrRequiredAttribute)}.' So, it can't be null or default.",
                            ex);
            if (ex != null) throw ex;
        }

        /// <summary>
        ///     Helper method to fix the necessary requirements in option object
        /// </summary>
        private static G9DtConfigInitialize<TConfigDataType> FixConfigInitialize(TConfigDataType instance)
        {
            var initializedRequirement = instance.Initialize();

            if (Equals(initializedRequirement, default(G9DtConfigInitialize<TConfigDataType>)))
                initializedRequirement = new G9DtConfigInitialize<TConfigDataType>();

            if (initializedRequirement.ConfigInitialization == null)
                initializedRequirement.ConfigInitialization = new TConfigDataType();

            if (initializedRequirement.ConfigOptions == null)
                initializedRequirement.ConfigOptions = new G9DtConfigSettings();

            // Fix options
            initializedRequirement.ConfigOptions = FixConfigOption(initializedRequirement.ConfigOptions);

            return initializedRequirement;
        }

        /// <summary>
        ///     Helper method to fix the necessary requirements in option object
        /// </summary>
        private static G9DtConfigSettings FixConfigOption(G9DtConfigSettings options)
        {
            if (string.IsNullOrEmpty(options.ConfigFileName) || string.IsNullOrEmpty(options.ConfigFileExtension) ||
                string.IsNullOrEmpty(options.ConfigFileLocation))
            {
                var configFileName = string.IsNullOrEmpty(options.ConfigFileName)
                    ? typeof(TConfigDataType).Name
                    : options.ConfigFileName;

                var configFileExtension =
                    string.IsNullOrEmpty(options.ConfigFileExtension)
                        ? "json"
                        : options.ConfigFileExtension;

                var fileLocation = string.IsNullOrEmpty(options.ConfigFileLocation)
                    ?
#if (NET35 || NET40 || NET45)
                    AppDomain.CurrentDomain.BaseDirectory
#else
                    AppContext.BaseDirectory
#endif
                    : options.ConfigFileLocation;

                return new G9DtConfigSettings(options.ChangeVersionReaction, configFileName, configFileExtension,
                    fileLocation, options.EnableAutomatedCreatingPath);
            }

            return options;
        }

        /// <summary>
        ///     Helper method to add the necessary config data to the config object
        /// </summary>
        private static void AddConfigBasisData(TConfigDataType config, string configFileName,
            string configFileExtension, string configPath)
        {
            var targetType = config.GetType();
            while (true)
            {
                targetType = targetType.BaseType;
                if (targetType == null || (targetType.IsGenericType &&
                                           targetType.GetGenericTypeDefinition() == typeof(G9AConfigStructure<>)))
                    break;
            }

            // Find and call
            // ReSharper disable once PossibleNullReferenceException
            var method = targetType.GetMethod("G9SetConfigBasisData", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable once PossibleNullReferenceException
            method.Invoke(config, new object[] { configFileName, configFileExtension, configPath });
        }

        /// <summary>
        ///     Helper method for check is a object on its default value or not.
        /// </summary>
        public static bool IsDefault<T>(T targetObject, Type objType)
        {
            if (targetObject == null)
                return true;

            return objType.IsValueType && !objType.IsEnum
                ? Equals(targetObject, G9Assembly.InstanceTools.CreateUninitializedInstanceFromType(objType))
                : EqualityComparer<T>.Default.Equals(targetObject, default);
        }

        /// <summary>
        ///     Helper method to prepare an instance and initialization data of config
        /// </summary>
        private static void PrepareRequiredData(out TConfigDataType instance,
            out G9DtConfigInitialize<TConfigDataType> initializationData)
        {
            // Specifying instance
            instance = new TConfigDataType();

            // Check the necessary validations
            CheckValidations(instance);

            // Access to initialization data
            initializationData = FixConfigInitialize(instance);
        }

        #endregion
    }
}