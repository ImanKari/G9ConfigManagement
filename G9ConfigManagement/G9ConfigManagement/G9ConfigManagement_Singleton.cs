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
    public class G9ConfigManagement_Singleton<TConfigDataType>
        where TConfigDataType : class, IConfigDataType, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Dictionary save initialized object
        ///     use for multi config
        /// </summary>
        private static readonly Dictionary<string, G9ConfigManagement_Singleton<TConfigDataType>> _configsManagement
            = new Dictionary<string, G9ConfigManagement_Singleton<TConfigDataType>>();

        /// <summary>
        ///     Dictionary save initialized config
        ///     use for multi config
        /// </summary>
        private static readonly Dictionary<string, InitializeConfigFile<TConfigDataType>> _configsInformation
            = new Dictionary<string, InitializeConfigFile<TConfigDataType>>();

        /// <summary>
        ///     Specify config name
        /// </summary>
        public string ConfigFileName { get; }

        /// <summary>
        ///     Access to config data value
        /// </summary>
        public TConfigDataType Configuration => _configsInformation[ConfigFileName].ConfigDataType;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement data
        /// </summary>

        #region G9LogConfig

        private G9ConfigManagement_Singleton(string configFileName)
        {
            try
            {
                // Set config file name 
                ConfigFileName = configFileName;
                // Initialize config files
                _configsInformation.Add(ConfigFileName, new InitializeConfigFile<TConfigDataType>(configFileName));
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

        #region G9LogConfig_Singleton

        public static G9ConfigManagement_Singleton<TConfigDataType> GetInstance(string configFileName = null)
        {
            // Set config file name if it's null
            if (string.IsNullOrEmpty(configFileName))
                configFileName = typeof(TConfigDataType).Name;

            // Check and instance new config if need
            if (!_configsManagement.ContainsKey(configFileName))
                _configsManagement.Add(configFileName,
                    new G9ConfigManagement_Singleton<TConfigDataType>(configFileName));

            // return config
            return _configsManagement[configFileName];
        }

        #endregion

        #endregion
    }
}