namespace DAL.Models
{
    /// <summary>
    /// Minimal header type. Displays name and value of the header.
    /// </summary>
    public class HttpMinimalHeader
    {
        /// <summary>
        /// Name of the header.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the header.
        /// </summary>
        public string Value { get; set; }
    }
}
