using System;
using System.Collections.Generic;
using System.Text;

namespace G9ConfigManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class Ignore : Attribute
    {
    }
}
