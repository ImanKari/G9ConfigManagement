using System.Diagnostics;
using G9ConfigManagement;
using G9ConfigManagementNUnitTest.Sample;
using NUnit.Framework;

namespace G9ConfigManagementNUnitTest
{
    public class G9ConfigManagementUnitTest
    {

        public G9ConfigManagement_Singleton<SampleConfig> Configuration;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void InitializeConfig()
        {
            Configuration = G9ConfigManagement_Singleton<SampleConfig>.GetInstance("Configuration.Config");

            Debug.WriteLine(Configuration.ConfigFileName);

            Debug.WriteLine(Configuration.Configuration.ConfigVersion);
            Debug.WriteLine(Configuration.Configuration.UserName);
            Debug.WriteLine(Configuration.Configuration.Password);
        }
    }
}