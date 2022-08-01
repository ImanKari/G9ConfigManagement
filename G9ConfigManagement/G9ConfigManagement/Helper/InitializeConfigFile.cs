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
        public InitializeConfigFile(string configFileName, TConfigDataType customConfigObject = null,
            bool forceUpdateWithObject = false, string baseApp = null, string configExtension = "config")
        {
            // Check and set base app
            BaseAppPath =
                string.IsNullOrEmpty(baseApp)
                    ?
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NET35 || NET40 || NET45)
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
                  (configFileName.Length == 1 && configFileName == ".")
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

        /// <summary>
        ///     Get properties info from custom object
        /// </summary>
        /// <param name="objectForParse">Specify object for get property info</param>
        /// <returns></returns>
        private PropertyInfo[] GetPropertiesInfosFromObject(object objectForParse)
        {
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NET35 || NET40 || NET45)
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

        /// <summary>
        ///     Create xml config file by data type
        /// </summary>
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

        /// <summary>
        ///     Read xml and set object from xml
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="propertiesInfo">Specify all property infos from config object</param>
        /// <param name="propertyObject">Config object for get values</param>
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

        /// <summary>
        ///     Write element tag and data to xml
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for read comment value</param>
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
#if (NET35 || NET40)
                var data = memberPropertyInfo.GetValue(memberObject, new object[0]);
                node.InnerText = data != null ? data.ToString() : string.Empty;
#else
                node.InnerText = memberPropertyInfo.GetValue(memberObject)?.ToString() ?? string.Empty;
#endif

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
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NET35 || NET40 || NET45)
                        .GetProperty(memberPropertyInfo.Name)
#else
                        .GetRuntimeProperty(memberPropertyInfo.Name)
#endif
#if (NET35 || NET40)
                        ?.GetValue(memberObject, new object[0]));
#else
                        ?.GetValue(memberObject));
#endif
            }
        }

        /// <summary>
        ///     Check property and write comment element to xml if has hint attribute
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        private void WriteHintCommentToXml(XmlNode rootNode, PropertyInfo memberPropertyInfo)
        {
            // Set hint comment for config
#if (NET35 || NET40)
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigHint), false).ToArray();
#else
            var hintAttr = memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigHint)).ToArray();
#endif

            if (hintAttr.Any())
                foreach (var t in hintAttr)
                {
                    G9ConfigHint oHint;
                    if ((oHint = t as G9ConfigHint) == null || string.IsNullOrEmpty(oHint.HintForProperty))
                        continue;
                    // Write comment
                    WriteComment(oHint.HintForProperty, rootNode);
                }
        }

        /// <summary>
        ///     Check property and write required notice element to xml if has hint attribute
        /// </summary>
        /// <param name="rootNode">Specify root xml node for write</param>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        private void WriteRequiredNoticeToXml(XmlNode rootNode, PropertyInfo memberPropertyInfo)
        {
#if (NET35 || NET40)
            if (memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired), false).Any())
#else
            if (memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired)).Any())
#endif
                // Write comment
                WriteComment(" ### Notice: This element is required! ### ", rootNode);
        }


        /// <summary>
        ///     Write comment to xml
        /// </summary>
        /// <param name="comment">Custom comment message</param>
        /// <param name="rootNode">Specify root xml node for write</param>
        private void WriteComment(string comment, XmlNode rootNode)
        {
            var hintComment = _configXmlDocument.CreateComment(comment);
            rootNode.AppendChild(hintComment);
        }

        /// <summary>
        ///     Set config object by xml data
        /// </summary>
        /// <param name="checkRequired">Check required item</param>
        private void LoadConfigByType(bool checkRequired)
        {
            ReadXmlByPropertiesInfo(
                GetPropertiesInfosFromObject(ConfigDataType),
                ConfigDataType, _configXmlDocument["Configuration"], checkRequired);
        }

        /// <summary>
        ///     Read xml and set object from xml
        /// </summary>
        /// <param name="propertiesInfo">Specify all property infos from config object</param>
        /// <param name="propertyObject">Config object</param>
        /// <param name="element">Element for read</param>
        /// <param name="checkRequired">Check required item</param>
        private void ReadXmlByPropertiesInfo(PropertyInfo[] propertiesInfo, object propertyObject, XmlElement element,
            bool checkRequired)
        {
            // Return if object is null
            if (propertyObject == null || element == null) return;

            // Create elements with properties info
            foreach (var t in propertiesInfo)
                ReadElement(t, propertyObject, element, checkRequired);
        }

        /// <summary>
        ///     Read element from xml and set object
        /// </summary>
        /// <param name="memberPropertyInfo">Specify property information for get information</param>
        /// <param name="memberObject">Object of config for set values from xml</param>
        /// <param name="element">Specify element for read</param>
        /// <param name="checkRequired">Check required item</param>
        private void ReadElement(PropertyInfo memberPropertyInfo, object memberObject, XmlElement element,
            bool checkRequired)
        {
            if (CheckTypeIsSupportedByConfigManagement(memberPropertyInfo.PropertyType))
            {
                // Check for required config item
                if (checkRequired &&
#if (NET35 || NET40)
                    memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired), false)
#else
                    memberPropertyInfo.GetCustomAttributes(typeof(G9ConfigRequired))
#endif
                        .Any() &&
                    string.IsNullOrEmpty(element[memberPropertyInfo.Name]?.InnerText))
                    throw new Exception(
                        $"Property {memberPropertyInfo.Name} in config is requirement, but isn't set in the config file. config file path and name:'{FullConfigPath}'");
                // Set config value
#if (NET35 || NET40)
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToPropertyType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText),
                    new object[0]);
#else
                memberPropertyInfo.SetValue(memberObject,
                    CastStringToPropertyType(memberPropertyInfo, element[memberPropertyInfo.Name]?.InnerText));
#endif
            }
            else
            {
                ReadXmlByPropertiesInfo(
                    GetPropertiesInfosFromObject(memberPropertyInfo),
                    memberObject.GetType()
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NET35 || NET40)
                        .GetProperty(memberPropertyInfo.Name)
#else
                        .GetRuntimeProperty(memberPropertyInfo.Name)
#endif
#if (NET35 || NET40)
                        ?.GetValue(memberObject, new object[0]),
#else
                        ?.GetValue(memberObject),
#endif
                    element[memberPropertyInfo.Name], checkRequired);
            }
        }

        /// <summary>
        ///     Cast string value to property type
        /// </summary>
        /// <param name="propertyInformation">Specify property information for cast</param>
        /// <param name="value">String value for cast to property type</param>
        /// <returns></returns>
        private static object CastStringToPropertyType(PropertyInfo propertyInformation, string value)
        {
            try
            {
                // Parse for enum
#if (NET35 || NET40)
                return propertyInformation.PropertyType?.GetType().BaseType == typeof(Enum)
                    ? Enum.Parse(propertyInformation.PropertyType, value)
                    : propertyInformation.PropertyType?.GetType().BaseType == typeof(TimeSpan)
                        ? TimeSpan.Parse(value)
                        : Convert.ChangeType(value, propertyInformation.PropertyType);
#else
                return propertyInformation.PropertyType.GetTypeInfo().BaseType == typeof(Enum)
                    ? Enum.Parse(propertyInformation.PropertyType, value)
                    : propertyInformation.PropertyType.GetTypeInfo().AsType() == typeof(TimeSpan)
                        ? TimeSpan.Parse(value)
                        : Convert.ChangeType(value, propertyInformation.PropertyType);
#endif
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"Invalid cast exception. property name: '{propertyInformation.Name}'. property value: '{value}'. Type for cast: '{propertyInformation.PropertyType}'.",
                    ex);
            }
        }

        /// <summary>
        ///     Generate MD5 from text
        /// </summary>
        /// <param name="text">Specify text</param>
        /// <returns>Return MD5 from text</returns>
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

        /// <summary>
        ///     Add config version element with comment to xml
        /// </summary>
        /// <param name="rootNode">specify node for write</param>
        private void AddConfigVersionToXml(XmlNode rootNode)
        {
            // Write comment
            WriteComment("Specify config version (automatic set by config management, don't change)", rootNode);
            XmlNode node = _configXmlDocument.CreateElement(nameof(ConfigDataType.ConfigVersion));
            node.InnerText = ConfigDataType.ConfigVersion;
            rootNode.AppendChild(node);
        }

        /// <summary>
        ///     Generate and add data type element with comment to xml
        /// </summary>
        /// <param name="rootNode">specify node for write</param>
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

        /// <summary>
        ///     Check type is supported by config management
        /// </summary>
        /// <param name="typeForCheck"></param>
        /// <returns></returns>
        private bool CheckTypeIsSupportedByConfigManagement(Type typeForCheck)
        {
            // check with supported array type
            return _typesSupportedByConfig.Any(s => s == typeForCheck);
        }

        #endregion
    }
}