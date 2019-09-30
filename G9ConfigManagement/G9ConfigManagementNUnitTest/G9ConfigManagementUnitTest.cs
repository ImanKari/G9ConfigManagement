using G9ConfigManagement;
using G9ConfigManagementNUnitTest.Sample;
using NUnit.Framework;

namespace G9ConfigManagementNUnitTest
{
    public class G9ConfigManagementUnitTest
    {
        public const string ConfigFileName = "Configuration.Config";

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
            var oConfig = new SampleConfig();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, typeof(SampleConfig).Name);

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, oConfig.ConfigVersion);
        }

        [Test]
        [Order(2)]
        public void InitializeConfigWithSetCustomName()
        {
            // Initialize config file
            Configuration = G9ConfigManagement_Singleton<SampleConfig>.GetInstance(ConfigFileName);

            // Initialize config
            var oConfig = new SampleConfig();

            // Check config file name
            Assert.AreEqual(Configuration.ConfigFileName, ConfigFileName);

            // Check version
            Assert.AreEqual(Configuration.Configuration.ConfigVersion, oConfig.ConfigVersion);
        }
    }
}