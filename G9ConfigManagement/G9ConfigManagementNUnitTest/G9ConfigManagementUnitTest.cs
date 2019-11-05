using System;
using G9ConfigManagement;
using G9ConfigManagementNUnitTest.Sample;
using NUnit.Framework;

namespace G9ConfigManagementNUnitTest
{
    public class G9ConfigManagementUnitTest
    {
        public const string ConfigFileName = "Configuration";

        public const string ConfigExtension = "setting";

        public G9ConfigManagement_Singleton<SampleConfig> Configuration;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Order(1)]
        public void InitializeConfigWithoutSetName()
        {
            // Initialize config file
            Configuration = G9ConfigManagement_Singleton<SampleConfig>.GetInstance();

            // Initialize config
            var sampleConfig = new SampleConfig();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, typeof(SampleConfig).Name);

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, sampleConfig.ConfigVersion);
        }

        [Test]
        [Order(2)]
        public void InitializeConfigWithSetCustomName()
        {
            // Initialize config file
            Configuration =
                G9ConfigManagement_Singleton<SampleConfig>.GetInstance(ConfigFileName,
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
                G9ConfigManagement_Singleton<SampleConfig>.GetInstance(ConfigFileName,
                    configExtension: ConfigExtension);
            // Initialize config file
            var config2 = G9ConfigManagement_Singleton<SampleConfig>.GetInstance();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, ConfigFileName);
            // Check config file name
            Assert.AreEqual(config2.ConfigFileName, typeof(SampleConfig).Name);

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
    }
}