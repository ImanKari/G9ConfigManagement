using System;
using System.Collections.Generic;
using G9ConfigManagement.Helper;
using G9ConfigManagement.Interface;

namespace G9ConfigManagement
{
    /// <summary>
    ///     Config management, based on singleton pattern design
    /// </summary>
    /// <typeparam name="TConfigDataType">Type of config object</typeparam>
    public class G9ConfigManagementSingleton<TConfigDataType>
        where TConfigDataType : class, IConfigDataType, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Dictionary save initialized object
        ///     use for multi config
        /// </summary>
        private static readonly Dictionary<string, G9ConfigManagementSingleton<TConfigDataType>> ConfigsManagement
            = new Dictionary<string, G9ConfigManagementSingleton<TConfigDataType>>();

        /// <summary>
        ///     Dictionary save initialized config
        ///     use for multi config
        /// </summary>
        private static readonly Dictionary<string, InitializeConfigFile<TConfigDataType>> ConfigsInformation
            = new Dictionary<string, InitializeConfigFile<TConfigDataType>>();

        /// <summary>
        ///     Specify config name
        /// </summary>
        public string ConfigFileName { get; }

        /// <summary>
        ///     Specify config extension
        /// </summary>
        public string ConfigExtension { get; }

        /// <summary>
        ///     Access to config data value
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public TConfigDataType Configuration => ConfigsInformation[ConfigFileName + ConfigExtension].ConfigDataType;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement data
        /// </summary>
        /// <param name="configFileName">Specify config file name</param>
        /// <param name="customConfigObject">
        ///     Optional: Specify custom object for create config xml file.
        ///     Just for create, if exists config file, doesn't use
        /// </param>
        /// <param name="forceRemake">
        ///     remake config data and file with config object.
        ///     Notice: remake xml config even if it exists
        /// </param>
        /// <param name="baseApp">
        ///     <para>Specified base path of application for create config</para>
        ///     <para>Notice: if set null => use 'BaseDirectory' value</para>
        /// </param>
        /// <param name="configExtension">
        ///     <para>Specified config extension</para>
        ///     <para>Notice: if not set argument => use default extension '.config'</para>
        /// </param>

        #region G9ConfigManagement_Singleton

        private G9ConfigManagementSingleton(string configFileName, TConfigDataType customConfigObject = null,
            bool forceRemake = false, string baseApp = null, string configExtension = null)
        {
            try
            {
                // Set config file name and extension
                ConfigFileName = configFileName;
                ConfigExtension = configExtension;

                // Initialize config files
                ConfigsInformation.Add(ConfigFileName + configExtension,
                    new InitializeConfigFile<TConfigDataType>(configFileName, customConfigObject, forceRemake, baseApp,
                        configExtension));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error when read or write config file '{configFileName}' and parse config... | {ex}");
            }
        }

        #endregion

        /// <summary>
        ///     Get instance
        ///     Singleton pattern
        /// </summary>
        /// <param name="configFileName">Specify name of config file, if set null => file name set = config type name</param>
        /// <returns>Instance of class</returns>
        /// <param name="customConfigObject">
        ///     Optional: Specify custom object for create config xml file.
        ///     Just for create, if created don't use
        /// </param>
        /// <param name="forceRemake">
        ///     remake config data and file with config object.
        ///     Notice: remake xml config even if it exists
        /// </param>
        /// <param name="baseApp">
        ///     <para>Specified base path of application for create config</para>
        ///     <para>Notice: if set null => use 'BaseDirectory' value</para>
        /// </param>
        /// <param name="configExtension">
        ///     <para>Specified config extension</para>
        ///     <para>Notice: if not set argument => use default extension '.config'</para>
        /// </param>

        #region G9ConfigManagement_Singleton

        // ReSharper disable once UnusedMember.Global
        public static G9ConfigManagementSingleton<TConfigDataType> GetInstance(string configFileName = null,
            TConfigDataType customConfigObject = null, bool forceRemake = false, string baseApp = null,
            string configExtension = "config")
        {
            // Set index label
            var indexLabel = configFileName + configExtension;

            // Set config file name if it's null
            if (string.IsNullOrEmpty(configFileName))
                configFileName = typeof(TConfigDataType).Name;

            // Check and instance new config if need
            if (!ConfigsManagement.ContainsKey(indexLabel))
                ConfigsManagement.Add(indexLabel,
                    new G9ConfigManagementSingleton<TConfigDataType>(configFileName, customConfigObject, forceRemake,
                        baseApp, configExtension));

            // return config
            return ConfigsManagement[indexLabel];
        }

        #endregion

        /// <summary>
        ///     Force update config data and file with config object.
        ///     Notice: remake xml config even if it exists
        /// </summary>
        /// <param name="newConfigForUpdate">Specified object of config</param>

        #region ForceUpdate

        // ReSharper disable once UnusedMember.Global
        public void ForceUpdate(TConfigDataType newConfigForUpdate)
        {
            var indexLabel = ConfigFileName + ConfigExtension;
            ConfigsInformation[indexLabel] =
                new InitializeConfigFile<TConfigDataType>(ConfigFileName, newConfigForUpdate, true,
                    ConfigsInformation[indexLabel].BaseAppPath,
                    ConfigsInformation[indexLabel].ConfigExtension);
        }

        #endregion

        #endregion
    }
}