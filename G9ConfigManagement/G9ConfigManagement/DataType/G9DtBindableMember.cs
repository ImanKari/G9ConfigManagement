using G9AssemblyManagement;

namespace G9ConfigManagement.DataType
{
    /// <summary>
    ///     A data type for defining a bindable member with the desired value type.
    /// </summary>
    /// <typeparam name="TBindableType">Specifies the desired type for bindable item.</typeparam>
    public class G9DtBindableMember<TBindableType>
    {
        /// <summary>
        ///     Custom delegate for event handler
        /// </summary>
        /// <param name="newValue">Specifies the new value of item</param>
        /// <param name="oldValue">Specifies the old value of item</param>
        public delegate void BindValue(TBindableType newValue, TBindableType oldValue);

        /// <summary>
        ///     Constructor for initializing the basic requirements.
        /// </summary>
        public G9DtBindableMember()
        {
            CurrentValue = default;
        }

        /// <summary>
        ///     Constructor for initializing the basic requirements.
        /// </summary>
        /// <param name="defaultValue">Specifies a default value for the bindable member</param>
        public G9DtBindableMember(TBindableType defaultValue)
        {
            CurrentValue = defaultValue;
        }

        /// <summary>
        ///     Specifies the current value of item
        /// </summary>
        public TBindableType CurrentValue { private set; get; }

        /// <summary>
        ///     Event for reacting to change in the value (Makes the value like a bindable item)
        /// </summary>
        public event BindValue OnChangeValue;

        /// <summary>
        ///     Method for setting a new value to the item.
        ///     <para />
        ///     After setting the value, the event calls all listeners that are assigned in event <see cref="OnChangeValue" />
        /// </summary>
        /// <param name="newValue">Specifies the new value for Item</param>
        public void SetNewValue(TBindableType newValue)
        {
            var oldValue = CurrentValue;
            CurrentValue = newValue;
            OnChangeValue?.Invoke(CurrentValue, oldValue);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return G9Assembly.TypeTools.SmartChangeType<string>(CurrentValue);
        }
    }
}