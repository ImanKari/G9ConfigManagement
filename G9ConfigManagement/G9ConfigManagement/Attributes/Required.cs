using System;

namespace G9ConfigManagement.Attributes
{
    /// <summary>
    ///     This attribute used for set config item to check required
    ///     Required just when read from xml config
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Required : Attribute
    {
    }
}