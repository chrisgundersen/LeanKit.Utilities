namespace Validation.Web.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class HtmlValidationAllowedMetadata
    {
        [JsonProperty("allowedHtmlAttributes")]
        public ISet<string> AllowedHtmlAttributes { get; set; }

        [JsonProperty("allowedUrlSchemes")]
        public ISet<string> AllowedUrlSchemes { get; set; }

        [JsonProperty("defaultAllowedCssProperties")]
        public ISet<string> DefaultAllowedCssProperties { get; set; }

        [JsonProperty("defaultAllowedTags")]
        public ISet<string> DefaultAllowedTags { get; set; }

        [JsonProperty("defaultUrlAttributes")]
        public ISet<string> DefaultUrlAttributes { get; set; }
    }
}
