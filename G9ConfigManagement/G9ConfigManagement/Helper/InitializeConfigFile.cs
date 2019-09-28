using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.Interface;

namespace G9ConfigManagement.Helper
{
    internal class InitializeConfigFile<TConfigDataType>
        where TConfigDataType : class, IConfigDataType, new()
    {
        #region Fields And Properties

        /// <summary>
        /// Save config file name
        /// </summary>
        public string ConfigFileName { get; }

        /// <summary>
        /// Save config data type
        /// </summary>
        public TConfigDataType ConfigDataType { private set; get; }

        /// <summary>
        /// Save xml document for config
        /// </summary>
        private XmlDocument _configXmlDocument = new XmlDocument();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configFileName">Specify config file name</param>
        #region InitializeConfigFile
        public InitializeConfigFile(string configFileName)
        {
            ConfigFileName = configFileName;
            if (File.Exists(configFileName))
            {
                _configXmlDocument.Load(ConfigFileName);
                LoadConfigByType();
            }
            else
                CreateConfigByType();
        }
        #endregion

        /// <summary>
        /// Create config file by data type
        /// </summary>

        #region CreateConfigByType

        public void CreateConfigByType()
        {
            var dataType = ConfigDataType = new TConfigDataType();

            // Set header
            XmlDeclaration xmlDeclaration = _configXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            _configXmlDocument.InsertBefore(xmlDeclaration, _configXmlDocument.DocumentElement);

            // Create Element
            var _rootNode = _configXmlDocument.AppendChild(_configXmlDocument.CreateElement("Configuration"));

            // Get all properties without attribute ignore
            WriteXmlByPropertiesInfo(_rootNode,
                dataType.GetType().GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore))).ToArray(),
                dataType);

            // Save config file
            _configXmlDocument.Save(ConfigFileName);
        }

        #endregion


        private void WriteXmlByPropertiesInfo(XmlNode rootNode, PropertyInfo[] propertiesInfo, object propertyObject)
        {
            // Return if object is null
            if (propertyObject == null) return;

            // Create elements with properties info
            for (var i = 0; i < propertiesInfo.Length; i++)
            {
                WriteElement(rootNode, propertiesInfo[i], propertyObject);
            }
        }

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
                        .ToArray(), memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject));
            }
        }

        private void WriteCommentElement(XmlNode rootNode, PropertyInfo memberPropertyInfo, object memberObject)
        {
            #region Hint Comment

            // Set hint comment for config
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(Hint)).ToArray();
            bool isConfigVersion = false;
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

        /// <summary>
        /// Load config by 
        /// </summary>
        #region LoadConfigByType
        public void LoadConfigByType()
        {
            var dataType = ConfigDataType = new TConfigDataType();
            ReadXmlByPropertiesInfo(
                dataType.GetType().GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore))).ToArray(),
                dataType, _configXmlDocument["Configuration"]);
        }
        #endregion

        private void ReadXmlByPropertiesInfo(PropertyInfo[] propertiesInfo, object propertyObject, XmlElement element)
        {
            // Return if object is null
            if (propertyObject == null || element == null) return;

            // Create elements with properties info
            for (var i = 0; i < propertiesInfo.Length; i++)
            {
                ReadElement(propertiesInfo[i], propertyObject, element);
            }
        }

        private void ReadElement(PropertyInfo memberPropertyInfo, object memberObject, XmlElement element)
        {
            
            if (memberPropertyInfo.PropertyType.IsValueType || memberPropertyInfo.PropertyType == typeof(string))
            {
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToCustomType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText));
            }
            else
            {
                ReadXmlByPropertiesInfo(
                    memberPropertyInfo.PropertyType.GetProperties().Where(s => !Attribute.IsDefined(s, typeof(Ignore))).ToArray(),
                    memberObject.GetType().GetProperty(memberPropertyInfo.Name)?.GetValue(memberObject), element[memberPropertyInfo.Name]);
            }
        }

        private object CastStringToCustomType(PropertyInfo propertyInformation, string value)
        {
            if (propertyInformation.PropertyType.IsEnum)
            {
                return Enum.Parse(propertyInformation.PropertyType, value);
            }
            else
            {
                return Convert.ChangeType(value, propertyInformation.PropertyType);
            }
        }

    }
}
