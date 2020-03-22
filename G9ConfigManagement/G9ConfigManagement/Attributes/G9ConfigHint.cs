using System;

namespace G9ConfigManagement.Attributes
{
    /// <summary>
    ///     This attribute used for added hint comment for config item
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class G9ConfigHint : Attribute
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="customHint">Custom hint (comment for xml config item)</param>
        public G9ConfigHint(string customHint)
        {
            HintForProperty = customHint;
        }

        /// <summary>
        ///     Save hint
        /// </summary>
        public string HintForProperty { get; }
    }
}