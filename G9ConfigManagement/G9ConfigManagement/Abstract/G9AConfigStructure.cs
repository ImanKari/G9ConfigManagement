using System.IO;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;
using G9JSONHandler.Attributes;

namespace G9ConfigManagement.Abstract
{
    /// <summary>
    ///     An abstract structure for config consists of the requirements of config structure.
    /// </summary>
    /// <typeparam name="TConfigType">Specifies the config file structure that must be creatable.</typeparam>
    public abstract class G9AConfigStructure<TConfigType> where TConfigType : G9AConfigStructure<TConfigType>, new()
    {
        /// <summary>
        ///     Specifies the version of the config
        /// </summary>
        [G9AttrRequired]
        public abstract G9DtConfigVersion ConfigVersion { set; get; }

        /// <summary>
        ///     Access to configuration
        /// </summary>
        public static TConfigType GetConfig()
        {
            return G9ConfigManagement<TConfigType>.GetOrCreate();
        }

        /// <summary>
        ///     Method to restore config data by existing data in the config file.
        /// </summary>
        public static void RestoreByConfigFile()
        {
            G9ConfigManagement<TConfigType>.RestoreToDefaultByConfigFile();
        }

        /// <summary>
        ///     Method to remake and restore the config by default initialized value.
        ///     <para />
        ///     In this case, the created config file is remade, and the config data restore by the new specified value.
        ///     <para />
        ///     This default value would be gotten from the method '<see cref="Initialize" />.'
        /// </summary>
        public static void RemakeAndRestoreByDefaultValue()
        {
            G9ConfigManagement<TConfigType>.RestoreToDefault();
        }

        /// <summary>
        ///     Method to remake and restore the config by custom initialized value.
        ///     <para />
        ///     In this case, the created config file is remade, and the config data restore by the new specified value.
        /// </summary>
        /// <param name="customConfigValue">Specifies a custom initialized config value for remaking.</param>
        public static void RemakeAndRestoreByCustomValue(TConfigType customConfigValue)
        {
            G9ConfigManagement<TConfigType>.Update(customConfigValue);
        }

        /// <summary>
        ///     Method to remake the config file by the current config structure values.
        ///     <para />
        ///     In this case, the created config file is remade by the currently specified value set on the config structure.
        /// </summary>
        public void RemakeConfigFileByCurrentValue()
        {
            G9ConfigManagement<TConfigType>.Update((TConfigType)this);
        }


        /// <summary>
        ///     Method to specify the initialized requirements of config.
        ///     <para />
        ///     Notice: This data is automatically used by the config core.
        /// </summary>
        /// <returns></returns>
        public virtual G9DtConfigInitialize<TConfigType> Initialize()
        {
            return new G9DtConfigInitialize<TConfigType>(new TConfigType(), new G9DtConfigSettings());
        }


        #region Config information Fields and Properties

        /// <summary>
        ///     Access to config information
        /// </summary>
        [G9AttrIgnore]
        public G9DtConfigInformation ConfigInformation { private set; get; }


        /// <summary>
        ///     Method to set config basis data.
        ///     <para />
        ///     This method is automatically called by the core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void G9SetConfigBasisData(string configFileName, string configFileExtension, string configPath)
        {
            ConfigInformation = new G9DtConfigInformation(configFileName, configFileExtension, configPath,
                Path.Combine(configPath, $"{configFileName}.{configFileExtension}"));
        }

        #endregion
    }
}