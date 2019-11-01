using System;

namespace G9ConfigManagement.Attributes
{
    /// <summary>
    ///     This attribute used for ignore item in config (ignore for read and write config item to xml file)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Ignore : Attribute
    {
    }
}