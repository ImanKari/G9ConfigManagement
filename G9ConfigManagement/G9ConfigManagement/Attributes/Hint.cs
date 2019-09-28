using System;
using System.Collections.Generic;
using System.Text;

namespace G9ConfigManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class Hint : Attribute
    {

        public string HintForProperty { get; }

        public Hint(string customHint)
        {
            HintForProperty = customHint;
        }
    }
}
