namespace UNLowCoder.Core
{
    public enum ParseMode
    {
        /// <summary>
        /// Parse all entries, including duplicates for different changes
        /// </summary>
        AllEntries,

        /// <summary>
        /// Parse only the newest entry for each location and ensures no duplicates
        /// </summary>
        OnlyNewest,
    }
}
