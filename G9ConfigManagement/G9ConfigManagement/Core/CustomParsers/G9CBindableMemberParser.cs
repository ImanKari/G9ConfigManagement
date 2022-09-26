using System;
using System.Linq;
using G9AssemblyManagement;
using G9AssemblyManagement.Enums;
using G9AssemblyManagement.Interfaces;
using G9ConfigManagement.DataType;
using G9JSONHandler.Abstract;

namespace G9ConfigManagement.Core.CustomParsers
{
    /// <summary>
    ///     Custom parser structure for Bindable member
    /// </summary>
    public class G9CBindableMemberParser : G9ACustomGenericTypeParser
    {
        /// <inheritdoc />
        public G9CBindableMemberParser()
            : base(typeof(G9DtBindableMember<>))
        {
        }

        /// <inheritdoc />
        public override string ObjectToString(object objectForParsing, Type[] genericTypes,
            G9IMemberGetter accessToObjectMember, Action<string> addCustomComment)
        {
            var property = G9Assembly.ReflectionTools.GetPropertiesOfObject(objectForParsing,
                G9EAccessModifier.Public, p => p.Name == nameof(G9DtBindableMember<object>.CurrentValue)).First();

            return G9Assembly.TypeTools.SmartChangeType<string>(property.GetValue());
        }

        /// <inheritdoc />
        public override object StringToObject(string stringForParsing, Type[] genericTypes,
            G9IMemberGetter accessToObjectMember)
        {
            // Create new instance of bindable object
            var instance = G9Assembly.InstanceTools.CreateInstanceFromGenericType(
                typeof(G9DtBindableMember<>), genericTypes);

            // Access to the property 'CurrentValue' and set the value of that
            var property = G9Assembly.ReflectionTools.GetPropertiesOfObject(instance,
                G9EAccessModifier.Public, p => p.Name == nameof(G9DtBindableMember<object>.CurrentValue)).First();
            property.SetValue(G9Assembly.TypeTools.SmartChangeType(stringForParsing, genericTypes[0]));

            // Return instance as object
            return instance;
        }
    }
}