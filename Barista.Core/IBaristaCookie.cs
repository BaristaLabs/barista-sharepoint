namespace Barista
{
    using Barista.Newtonsoft.Json;
    using System;

    public interface IBaristaCookie
    {
        /// <summary>
        /// The domain to restrict the cookie to
        /// </summary>
        [JsonProperty("domain")]
        string Domain
        {
            get;
            set;
        }

        /// <summary>
        /// When the cookie should expire
        /// </summary>
        /// <value>A <see cref="DateTime"/> instance containing the date and time when the cookie should expire; otherwise <see langword="null"/> if it should expire at the end of the session.</value>
        [JsonProperty("expires")]
        DateTime? Expires
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the cookie
        /// </summary>
        [JsonProperty("name")]
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the encoded name of the cookie
        /// </summary>
        [JsonProperty("encodedName")]
        string EncodedName
        {
            get;
        }

        /// <summary>
        /// The path to restrict the cookie to
        /// </summary>
        [JsonProperty("path")]
        string Path
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the cookie
        /// </summary>
        [JsonProperty("value")]
        string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the encoded value of the cookie
        /// </summary>
        [JsonProperty("encodedValue")]
        string EncodedValue
        {
            get;
        }

        /// <summary>
        /// Whether the cookie is http only
        /// </summary>
        [JsonProperty("httpOnly")]
        bool HttpOnly
        {
            get;
            set; 
        }

        /// <summary>
        /// Whether the cookie is secure (i.e. HTTPS only)
        /// </summary>
        [JsonProperty("secure")]
        bool Secure
        {
            get;
            set;
        }
    }
}
