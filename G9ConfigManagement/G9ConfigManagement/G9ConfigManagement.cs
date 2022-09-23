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
        ///     A collection for storing the latest instance of the config.
        /// </summary>
        private static readonly Dictionary<int, InitializeConfigFile<TConfigDataType>> LatestConfigCollection
            = new Dictionary<int, InitializeConfigFile<TConfigDataType>>();

        /// <summary>
        ///     A collection for storing the latest instance of the config.
        /// </summary>
        private static readonly Dictionary<int, TConfigDataType> InstanceInitializationCollection
            = new Dictionary<int, TConfigDataType>();

        /// <summary>
        ///     A collection for storing the latest instance of the config.
        /// </summary>
        private static readonly Dictionary<int, G9DtConfigSettings> ConfigSettingsCollection
            = new Dictionary<int, G9DtConfigSettings>();

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
                if (LatestConfigCollection.ContainsKey(configCode))
                    return LatestConfigCollection[configCode].ConfigObject;
            }

            // Prepare default data
            PrepareRequiredData(configCode, out var instance, out var settings);

            try
            {
                lock (ObjectLock)
                {
                    // Initialize or restore config file
                    var newInitialize =
                        new InitializeConfigFile<TConfigDataType>(instance, settings, instance.ConfigVersion,
                            false);
                    instance = newInitialize.ConfigObject;
                    LatestConfigCollection.Add(typeof(TConfigDataType).GetHashCode(), newInitialize);

                    // Add basis data
                    AddConfigBasisData(instance, settings);
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
        ///     Method to update a config structure.
        /// </summary>
        /// <param name="newConfigValue">Specifies a new structure for config.</param>
        public static void Update(TConfigDataType newConfigValue)
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                if (!LatestConfigCollection.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                LatestConfigCollection[configCode].UpdateConfig(newConfigValue);
            }
        }

        /// <summary>
        ///     Method to assign a custom settings for a config structure.
        /// </summary>
        /// <param name="settings">Specifies the desired setting.</param>
        public static void SetConfigSettings(G9DtConfigSettings settings)
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                if (ConfigSettingsCollection.ContainsKey(configCode))
                {
                    ConfigSettingsCollection[configCode] = settings;
                    if (LatestConfigCollection.ContainsKey(configCode))
                        LatestConfigCollection[configCode].RefreshByNewConfigSettings(settings);
                }
                else
                {
                    ConfigSettingsCollection.Add(configCode, settings);
                }
            }
        }

        /// <summary>
        ///     Method to set an instance for first initialization.
        ///     <para />
        ///     By default, the core uses an instance of config created by its default values.
        ///     <para />
        ///     The specified instance is just used for the first initialization, which means if a config file doesn't exist, the
        ///     config core uses it as an instance for creating the config file. So, it must be used before using the method "
        ///     <see cref="G9AConfigStructure{TConfigType}.GetConfig" />".
        /// </summary>
        /// <param name="instance">Specifies an instance for first initialization.</param>
        public static void SetInstanceForInitialization(TConfigDataType instance)
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                if (InstanceInitializationCollection.ContainsKey(configCode))
                    InstanceInitializationCollection[configCode] = instance;
                else
                    InstanceInitializationCollection.Add(configCode, instance);
            }
        }

        /// <summary>
        ///     Method to restore a config structure to the default value.
        /// </summary>
        public static void RestoreToDefault()
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            // Prepare default data
            PrepareRequiredData(configCode, out var instance, out var _);

            lock (ObjectLock)
            {
                if (!LatestConfigCollection.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                LatestConfigCollection[configCode].UpdateConfig(instance);
            }
        }

        /// <inheritdoc cref="InitializeConfigFile{TConfigDataType}.RestoreToDefaultByConfigFile" />
        public static void RestoreToDefaultByConfigFile()
        {
            // Prepare config identity by its hash code type
            var configCode = typeof(TConfigDataType).GetHashCode();

            lock (ObjectLock)
            {
                if (!LatestConfigCollection.ContainsKey(configCode))
                    throw new Exception(
                        $"The specified config '{typeof(TConfigDataType).FullName}' hasn't been created. Before the updating process, it must have been created.");
                LatestConfigCollection[configCode].RestoreToDefaultByConfigFile();
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
        private static G9DtConfigSettings FixConfigSetting(G9DtConfigSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.ConfigFileName) &&
                !string.IsNullOrEmpty(settings.ConfigFileExtension) &&
                !string.IsNullOrEmpty(settings.ConfigFileLocation)) return settings;

            var configFileName = string.IsNullOrEmpty(settings.ConfigFileName)
                ? typeof(TConfigDataType).Name
                : settings.ConfigFileName;

            var configFileExtension =
                string.IsNullOrEmpty(settings.ConfigFileExtension)
                    ? "json"
                    : settings.ConfigFileExtension;

            var fileLocation = string.IsNullOrEmpty(settings.ConfigFileLocation)
                ?
#if (NET35 || NET40 || NET45)
                AppDomain.CurrentDomain.BaseDirectory
#else
                AppContext.BaseDirectory
#endif
                : settings.ConfigFileLocation;

            return new G9DtConfigSettings(settings.ChangeVersionReaction, configFileName, configFileExtension,
                fileLocation, settings.EnableAutomatedCreatingPath);
        }

        /// <summary>
        ///     Helper method to add the necessary config data to the config object
        /// </summary>
        private static void AddConfigBasisData(TConfigDataType instance, G9DtConfigSettings settings)
        {
            var targetType = instance.GetType();
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
            method.Invoke(instance,
                new object[] { settings.ConfigFileName, settings.ConfigFileExtension, settings.ConfigFileLocation });
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
        private static void PrepareRequiredData(int configCode, out TConfigDataType instance,
            out G9DtConfigSettings settings)
        {
            lock (ObjectLock)
            {
                // Specifying instance
                if (InstanceInitializationCollection.ContainsKey(configCode))
                    instance = InstanceInitializationCollection[configCode];
                else
                    instance = new TConfigDataType();

                // Specifying setting
                if (ConfigSettingsCollection.ContainsKey(configCode))
                    settings = ConfigSettingsCollection[configCode];
                else
                    settings = new G9DtConfigSettings();
            }

            // fixing config
            settings = FixConfigSetting(settings);

            // Check the necessary validations
            CheckValidations(instance);
        }

        #endregion
    }
}