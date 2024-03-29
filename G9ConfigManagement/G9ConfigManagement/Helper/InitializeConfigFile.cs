﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using G9AssemblyManagement;
using G9AssemblyManagement.DataType;
using G9AssemblyManagement.Enums;
using G9AssemblyManagement.Interfaces;
using G9ConfigManagement.Abstract;
using G9ConfigManagement.DataType;
using G9ConfigManagement.Enum;
using G9JSONHandler;
using G9JSONHandler.DataType;
using G9JSONHandler.Enum;

namespace G9ConfigManagement.Helper
{
    /// <summary>
    ///     Class management config file
    /// </summary>
    /// <typeparam name="TConfigDataType">Type of config object</typeparam>
    internal class InitializeConfigFile<TConfigDataType> : IDisposable
        where TConfigDataType : G9AConfigStructure<TConfigDataType>, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Object for managing locks
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object ObjectLockForReadingFile = new object();

        /// <summary>
        ///     Object for managing locks
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object ObjectLockForChangingFile = new object();

        /// <summary>
        ///     A collection for storing the bindable members of a config structure
        /// </summary>
        private G9IMember[] _bindableMembers;

        /// <summary>
        ///     A watcher for considering any changes for bindable members.
        /// </summary>
        private FileSystemWatcher _watcherForConfigFile;

        /// <summary>
        ///     An created instance of the config
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public TConfigDataType ConfigObject { private set; get; }

        /// <summary>
        ///     Specifies the full path of the config file.
        /// </summary>
        public readonly string FullConfigPath;

        /// <summary>
        ///     A property for storing the latest settings of the config.
        /// </summary>
        public G9DtConfigSettings ConfigSettings { private set; get; }

        /// <summary>
        ///     A field for storing the latest version of the config.
        /// </summary>
        public G9DtConfigVersion ConfigVersion;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="instance">Specifies the custom instance for initialization.</param>
        /// <param name="settings">Specifies the settings for config</param>
        /// <param name="configVersion">Specifies the latest version of the config.</param>
        /// <param name="forceUpdateWithObject">
        ///     Specifies whether the config file must be remade (if it exists) or not.
        /// </param>
        public InitializeConfigFile(TConfigDataType instance, G9DtConfigSettings settings,
            G9DtConfigVersion configVersion,
            bool forceUpdateWithObject)
        {
            // Set requirement
            ConfigObject = instance;
            ConfigSettings = settings;
            ConfigVersion = configVersion;

            // Set config path
            FullConfigPath = Path.Combine(ConfigSettings.ConfigFileLocation,
                $"{ConfigSettings.ConfigFileName}.{ConfigSettings.ConfigFileExtension}");

            // Initialize
            ConfigInitializer(forceUpdateWithObject);
        }

        /// <summary>
        ///     Method to update the config
        /// </summary>
        /// <param name="newInstance">Specifies a new instance of the config</param>
        public void UpdateConfig(TConfigDataType newInstance)
        {
            // Merge the old value with new value
            G9Assembly.ReflectionTools.MergeObjectsValues(ConfigObject, newInstance, G9EAccessModifier.Public,
                customProcess: CustomMergeProcessForSomeTypes);

            // Initialize
            ConfigInitializer(true);
        }

        /// <summary>
        ///     Method to restore a config structure to the default value that would be gotten from the config file.
        /// </summary>
        public void RestoreToDefaultByConfigFile()
        {
            // Initialize
            ConfigInitializer(false);
        }

        /// <summary>
        ///     Method to refresh config with a new config settings
        /// </summary>
        /// <param name="newSettings">Specifies a new config settings</param>
        public void RefreshByNewConfigSettings(G9DtConfigSettings newSettings)
        {
            ConfigSettings = newSettings;
            RestoreToDefaultByConfigFile();
        }

        /// <summary>
        ///     Helper method
        /// </summary>
        private void ConfigInitializer(bool forceUpdateWithObject)
        {
            // Create or load config data
            // If file exists, The core must load it
            if (File.Exists(FullConfigPath))
            {
                if (forceUpdateWithObject)
                {
                    CreateJsonConfigByType();
                }
                else
                {
                    string configJsonText = null;
                    G9Assembly.InputOutputTools.WaitForAccessToFile(FullConfigPath, fs =>
                    {
                        var jsonByteArray = new byte[fs.Length];
                        _ = fs.Read(jsonByteArray, 0, (int)fs.Length);
                        configJsonText = Encoding.UTF8.GetString(jsonByteArray);
                    }, FileMode.Open, FileAccess.Read, FileShare.Read);

                    var jsonObject = G9JSON.JsonToObject<TConfigDataType>(configJsonText,
                        new G9DtJsonParserConfig(G9EAccessModifier.Public, true));

                    // If config version change
                    // Remake with change value
                    if (jsonObject.ConfigVersion == null ||
                        jsonObject.ConfigVersion != ConfigObject.ConfigVersion)
                    {
                        if (ConfigSettings.ChangeVersionReaction == G9EChangeVersionReaction.MergeThenOverwrite)
                            // Unifying new version by old version
                            G9Assembly.ReflectionTools.MergeObjectsValues(ConfigObject, jsonObject,
                                G9EAccessModifier.Public,
                                G9EValueMismatchChecking.AllowMismatchValues, true,
                                customProcess: CustomMergeProcessForSomeTypes);
                        CreateJsonConfigByType();
                    }
                    // Else load config from config file
                    else
                    {
                        // Unifying new version by old version
                        G9Assembly.ReflectionTools.MergeObjectsValues(ConfigObject, jsonObject,
                            G9EAccessModifier.Public,
                            G9EValueMismatchChecking.AllowMismatchValues, true,
                            customProcess: CustomMergeProcessForSomeTypes);
                    }
                }
            }
            else
            {
                // Create config file with config object
                CreateJsonConfigByType();
            }

            BindableMemberHandler();
        }

        /// Specifies a custom process for desired members if needed.
        /// <para />
        /// Notice: The function's result specifies whether the custom process handled merging or not.
        /// <para />
        /// If it's returned 'true.' Specifies that the custom process has done the merging process, and the core mustn't do
        /// anything.
        /// <para />
        /// If it's returned 'false.' Specifies that the custom process skipped the merging process, So the core must do it.
        private static bool CustomMergeProcessForSomeTypes(G9IMember m1, G9IMember m2)
        {
            // Skip on config
            if (m2.MemberType == typeof(G9DtConfigVersion))
                return true;

            // The merging process passes to the core if the type isn't a bindable value.
            if (!m2.MemberType.IsGenericParameter ||
                m2.MemberType.GetGenericTypeDefinition() != typeof(G9DtBindableMember<>)) return false;

            SetBindableValue(m1, m2);

            return true;
        }

        /// <summary>
        ///     Helper method for setting the new value for bindable member if needed (if a difference is existed)
        /// </summary>
        private static void SetBindableValue(G9IMember m1, G9IMember m2)
        {
            var firstValue = m1.GetValue();
            var secondValue = m2.GetValue();
            if (Equals(firstValue, null) || Equals(secondValue, null))
                return;

            // Access to the value of the bindable member in the first object.
            var propertyValueOfFirstObject = G9Assembly.ReflectionTools.GetPropertiesOfObject(firstValue,
                    G9EAccessModifier.Public, p => p.Name == nameof(G9DtBindableMember<object>.CurrentValue)).First()
                .GetValue();

            // Access to the value of the bindable member in the second object.
            var propertyValueOfSecondObject = G9Assembly.ReflectionTools.GetPropertiesOfObject(secondValue,
                    G9EAccessModifier.Public, p => p.Name == nameof(G9DtBindableMember<object>.CurrentValue)).First()
                .GetValue();

            // If the values of both members are unequal, the setting process must be done.
            if (!Equals(propertyValueOfFirstObject, propertyValueOfSecondObject))
                // Access the method of setting in the first object and pass the new value for the setting.
                G9Assembly.ReflectionTools.GetMethodsOfObject(firstValue,
                        G9EAccessModifier.Public,
                        s => s.Name == nameof(G9DtBindableMember<object>.SetNewValue)).First()
                    .CallMethod(propertyValueOfSecondObject);
        }

        /// <summary>
        ///     Create xml config file by data type
        /// </summary>
        private void CreateJsonConfigByType()
        {
            // Set Json File
            var jsonString = G9JSON.ObjectToJson(ConfigObject,
                new G9DtJsonWriterConfig(G9EAccessModifier.Public, true, G9ECommentMode.NonstandardMode));

            // Dispose before core change
            _watcherForConfigFile?.Dispose();

            // Creates config directory path according to config
            if (ConfigSettings.EnableAutomatedCreatingPath && !Directory.Exists(ConfigSettings.ConfigFileLocation))
                Directory.CreateDirectory(ConfigSettings.ConfigFileLocation);

            lock (ObjectLockForReadingFile)
            {
                Thread.Sleep(9);
                G9Assembly.InputOutputTools.WaitForAccessToFile(FullConfigPath, fs =>
                {
                    // Save config file
                    var bytesOfConfig = Encoding.UTF8.GetBytes(jsonString);
                    fs.Write(bytesOfConfig, 0, bytesOfConfig.Length);
                }, FileMode.Create, FileAccess.Write, FileShare.Write);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _watcherForConfigFile?.Dispose();
            _bindableMembers = null;
        }

        /// <summary>
        ///     Helper method for managing process of bindable members
        /// </summary>
        private void BindableMemberHandler()
        {
            var bindableMembers = GetBindableMembersFromAnObject(ConfigObject);

            // If any bindable members exist, they must add to the specified collection.
            if (!bindableMembers.Any()) return;

            lock (ObjectLockForChangingFile)
            {
                _bindableMembers = bindableMembers;
            }

            // Initializing a watcher for considering any changes on config file
            _watcherForConfigFile = new FileSystemWatcher(ConfigSettings.ConfigFileLocation,
                $"{ConfigSettings.ConfigFileName}.{ConfigSettings.ConfigFileExtension}")
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcherForConfigFile.Changed += (sender, e) =>
            {
                lock (ObjectLockForChangingFile)
                {
                    string newData = null;
                    Thread.Sleep(9);
                    G9Assembly.InputOutputTools.WaitForAccessToFile(FullConfigPath, fs =>
                    {
                        var jsonByteArray = new byte[fs.Length];
                        _ = fs.Read(jsonByteArray, 0, (int)fs.Length);
                        newData = Encoding.UTF8.GetString(jsonByteArray);
                    }, FileMode.Open, FileAccess.Read, FileShare.Read);

                    var jsonObject =
                        G9JSON.JsonToObject<TConfigDataType>(newData,
                            new G9DtJsonParserConfig(G9EAccessModifier.Public, true));

                    var joinBindableMembers = _bindableMembers.Join(
                        GetBindableMembersFromAnObject(jsonObject), a => a.Name, b => b.Name,
                        (a, b) => new G9DtTuple<G9IMember>(a, b)
                    ).ToArray();

                    foreach (var bindableMember in joinBindableMembers)
                        try
                        {
                            SetBindableValue(bindableMember.Item1, bindableMember.Item2);
                        }
                        catch
                        {
                            // Ignore
                        }
                }
            };
        }

        /// <summary>
        ///     Helper method for getting bindable members from an object
        /// </summary>
        private static G9IMember[] GetBindableMembersFromAnObject(TConfigDataType configObject)
        {
            // Getting the total fields and properties that consist of the bindable member.
            return G9Assembly.ReflectionTools.GetFieldsOfObject(configObject,
                    G9EAccessModifier.Public,
                    s => s.FieldType.IsGenericType &&
                         s.FieldType.GetGenericTypeDefinition() == typeof(G9DtBindableMember<>))
                .Select(s => (G9IMember)s)
                .Concat(
                    G9Assembly.ReflectionTools.GetPropertiesOfObject(configObject,
                            G9EAccessModifier.Public,
                            s => s.PropertyType.IsGenericType &&
                                 s.PropertyType.GetGenericTypeDefinition() == typeof(G9DtBindableMember<>))
                        .Select(s => (G9IMember)s)
                ).ToArray();
        }

        #endregion
    }
}