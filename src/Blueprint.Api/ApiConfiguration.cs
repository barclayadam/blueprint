namespace Blueprint.Api
{
    /// <summary>
    /// Configuration object for Blueprint API, will be a singleton throughout the site.
    /// </summary>
    public class ApiConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiConfiguration" /> class with empty
        /// data.
        /// </summary>
        public ApiConfiguration()
        {
            BaseApiUrl = string.Empty;
        }

        /// <summary>
        /// Gets or sets the Base URL for the API, the fully-qualified URL that represents where the
        /// root of the API can be accessed from (i.e. https://api.somewhere.com/api/).
        /// </summary>
        public string BaseApiUrl { get; set; }
    }
}
