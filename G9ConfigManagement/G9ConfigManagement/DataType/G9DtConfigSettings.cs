using System;
using G9AssemblyManagement;
using G9AssemblyManagement.Enums;
using G9ConfigManagement.Enum;

namespace G9ConfigManagement.DataType
{
    /// <summary>
    ///     The data type for specifying config options
    /// </summary>
    public class G9DtConfigSettings
    {
        #region Methods

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="changeVersionReaction">
        ///     Specifies the reaction when in a config structure a change version happens.
        ///     <para />
        ///     By default, it's set on '<see cref="G9EChangeVersionReaction.MergeThenOverwrite" />'.
        /// </param>
        /// <param name="configFileName">
        ///     Specifies the name of the config file
        ///     <para />
        ///     By default, it's set to the name of the config structure (class name).
        /// </param>
        /// <param name="configFileExtension">
        ///     Specifies the extension of the config file
        ///     <para />
        ///     By default, it's set on '.json'.
        /// </param>
        /// <param name="configFileLocation">
        ///     Specifies the location of the config file
        ///     <para />
        ///     By default, it's set on the base location of the application.
        /// </param>
        /// <param name="enableAutomatedCreatingPath">
        ///     Specifies whether the automated creating for the directory path is enabled or not.
        ///     <para />
        ///     By default, it's set to "true."
        /// </param>
        public G9DtConfigSettings(
            G9EChangeVersionReaction changeVersionReaction = G9EChangeVersionReaction.MergeThenOverwrite,
            string configFileName = null,
            string configFileExtension = "json",
            string configFileLocation = null,
            bool enableAutomatedCreatingPath = true)
        {
            ConfigFileExtension = string.IsNullOrEmpty(configFileExtension)
                ? "json"
                : configFileExtension;

            if (!string.IsNullOrEmpty(configFileName))
            {
                var fullFileName = $"{configFileName}.json";
                if (G9Assembly.InputOutputTools.CheckFilePathValidation(fullFileName, false, false) !=
                    G9EPatchCheckResult.Correct)
                    throw new ArgumentException(
                        $"The fixed value '{configFileName}' in the specified parameter '{nameof(configFileName)}' is incorrect regarding a file name. The core can't use it as a file name.",
                        nameof(configFileName));
            }

            if (!string.IsNullOrEmpty(configFileExtension))
            {
                var fullFileName = $"Config.{configFileExtension}";
                if (G9Assembly.InputOutputTools.CheckFilePathValidation(fullFileName, false, false) !=
                    G9EPatchCheckResult.Correct)
                    throw new ArgumentException(
                        $"The fixed value '{configFileExtension}' in the specified parameter '{nameof(configFileExtension)}' is incorrect regarding a file name. The core can't use it as a file name.",
                        nameof(configFileExtension));
            }

            if (!string.IsNullOrEmpty(configFileLocation))
            {
                var result =
                    G9Assembly.InputOutputTools.CheckDirectoryPathValidation(configFileLocation, true,
                        !enableAutomatedCreatingPath);
                switch (result)
                {
                    case G9EPatchCheckResult.PathNameIsIncorrect:
                        throw new ArgumentException(
                            $"The fixed value '{configFileLocation}' in the specified parameter '{nameof(configFileLocation)}' is incorrect regarding a directory path. The core can't use it as a directory path.",
                            nameof(configFileLocation));
                    case G9EPatchCheckResult.PathDriveIsIncorrect:
                        throw new ArgumentException(
                            $"The fixed value '{configFileLocation}' in the specified parameter '{nameof(configFileLocation)}' is incorrect regarding the directory drive. The specified drive doesn't exist.",
                            nameof(configFileLocation));
                    case G9EPatchCheckResult.PathExistenceIsIncorrect:
                        throw new ArgumentException(
                            $"The fixed value '{configFileLocation}' in the specified parameter '{nameof(configFileLocation)}' is incorrect regarding the path existence. The specified path doesn't exist.",
                            nameof(configFileLocation));
                    case G9EPatchCheckResult.Correct:
                    default:
                        break;
                }
            }


            EnableAutomatedCreatingPath = enableAutomatedCreatingPath;
            ConfigFileName = configFileName;
            ConfigFileLocation = configFileLocation;
            ChangeVersionReaction = changeVersionReaction;
        }

        #endregion

        #region Properties And Fields

        /// <summary>
        ///     Specifies the reaction when in a config structure a change version happens.
        ///     <para />
        ///     By default, it's set on '<see cref="G9EChangeVersionReaction.MergeThenOverwrite" />'.
        /// </summary>
        public readonly G9EChangeVersionReaction ChangeVersionReaction;

        /// <summary>
        ///     Specifies the extension of the config file
        ///     <para />
        ///     By default, it's set on '.json'.
        /// </summary>
        public readonly string ConfigFileExtension;

        /// <summary>
        ///     Specifies the location of the config file
        ///     <para />
        ///     By default, it's set on the base location of the application.
        /// </summary>
        public readonly string ConfigFileLocation;

        /// <summary>
        ///     Specifies whether the automated creating for the directory path is enabled or not.
        ///     <para />
        ///     By default, it's set to "true."
        /// </summary>
        public readonly bool EnableAutomatedCreatingPath;

        /// <summary>
        ///     Specifies the name of the config file
        ///     <para />
        ///     By default, it's set to the name of the config structure (class name).
        /// </summary>
        public readonly string ConfigFileName;

        #endregion
    }
}