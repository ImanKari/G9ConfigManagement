using System;
using G9ConfigManagement.Abstract;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;
using G9JSONHandler.Attributes;

namespace G9ConfigManagementNUnitTest.Sample
{
    public enum UserType : byte
    {
        Admin,
        Editor
    }

    public class SampleConfig : G9AConfigStructure<SampleConfig>
    {
        public SampleConfig()
        {
            UserName = "ImanKari";
            Password = "G9Studio";
            SubConfig = new SampleSubConfig();
            TypeOfUser = UserType.Editor;
        }


        public string UserName { set; get; }

        public TimeSpan TestTimeSpan { set; get; } = TimeSpan.FromSeconds(3);

        [G9AttrComment("Specifies an encrypted password (Algorithm: PKCS7/CBC,128).")]
        [G9AttrEncryption("G9-KaPSgV9Yp6s3v", "H@McQfTjWnZr4u7x")]
        public string Password { set; get; }

        [G9AttrRequired]
        [G9AttrComment("Specifies the type of user (multi-line comments)")]
        [G9AttrComment("Values: (" + nameof(UserType.Admin) + "-" + nameof(UserType.Editor) + ")")]
        [G9AttrComment("Default value: " + nameof(UserType.Admin))]
        public UserType TypeOfUser { set; get; }

        [G9AttrComment("Specifies a nested config in the main config structure.")]
        public SampleSubConfig SubConfig { set; get; }

        public override G9DtConfigVersion ConfigVersion { set; get; } = new G9DtConfigVersion(9, 6, 3, 1);
    }
}