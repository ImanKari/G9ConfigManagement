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
            ConfigVersion = "1.3.0.0";
            UserName = "ImanKari";
            Password = "G9Studio";
            SubConfig = new SampleSubConfig();
        }

        public string ConfigVersion { set; get; }

        [G9ConfigHint("Set user name for service")]
        public string UserName { set; get; }

        [G9ConfigHint("Set password for service")]
        public string Password { set; get; }

        [G9ConfigRequired]
        [G9ConfigHint("Set type of user")]
        [G9ConfigHint("Values: " + nameof(UserType.Admin) + " - " + nameof(UserType.Editor))]
        [G9ConfigHint("Default values " + nameof(UserType.Admin))]
        public UserType TypeOfUser { set; get; }

        [G9ConfigHint("Set sub class")]
        public SampleSubConfig SubConfig { set; get; }
    }
}
