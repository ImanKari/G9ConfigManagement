using G9ConfigManagement.Abstract;

namespace G9ConfigManagement.DataType
{
    public struct G9DtConfigInitialize<TConfigType> where TConfigType : G9AConfigStructure<TConfigType>, new()
    {
        /// <summary>
        ///     Field to get an initialization of config structure
        ///     <para />
        ///     Essentially, it specifies the first config structure to create a config (file) if it's not existed.
        ///     <para />
        ///     Notice: This data is automatically used by the config core.
        /// </summary>
        public TConfigType ConfigInitialization;

        /// <summary>
        ///     Field to get config options
        ///     <para />
        ///     Notice: The implementation of the method must do with the programmer.
        ///     <para />
        ///     Notice: This data is automatically used by the config core.
        /// </summary>
        public G9DtConfigSettings ConfigOptions;

        /// <summary>
        /// </summary>
        /// <param name="configInitialization">
        ///     Specifies an initialization of config structure
        ///     <para />
        ///     Essentially, it specifies the first config structure to create a config (file) if it's not existed.
        ///     <para />
        ///     Notice: This data is automatically used by the config core.
        /// </param>
        /// <param name="configOptions">
        ///     Specifies the config settings
        ///     <para />
        ///     Notice: The implementation of the method must do with the programmer.
        ///     <para />
        ///     Notice: This data is automatically used by the config core.
        /// </param>
        public G9DtConfigInitialize(TConfigType configInitialization, G9DtConfigSettings configOptions)
        {
            ConfigInitialization = configInitialization;
            ConfigOptions = configOptions;
        }
    }
}