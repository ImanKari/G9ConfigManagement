using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.Interface;

namespace G9ConfigManagement.Helper
{
    /// <summary>
    ///     Class management config file
    /// </summary>
    /// <typeparam name="TConfigDataType">Type of config object</typeparam>
    internal class InitializeConfigFile<TConfigDataType>
        where TConfigDataType : class, IConfigDataType, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Save config file name
        /// </summary>
        public string ConfigFileName { get; }

        /// <summary>
        ///     Save config data type
        /// </summary>
        public TConfigDataType ConfigDataType { get; }

        /// <summary>
        ///     Save xml document for config
        /// </summary>
        private readonly XmlDocument _configXmlDocument = new XmlDocument();

        /// <summary>
        ///     Specify config data type element name
        ///     Value is FullName of config type converted to md5
        /// </summary>
        public const string ConfigDataTypeElement = "ConfigDataType";

        #endregion

        #region Methods 

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="configFileName">Specify config file name</param>

        #region InitializeConfigFile

        public InitializeConfigFile(string configFileName)
        {
            // Initialize config
            ConfigDataType = new TConfigDataType();
            // Check version null
            if (string.IsNullOrEmpty(ConfigDataType.ConfigVersion))
                throw new NullReferenceException(
                    $"Config version property '{nameof(ConfigDataType.ConfigVersion)}', can be null!");

            // Set config file name
            ConfigFileName = configFileName;

            // Create or load config data
            // If file exists load
            if (File.Exists(configFileName))
            {
                _configXmlDocument.Load(ConfigFileName);
                // Check data type is equal with this type
                if (string.IsNullOrEmpty(_configXmlDocument["Configuration"]?[ConfigDataTypeElement]?.InnerText) ||
                    _configXmlDocument["Configuration"]?[ConfigDataTypeElement]?.InnerText !=
                    CreateMd5(ConfigDataType.GetType().FullName ?? ConfigDataType.GetType().Name))
                    throw new Exception(
                        $"A file with this name '{configFileName}' exists for another data type. If you need this file for new type of data, please delete it to recreate.");

                // If config version change
                // Remake with change value
                if (string.IsNullOrEmpty(_configXmlDocument["Configuration"]?["ConfigVersion"]?.InnerText) ||
                    _configXmlDocument["Configuration"]?["ConfigVersion"]?.InnerText != ConfigDataType.ConfigVersion)
                {
                    LoadConfigByType();
                    File.Delete(configFileName);
                    _configXmlDocument = new XmlDocument();
                    CreateXmlConfigByType();
                }
                // Else load config from config file
                else
                {
                    LoadConfigByType();
                }
            }
            else
            {
                // Create config file with config object
                CreateXmlConfigByType();
            }
        }

        #endregion

        /// <summary>
        ///     Create xml config file by data type
        /// </summary>

        #region CreateXmlConfigByType

        private void CreateXmlConfigByType()
        {
            // Set header
            var xmlDeclaration = _configXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            _configXmlDocument.InsertBefore(xmlDeclaration, _configXmlDocument.DocumentElement);

            // Create Element
            var rootNode = _configXmlDocument.AppendChild(_configXmlDocument.CreateElement("Configuration"));

            // Get all properties without attribute ignore
            WriteXmlByPropertiesInfo(rootNode,
                ConfigDataType.GetType().GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore))).ToArray(),
                ConfigDataType);

            // Add data type xml
            AddConfigDataTypeToXml(rootNode);

            // Save config file
            _configXmlDocument.Save(ConfigFileName);
        }

        #endregion

        /// <summary>
        ///     Read xml and set object from xml
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="propertiesInfo">Specify all property infos from config object</param>
        /// <param name="propertyObject">Config object for get values</param>

        #region WriteXmlByPropertiesInfo

        private void WriteXmlByPropertiesInfo(XmlNode rootNode, PropertyInfo[] propertiesInfo, object propertyObject)
        {
            // Return if object is null
            if (propertyObject == null) return;

            // Create elements with properties info
            for (var i = 0; i < propertiesInfo.Length; i++) WriteElement(rootNode, propertiesInfo[i], propertyObject);
        }

        #endregion

        /// <summary>
        ///     Write element tag and data to xml
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for read comment value</param>

        #region WriteElement

        private void WriteElement(XmlNode rootNode, PropertyInfo memberPropertyInfo, object memberObject)
        {
            // Return if object is null
            if (memberObject == null) return;

            // Add Comment if need
            WriteCommentElement(rootNode, memberPropertyInfo, memberObject);

            if (memberPropertyInfo.PropertyType.IsValueType || memberPropertyInfo.PropertyType == typeof(string))
            {
                XmlNode node = _configXmlDocument.CreateElement(memberPropertyInfo.Name);
                node.InnerText = memberPropertyInfo.GetValue(memberObject).ToString();
                rootNode.AppendChild(node);
            }
            else
            {
                XmlNode node = _configXmlDocument.CreateElement(memberPropertyInfo.Name);
                var newNode = rootNode.AppendChild(node);
                WriteXmlByPropertiesInfo(
                    newNode,
                    memberPropertyInfo.PropertyType.GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore)))
                        .ToArray(),
                    memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject));
            }
        }

        #endregion

        /// <summary>
        ///     Write comment element to xml
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for read comment value</param>

        #region WriteCommentElement

        private void WriteCommentElement(XmlNode rootNode, PropertyInfo memberPropertyInfo, object memberObject)
        {
            #region Hint Comment

            // Set hint comment for config
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(Hint)).ToArray();
            var isConfigVersion = false;
            if (memberObject is TConfigDataType configVersion &&
                (isConfigVersion = memberPropertyInfo.Name == nameof(configVersion.ConfigVersion)) || hintAttr.Any())
            {
                if (isConfigVersion)
                {
                    var hintComment =
                        _configXmlDocument.CreateComment(
                            "Specify config version (automatic set by config management, don't change)");
                    rootNode.AppendChild(hintComment);
                }
                else
                {
                    for (var i = 0; i < hintAttr.Length; i++)
                    {
                        Hint oHint;
                        if ((oHint = hintAttr[i] as Hint) == null || string.IsNullOrEmpty(oHint.HintForProperty))
                            continue;
                        var hintComment = _configXmlDocument.CreateComment(oHint.HintForProperty);
                        rootNode.AppendChild(hintComment);
                    }
                }
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Set config object by xml data
        /// </summary>

        #region LoadConfigByType

        private void LoadConfigByType()
        {
            ReadXmlByPropertiesInfo(
                ConfigDataType.GetType().GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore))).ToArray(),
                ConfigDataType, _configXmlDocument["Configuration"]);
        }

        #endregion

        /// <summary>
        ///     Read xml and set object from xml
        /// </summary>
        /// <param name="propertiesInfo">Specify all property infos from config object</param>
        /// <param name="propertyObject">Config object</param>
        /// <param name="element">Element for read</param>

        #region ReadXmlByPropertiesInfo

        private void ReadXmlByPropertiesInfo(PropertyInfo[] propertiesInfo, object propertyObject, XmlElement element)
        {
            // Return if object is null
            if (propertyObject == null || element == null) return;

            // Create elements with properties info
            for (var i = 0; i < propertiesInfo.Length; i++) ReadElement(propertiesInfo[i], propertyObject, element);
        }

        #endregion

        /// <summary>
        ///     Read element from xml and set object
        /// </summary>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for set values from xml</param>
        /// <param name="element">Specify element for read</param>

        #region ReadElement

        private void ReadElement(PropertyInfo memberPropertyInfo, object memberObject, XmlElement element)
        {
            if (memberPropertyInfo.PropertyType.IsValueType || memberPropertyInfo.PropertyType == typeof(string))
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToPropertyType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText));
            else
                ReadXmlByPropertiesInfo(
                    memberPropertyInfo.PropertyType.GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore)))
                        .ToArray(),
                    memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject),
                    element[memberPropertyInfo.Name]);
        }

        #endregion

        /// <summary>
        ///     Cast string value to property type
        /// </summary>
        /// <param name="propertyInformation">Specify property information for cast</param>
        /// <param name="value">String value for cast to property type</param>
        /// <returns></returns>

        #region CastStringToPropertyType

        private object CastStringToPropertyType(PropertyInfo propertyInformation, string value)
        {
            try
            {
                if (propertyInformation.PropertyType.IsEnum) return Enum.Parse(propertyInformation.PropertyType, value);
                return Convert.ChangeType(value, propertyInformation.PropertyType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"Invalid cast exception. property name: '{propertyInformation.Name}'. property value: '{value}'. Type for cast: '{propertyInformation.PropertyType}'.",
                    ex);
            }
        }

        #endregion


        /// <summary>
        ///     Generate MD5 from text
        /// </summary>
        /// <param name="text">Specify text</param>
        /// <returns>Return MD5 from text</returns>

        #region CreateMd5

        private string CreateMd5(string text)
        {
            using var md5 = MD5.Create();
            var encoding = Encoding.ASCII;
            var data = encoding.GetBytes(text);

            Span<byte> hashBytes = stackalloc byte[16];
            md5.TryComputeHash(data, hashBytes, out var written);
            if (written != hashBytes.Length)
                throw new OverflowException();


            Span<char> stringBuffer = stackalloc char[32];
            for (var i = 0; i < hashBytes.Length; i++)
                hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
            return new string(stringBuffer);
        }

        #endregion

        /// <summary>
        ///     Generate and add data type element with comment to xml
        /// </summary>
        /// <param name="rootNode">specify node for write</param>

        #region AddConfigDataTypeToXml

        private void AddConfigDataTypeToXml(XmlNode rootNode)
        {
            var hintComment =
                _configXmlDocument.CreateComment(
                    "Specify data type (automatic use by config management, don't change)");
            rootNode.AppendChild(hintComment);
            XmlNode node = _configXmlDocument.CreateElement(ConfigDataTypeElement);
            node.InnerText = CreateMd5(ConfigDataType.GetType().FullName ?? ConfigDataType.GetType().Name);
            rootNode.AppendChild(node);
        }

        #endregion

        #endregion
    }
}