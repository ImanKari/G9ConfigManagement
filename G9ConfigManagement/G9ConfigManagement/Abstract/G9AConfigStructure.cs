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
        [G9AttrOrder(uint.MaxValue)]
        public abstract G9DtConfigVersion ConfigVersion { set; get; }

        /// <summary>
        ///     Access to configuration
        /// </summary>
        public static TConfigType GetConfig()
        {
            return G9ConfigManagement<TConfigType>.GetOrCreate();
        }

        /// <inheritdoc cref="G9ConfigManagement{TConfigDataType}.RestoreToDefaultByConfigFile"/>
        public static void RestoreByConfigFile()
        {
            G9ConfigManagement<TConfigType>.RestoreToDefaultByConfigFile();
        }

        /// <summary>
        ///     Method to remake and restore the config by default initialized value.
        ///     <para />
        ///     In this case, the config file is remade, and the config data is restored to the first structure.
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

        /// <inheritdoc cref="G9ConfigManagement{TConfigDataType}.SetConfigSettings" />
        public static void SetConfigSettings(G9DtConfigSettings settings)
        {
            G9ConfigManagement<TConfigType>.SetConfigSettings(settings);
        }

        /// <inheritdoc cref="G9ConfigManagement{TConfigDataType}.SetInstanceForInitialization" />
        public static void SetInstanceForInitialization(TConfigType instance)
        {
            G9ConfigManagement<TConfigType>.SetInstanceForInitialization(instance);
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