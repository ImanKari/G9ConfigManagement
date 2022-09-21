namespace G9ConfigManagement.Enum
{
    /// <summary>
    ///     Enum for specifying the reaction when in a config structure a change version happens.
    /// </summary>
    public enum G9EChangeVersionReaction
    {
        /// <summary>
        ///     If a change version happens in the config structure, the old config file data merges with the new config structure
        ///     as much as possible, and then the new file is recreated.
        ///     <para />
        ///     In this case, all config members in the old structure with the same name and data type merge with the new
        ///     structure, so they don't lose their values unless their name or data type has changed.
        /// </summary>
        MergeThenOverwrite,

        /// <summary>
        ///     If a change version happens in the config structure, the old config file overwrites by the new structure.
        /// </summary>
        ForceOverwrite
    }
}