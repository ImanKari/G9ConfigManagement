using System;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.Interface;

namespace G9ConfigManagementNUnitTest.Sample
{
    [Serializable]
    public class SampleConfig : IConfigDataType
    {

        public enum UserType : byte
        {
            Admin,
            Editor
        }

        public SampleConfig()
        {
            ConfigVersion = "1.0.0.0";
            UserName = "ImanKari";
            Password = "G9Studio";
            SubConfig = new SampleSubConfig();
        }

        public string ConfigVersion { set; get; }

        [Hint("Set user name for service")]
        public string UserName { set; get; }

        [Hint("Set password for service")]
        public string Password { set; get; }

        [Hint("Set type of user")]
        [Hint("Values: " + nameof(UserType.Admin) + " - " + nameof(UserType.Editor))]
        [Hint("Default values " + nameof(UserType.Admin))]
        public UserType TypeOfUser { set; get; }

        [Hint("Set sub class")]
        public SampleSubConfig SubConfig { set; get; }
    }
}
