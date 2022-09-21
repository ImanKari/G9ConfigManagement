using System;

namespace G9ConfigManagement.Attributes
{
    /// <summary>
    ///     This attribute uses to set a config member as a mandatory member in terms of the value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        // ReSharper disable once RedundantAttributeUsageProperty
        AllowMultiple = false)]
    public class G9AttrRequiredAttribute : Attribute
    {
    }
}