using System;
using G9ConfigManagement.Abstract;
using G9ConfigManagement.Attributes;
using G9ConfigManagement.DataType;
using G9JSONHandler.Attributes;

public class SampleConfig : G9AConfigStructure<SampleConfig>
{

    // The desired config structure must be derived from the abstract class 'G9AConfigStructure' and would pass to the derived abstract class as a generic type.
    [G9AttrComment("My custom comment.")]
    public string ApplicationName { set; get; } = "My Custom Application";

    [G9AttrIgnore]
    public string User = "MyUser";

    [G9AttrRequired]
    [G9AttrEncryption("G9-KaPSgV9Yp6s3v", "H@McQfTjWnZr4u7x")]
    public string ConnectionString =
        "Server=myServerName\\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;";

    [G9AttrCustomName("MyCustomBindableMember")]
    public G9DtBindableMember<string> CustomBindableMember 
        = new G9DtBindableMember<string>("My bindable value");

    [G9AttrStoreEnumAsString]
    public ConsoleColor Color = ConsoleColor.DarkMagenta;

    /// <inheritdoc />
    public override G9DtConfigVersion ConfigVersion { set; get; } = new(1, 0, 0, 0);
}
