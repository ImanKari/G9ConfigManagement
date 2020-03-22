using System;
using System.IO;
using System.Linq;
using System.Net;
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
        #region Enums

        /// <summary>
        ///     Specify supported type bye config management
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public enum SupportedTypes
        {
            // ReSharper disable once UnusedMember.Global
            SbyteType,

            // ReSharper disable once UnusedMember.Global
            ShortType,

            // ReSharper disable once UnusedMember.Global
            IntType,

            // ReSharper disable once UnusedMember.Global
            LongType,

            // ReSharper disable once UnusedMember.Global
            ByteType,

            // ReSharper disable once UnusedMember.Global
            UshortType,

            // ReSharper disable once UnusedMember.Global
            UintType,

            // ReSharper disable once UnusedMember.Global
            UlongType,

            // ReSharper disable once UnusedMember.Global
            CharType,

            // ReSharper disable once UnusedMember.Global
            FloatType,

            // ReSharper disable once UnusedMember.Global
            DoubleType,

            // ReSharper disable once UnusedMember.Global
            DecimalType,

            // ReSharper disable once UnusedMember.Global
            BoolType,

            // ReSharper disable once UnusedMember.Global
            EnumType,

            // ReSharper disable once UnusedMember.Global
            StringType,

            // ReSharper disable once UnusedMember.Global
            DateTimeType,

            // ReSharper disable once UnusedMember.Global
            TimeSpanType,

            // ReSharper disable once UnusedMember.Global
            GuidType,

            // ReSharper disable once UnusedMember.Global
            IpAddressType,

            // ReSharper disable once UnusedMember.Global
            TimeSpan
        }

        #endregion

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

        /// <summary>
        ///     Save supported types by config management
        /// </summary>
        private readonly Type[] _typesSupportedByConfig =
        {
            typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(byte), typeof(ushort), typeof(uint),
            typeof(ulong), typeof(char), typeof(float), typeof(double), typeof(decimal), typeof(bool), typeof(Enum),
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(IPAddress)
        };

        /// <summary>
        ///     Access to base app path
        /// </summary>
        public readonly string BaseAppPath;

        /// <summary>
        ///     Access to config extension
        /// </summary>
        public readonly string ConfigExtension;

        /// <summary>
        ///     Access full config path and name
        /// </summary>
        public readonly string FullConfigPath;

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
        /// <param name="baseApp">
        ///     <para>Specified base path of application for create config</para>
        ///     <para>Notice: if set null => use 'BaseDirectory' value</para>
        /// </param>
        /// <param name="configExtension">
        ///     <para>Specified config extension</para>
        ///     <para>Notice: if not set argument => use default extension '.config'</para>
        /// </param>

        #region InitializeConfigFile

        public InitializeConfigFile(string configFileName, TConfigDataType customConfigObject = null,
            bool forceUpdateWithObject = false, string baseApp = null, string configExtension = "config")
        {
            // Check and set base app
            BaseAppPath =
                string.IsNullOrEmpty(baseApp)
                    ?
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
                    AppDomain.CurrentDomain.BaseDirectory
#else
                    AppContext.BaseDirectory
#endif
                    : baseApp;

            // Check and Set config file name
            ConfigFileName = string.IsNullOrEmpty(configFileName)
                ? throw new ArgumentNullException(nameof(configFileName))
                : configFileName.IndexOfAny(Path.GetInvalidPathChars()) > 0
                    ? throw new ArgumentException($"Invalid file name exception: '{configFileName}'",
                        nameof(configFileName))
                    : configFileName;


            // Check and Set config extension
            ConfigExtension = string.IsNullOrEmpty(configExtension)
                ? "config"
                : configFileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
                  configFileName.Length == 1 && configFileName == "."
                    ? throw new ArgumentException($"Invalid file name exception: '{configFileName}'",
                        nameof(configFileName))
                    : configExtension.StartsWith(".")
                        ? configExtension.Substring(1)
                        : configExtension;

            // Set config path
            FullConfigPath = Path.Combine(BaseAppPath, $"{ConfigFileName}.{ConfigExtension}");

            // Initialize config if custom object is null
            ConfigDataType = customConfigObject ?? new TConfigDataType();

            // Check version null
            if (string.IsNullOrEmpty(ConfigDataType.ConfigVersion))
                throw new NullReferenceException(
                    $"Config version property '{nameof(ConfigDataType.ConfigVersion)}', can be null!");

            // Create or load config data
            // If file exists load
            if (File.Exists(FullConfigPath))
            {
                if (customConfigObject != null && forceUpdateWithObject)
                {
                    File.Delete(FullConfigPath);
                    _configXmlDocument = new XmlDocument();
                    CreateXmlConfigByType();
                }
                else
                {
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
                    _configXmlDocument.Load(FullConfigPath);
#else
                    _configXmlDocument.Load(new FileStream(FullConfigPath, FileMode.Open));
#endif
                    // Check data type is equal with this type
                    if (string.IsNullOrEmpty(_configXmlDocument["Configuration"]?[ConfigDataTypeElement]?.InnerText) ||
                        _configXmlDocument["Configuration"]?[ConfigDataTypeElement]?.InnerText !=
                        CreateMd5(ConfigDataType.GetType().FullName ?? ConfigDataType.GetType().Name))
                        throw new Exception(
                            $"A file with this path and name '{FullConfigPath}' exists for another data type. If you need this file for new type of data, please delete it to recreate.");

                    // If config version change
                    // Remake with change value
                    if (string.IsNullOrEmpty(_configXmlDocument["Configuration"]?["ConfigVersion"]?.InnerText) ||
                        _configXmlDocument["Configuration"]?["ConfigVersion"]?.InnerText !=
                        ConfigDataType.ConfigVersion)
                    {
                        LoadConfigByType(false);
                        File.Delete(FullConfigPath);
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
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
            if (objectForParse is PropertyInfo)
                return (objectForParse as PropertyInfo).PropertyType.GetProperties().Where(s =>
                        !Attribute.IsDefined(s, typeof(G9ConfigIgnore)) && s.CanRead && s.CanWrite)
                    .ToArray();
            return objectForParse.GetType().GetProperties()
                .Where(s => !Attribute.IsDefined(s, typeof(G9ConfigIgnore)) && s.CanRead && s.CanWrite &&
                            s.Name != nameof(ConfigDataType.ConfigVersion))
                .ToArray();
#else
            if (objectForParse is PropertyInfo info)
                return info.PropertyType.GetRuntimeProperties().Where(s =>
                        !s.IsDefined(typeof(G9ConfigIgnore)) && s.CanRead && s.CanWrite)
                    .ToArray();
            return objectForParse.GetType().GetRuntimeProperties()
                .Where(s => !s.IsDefined(typeof(G9ConfigIgnore)) && s.CanRead && s.CanWrite &&
                            s.Name != nameof(ConfigDataType.ConfigVersion))
                .ToArray();
#endif
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
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
            _configXmlDocument.Save(FullConfigPath);
#else
            _configXmlDocument.Save(new FileStream(FullConfigPath, FileMode.Create));
#endif
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
                foreach (var t in propertiesInfo)
                    WriteElement(rootNode, t, propertyObject);
            else
                WriteComment("Property with set and get not found!", rootNode);
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
            WriteHintCommentToXml(rootNode, memberPropertyInfo);

            // Add notice required if need
            WriteRequiredNoticeToXml(rootNode, memberPropertyInfo);
            if (CheckTypeIsSupportedByConfigManagement(memberPropertyInfo.PropertyType))
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
                    memberObject.GetType()
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
                        .GetProperty(memberPropertyInfo.Name)
#else
                        .GetRuntimeProperty(memberPropertyInfo.Name)
#endif
                        ?.GetValue(memberObject));
            }
        }

        #endregion

        /// <summary>
        ///     Check property and write comment element to xml if has hint attribute
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>

        #region WriteHintCommentToXml

        private void WriteHintCommentToXml(XmlNode rootNode, PropertyInfo memberPropertyInfo)
        {
            #region Hint Comment

            // Set hint comment for config
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigHint)).ToArray();
            if (hintAttr.Any())
                foreach (var t in hintAttr)
                {
                    G9ConfigHint oHint;
                    if ((oHint = t as G9ConfigHint) == null || string.IsNullOrEmpty(oHint.HintForProperty))
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
            if (memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired)).Any())
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
            foreach (var t in propertiesInfo)
                ReadElement(t, propertyObject, element, checkRequired);
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
            if (CheckTypeIsSupportedByConfigManagement(memberPropertyInfo.PropertyType))
            {
                // Check for required config item
                if (checkRequired && memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired)).Any() &&
                    string.IsNullOrEmpty(element[memberPropertyInfo.Name]?.InnerText))
                    throw new Exception(
                        $"Property {memberPropertyInfo.Name} in config is requirement, but isn't set in the config file. config file path and name:'{FullConfigPath}'");
                // Set config value
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToPropertyType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText));
            }
            else
            {
                ReadXmlByPropertiesInfo(
                    GetPropertiesInfosFromObject(memberPropertyInfo),
                    memberObject.GetType()
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
                        .GetProperty(memberPropertyInfo.Name)
#else
                        .GetRuntimeProperty(memberPropertyInfo.Name)
#endif
                        ?.GetValue(memberObject),
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

        private static object CastStringToPropertyType(PropertyInfo propertyInformation, string value)
        {
            try
            {
                // Parse for enum
                return propertyInformation.PropertyType.GetTypeInfo().BaseType == typeof(Enum)
                    ? Enum.Parse(propertyInformation.PropertyType, value)
                    : propertyInformation.PropertyType.GetTypeInfo().BaseType == typeof(TimeSpan)
                        ? TimeSpan.Parse(value)
                        : Convert.ChangeType(value, propertyInformation.PropertyType);
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

        private static string CreateMd5(string text)
        {
#if NETSTANDARD2_1
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
            return new string(stringBuffer).ToLower();
#else
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(text);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                    sb.Append(t.ToString("X2"));

                return sb.ToString().ToLower();
            }
#endif
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

        /// <summary>
        ///     Check type is supported by config management
        /// </summary>
        /// <param name="typeForCheck"></param>
        /// <returns></returns>

        #region CheckTypeIsSupportedByeConfigManagement

        private bool CheckTypeIsSupportedByConfigManagement(Type typeForCheck)
        {
            // check with supported array type
            return _typesSupportedByConfig.Any(s => s == typeForCheck);
        }

        #endregion

        #endregion
    }
}