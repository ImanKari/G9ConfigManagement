using System;
using System.Collections.Generic;
using G9ConfigManagement;
using G9ConfigManagement.JSON;
using G9ConfigManagementNUnitTest.Sample;
using NUnit.Framework;

namespace G9ConfigManagementNUnitTest
{

    public class Test
    {
        public string FirstName { set; get; } = "Iman";
        public string LastName { set; get; } = "Kari";
        public TimeSpan Time { set; get; } = TimeSpan.MaxValue;
        public DateTime DateTime { set; get; } = DateTime.MaxValue;
        public int Age { set; get; } = 32;
        public List<string> Names { set; get; } = new List<string>() { "A1", "B2", "C3" };
    }

    public class Test2
    {
        public string FirstName { set; get; } = "Iman";
        public string LastName { set; get; } = "Kari";
        public TimeSpan Time { set; get; } = TimeSpan.MaxValue;
        public DateTime DateTime { set; get; } = DateTime.MaxValue;

        public Test InnerObject { set; get; } = new Test();
    }

    /// <summary>
    /// Test Class
    /// </summary>
    public class G9ConfigManagementUnitTest
    {
        public const string ConfigFileName = "Configuration";

        public const string ConfigExtension = "setting";

        public G9ConfigManagementSingleton<SampleConfig> Configuration;

        

        [SetUp]
        public void Setup()
        {
            var x = TimeSpan.MaxValue.ToString();
            var oClass = new Test2();
            var data = JSONWriter.ToJson(oClass);
            var oObject = JSONParser.FromJson<Test>(data);
        }

        [Test]
        [Order(1)]
        public void InitializeConfigWithoutSetName()
        {
            // Initialize config file
            Configuration = G9ConfigManagementSingleton<SampleConfig>.GetInstance();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, nameof(SampleConfig));

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(2)]
        public void InitializeConfigWithSetCustomName()
        {
            // Initialize config file
            Configuration =
                G9ConfigManagementSingleton<SampleConfig>.GetInstance(ConfigFileName,
                    configExtension: ConfigExtension);

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, ConfigFileName);

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(3)]
        public void RestoreConfig()
        {
            // Initialize config file
            Configuration =
                G9ConfigManagementSingleton<SampleConfig>.GetInstance(ConfigFileName,
                    configExtension: ConfigExtension);

            // Initialize config file
            var config2 = G9ConfigManagementSingleton<SampleConfig>.GetInstance();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, ConfigFileName);
            // Check config file name
            Assert.AreEqual(config2.ConfigFileName, nameof(SampleConfig));

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, sampleConfig.ConfigVersion);
            // Check version
            Assert.AreEqual(config2.Configuration.ConfigVersion, sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(4)]
        public void ForceUpdate()
        {
            Configuration.ForceUpdate(new SampleConfig()
            {
                SubConfig = new SampleSubConfig()
                {
                    SampleSubTwo = new SampleSubSubConfig()
                    {
                        SaveTime = 99,
                        StartDateTime = DateTime.Now
                    }
                }
            });
        }

        [Test]
        [Order(5)]
        public void AddCustomConfig()
        {
            // Initialize config file
            Configuration =
                G9ConfigManagementSingleton<SampleConfig>.GetInstance(ConfigFileName, new SampleConfig()
                {
                    UserName = "TEST"
                }, false, null, "ini");

            Assert.AreEqual(Configuration.Configuration.UserName, "TEST");
        }

        [Test]
        [Order(6)]
        public void RestoreCustomConfig()
        {
            // Initialize config file
            Configuration =
                G9ConfigManagementSingleton<SampleConfig>.GetInstance(ConfigFileName, new SampleConfig()
                {
                    UserName = "TEST"
                }, false, null, "ini");

            Assert.AreEqual(Configuration.Configuration.UserName, "TEST");
        }
    }
}