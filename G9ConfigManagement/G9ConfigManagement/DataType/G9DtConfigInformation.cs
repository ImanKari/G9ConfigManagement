namespace G9ConfigManagement.DataType
{
    public readonly struct G9DtConfigInformation
    {
        /// <summary>
        ///     Specifies the config file name.
        /// </summary>
        public readonly string ConfigFileName;

        /// <summary>
        ///     Specifies the config file extension.
        /// </summary>
        public readonly string ConfigFileExtension;

        /// <summary>
        ///     Specifies the config path.
        /// </summary>
        public readonly string ConfigPath;

        /// <summary>
        ///     Specifies the config full path.
        /// </summary>
        public readonly string ConfigFullPath;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="configFileName">Specifies the config file name.</param>
        /// <param name="configFileExtension">Specifies the config file extension.</param>
        /// <param name="configPath">Specifies the config path.</param>
        /// <param name="configFullPath">Specifies the config full path.</param>
        public G9DtConfigInformation(string configFileName, string configFileExtension, string configPath,
            string configFullPath)
        {
            ConfigFileName = configFileName;
            ConfigFileExtension = configFileExtension;
            ConfigPath = configPath;
            ConfigFullPath = configFullPath;
        }
    }
}