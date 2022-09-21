using System;
using G9AssemblyManagement.Interfaces;
using G9ConfigManagement.DataType;
using G9JSONHandler.Abstract;

namespace G9ConfigManagement.Core.CustomParsers
{
    /// <summary>
    ///     A custom parser for type 'G9DtConfigVersion'
    /// </summary>
    public class G9CConfigVersionParser : G9ACustomTypeParser<G9DtConfigVersion>
    {
        /// <inheritdoc />
        public override string ObjectToString(G9DtConfigVersion objectForParsing, G9IMemberGetter accessToObjectMember, Action<string> addCustomComment)
        {
            addCustomComment("It's specified and used by the core.");
            addCustomComment("Please don't change it manually!");
            return objectForParsing.ToString();
        }

        /// <inheritdoc />
        public override G9DtConfigVersion StringToObject(string stringForParsing, G9IMemberGetter accessToObjectMember)
        {
            return G9DtConfigVersion.Parse(stringForParsing);
        }
    }
}