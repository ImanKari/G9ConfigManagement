using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        /// <param name="customConfigObject">
        ///     Optional: Specify custom object for create config xml file.
        ///     Just for create, if created don't use
        /// </param>
        /// <param name="forceUpdateWithObject">
        ///     Specify force update config xml file.
        ///     Recreate config file
        /// </param>

        #region InitializeConfigFile

        public InitializeConfigFile(string configFileName, TConfigDataType customConfigObject = null,
            bool forceUpdateWithObject = false)
        {
            // Initialize config if custom object is null
            ConfigDataType = customConfigObject ?? new TConfigDataType();

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
                if (customConfigObject != null && forceUpdateWithObject)
                {
                    File.Delete(configFileName);
                    _configXmlDocument = new XmlDocument();
                    CreateXmlConfigByType();
                }
                else
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
                        _configXmlDocument["Configuration"]?["ConfigVersion"]?.InnerText !=
                        ConfigDataType.ConfigVersion)
                    {
                        LoadConfigByType(false);
                        File.Delete(configFileName);
                        _configXmlDocument = new XmlDocument();
                        CreateXmlConfigByType();
                    }
                    // Else load config from config file
                    else
                    {
                        LoadConfigByType(true);
                    }
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
        ///     Get properties info from custom object
        /// </summary>
        /// <param name="objectForParse">Specify object for get property info</param>
        /// <returns></returns>

        #region GetPropertiesInfosFromObject

        private PropertyInfo[] GetPropertiesInfosFromObject(object objectForParse)
        {
            if (objectForParse is PropertyInfo)
                return (objectForParse as PropertyInfo).PropertyType.GetProperties().Where(s =>
                        !Attribute.IsDefined(s, typeof(Ignore)) && s.CanRead && s.CanWrite)
                    .ToArray();
            return objectForParse.GetType().GetProperties()
                .Where(s => !Attribute.IsDefined(s, typeof(Ignore)) && s.CanRead && s.CanWrite &&
                            s.Name != nameof(ConfigDataType.ConfigVersion))
                .ToArray();
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
                GetPropertiesInfosFromObject(ConfigDataType),
                ConfigDataType);

            // Add config version to xml
            AddConfigVersionToXml(rootNode);

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
            if (propertiesInfo.Any())
            {
                for (var i = 0; i < propertiesInfo.Length; i++)
                    WriteElement(rootNode, propertiesInfo[i], propertyObject);
            }
            else
            {
                WriteComment($"Property with set and get not found!", rootNode);
            }
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
            WriteHintCommentToXml(rootNode, memberPropertyInfo, memberObject);

            // Add notice required if need
            WriteRequiredNoticeToXml(rootNode, memberPropertyInfo);

            if (memberPropertyInfo.PropertyType.IsValueType || memberPropertyInfo.PropertyType == typeof(string))
            {
                XmlNode node = _configXmlDocument.CreateElement(memberPropertyInfo.Name);
                node.InnerText = memberPropertyInfo.GetValue(memberObject)?.ToString() ?? string.Empty;
                rootNode.AppendChild(node);
            }
            else
            {
                XmlNode node = _configXmlDocument.CreateElement(memberPropertyInfo.Name);
                var newNode = rootNode.AppendChild(node);
                WriteXmlByPropertiesInfo(
                    newNode,
                    GetPropertiesInfosFromObject(memberPropertyInfo),
                    memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject));
            }
        }

        #endregion

        /// <summary>
        ///     Check property and write comment element to xml if has hint attribute
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for read comment value</param>

        #region WriteHintCommentToXml

        private void WriteHintCommentToXml(XmlNode rootNode, PropertyInfo memberPropertyInfo, object memberObject)
        {
            #region Hint Comment

            // Set hint comment for config
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(Hint)).ToArray();
            if (hintAttr.Any())
                for (var i = 0; i < hintAttr.Length; i++)
                {
                    Hint oHint;
                    if ((oHint = hintAttr[i] as Hint) == null || string.IsNullOrEmpty(oHint.HintForProperty))
                        continue;
                    // Write comment
                    WriteComment(oHint.HintForProperty, rootNode);
                }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Check property and write required notice element to xml if has hint attribute
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>

        #region WriteRequiredNoticeToXml

        private void WriteRequiredNoticeToXml(XmlNode rootNode, PropertyInfo memberPropertyInfo)
        {
            if (memberPropertyInfo.GetCustomAttributes(typeof(Required)).Any())
                // Write comment
                WriteComment(" ### Notice: This element is required! ### ", rootNode);
        }

        #endregion

        /// <summary>
        ///     Write comment to xml
        /// </summary>
        /// <param name="comment">Custom comment message</param>
        /// <param name="rootNode">Specify root xml node for write</param>

        #region WriteComment

        private void WriteComment(string comment, XmlNode rootNode)
        {
            var hintComment = _configXmlDocument.CreateComment(comment);
            rootNode.AppendChild(hintComment);
        }

        #endregion

        /// <summary>
        ///     Set config object by xml data
        /// </summary>
        /// <param name="checkRequired">Check required item</param>

        #region LoadConfigByType

        private void LoadConfigByType(bool checkRequired)
        {
            ReadXmlByPropertiesInfo(
                GetPropertiesInfosFromObject(ConfigDataType),
                ConfigDataType, _configXmlDocument["Configuration"], checkRequired);
        }

        #endregion

        /// <summary>
        ///     Read xml and set object from xml
        /// </summary>
        /// <param name="propertiesInfo">Specify all property infos from config object</param>
        /// <param name="propertyObject">Config object</param>
        /// <param name="element">Element for read</param>
        /// <param name="checkRequired">Check required item</param>

        #region ReadXmlByPropertiesInfo

        private void ReadXmlByPropertiesInfo(PropertyInfo[] propertiesInfo, object propertyObject, XmlElement element,
            bool checkRequired)
        {
            // Return if object is null
            if (propertyObject == null || element == null) return;

            // Create elements with properties info
            for (var i = 0; i < propertiesInfo.Length; i++)
                ReadElement(propertiesInfo[i], propertyObject, element, checkRequired);
        }

        #endregion

        /// <summary>
        ///     Read element from xml and set object
        /// </summary>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for set values from xml</param>
        /// <param name="element">Specify element for read</param>
        /// <param name="checkRequired">Check required item</param>

        #region ReadElement

        private void ReadElement(PropertyInfo memberPropertyInfo, object memberObject, XmlElement element,
            bool checkRequired)
        {
            if (memberPropertyInfo.PropertyType.IsValueType || memberPropertyInfo.PropertyType == typeof(string))
            {
                // Check for required config item
                if (checkRequired && memberPropertyInfo.GetCustomAttributes(typeof(Required)).Any() &&
                    string.IsNullOrEmpty(element[memberPropertyInfo.Name]?.InnerText))
                    throw new Exception(
                        $"Property {memberPropertyInfo.Name} in config is requirement, but isn't set in the config file. config file name:'{ConfigFileName}'");
                // Set config value
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToPropertyType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText));
            }
            else
            {
                ReadXmlByPropertiesInfo(
                    GetPropertiesInfosFromObject(memberPropertyInfo),
                    memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject),
                    element[memberPropertyInfo.Name], checkRequired);
            }
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
                // Parse for enum
                if (propertyInformation.PropertyType.IsEnum) return Enum.Parse(propertyInformation.PropertyType, value);
                // Parse for other type
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
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(text);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString();
            }
        }

        #endregion

        /// <summary>
        ///     Add config version element with comment to xml
        /// </summary>
        /// <param name="rootNode">specify node for write</param>

        #region AddConfigVersionToXml

        private void AddConfigVersionToXml(XmlNode rootNode)
        {
            // Write comment
            WriteComment("Specify config version (automatic set by config management, don't change)", rootNode);
            XmlNode node = _configXmlDocument.CreateElement(nameof(ConfigDataType.ConfigVersion));
            node.InnerText = ConfigDataType.ConfigVersion;
            rootNode.AppendChild(node);
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