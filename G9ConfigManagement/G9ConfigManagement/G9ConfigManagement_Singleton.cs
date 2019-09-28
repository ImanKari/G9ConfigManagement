using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using G9ConfigManagement.Helper;
using G9ConfigManagement.Interface;

namespace G9ConfigManagement
{
    public class G9ConfigManagement_Singleton<TConfigDataType>
        where TConfigDataType : class, IConfigDataType, new()
    {
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
                throw new Exception($"Error when read file 'G9Log.config' and parse config...\n{ex}");
            }
        }

        #endregion

        /// <summary>
        ///     Generate MD5 from text
        /// </summary>
        /// <param name="s">Specify text</param>
        /// <returns>Return MD5 from text</returns>

        #region CreateMD5

        private string CreateMD5(string s)
        {
            using (var md5 = MD5.Create())
            {
                var encoding = Encoding.ASCII;
                var data = encoding.GetBytes(s);

                Span<byte> hashBytes = stackalloc byte[16];
                md5.TryComputeHash(data, hashBytes, out var written);
                if (written != hashBytes.Length)
                    throw new OverflowException();


                Span<char> stringBuffer = stackalloc char[32];
                for (var i = 0; i < hashBytes.Length; i++)
                    hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
                return new string(stringBuffer);
            }
        }

        #endregion

        /// <summary>
        ///     Get instance
        ///     Singleton pattern
        /// </summary>
        /// <returns>Instance of class</returns>

        #region G9LogConfig_Singleton

        public static G9ConfigManagement_Singleton<TConfigDataType> GetInstance(string configFileName)
        {
            if (!_configsManagement.ContainsKey(configFileName))
                _configsManagement.Add(configFileName,
                    new G9ConfigManagement_Singleton<TConfigDataType>(configFileName));

            return _configsManagement[configFileName];
        }

        #endregion

        #endregion

        #region Fields And Properties


        private static Dictionary<string, G9ConfigManagement_Singleton<TConfigDataType>> _configsManagement
            = new Dictionary<string, G9ConfigManagement_Singleton<TConfigDataType>>();

        private static Dictionary<string, InitializeConfigFile<TConfigDataType>> _configsInformation
            = new Dictionary<string, InitializeConfigFile<TConfigDataType>>();

        public string ConfigFileName { get; }

        public TConfigDataType Configuration => _configsInformation[ConfigFileName].ConfigDataType;

        #endregion
    }
}
