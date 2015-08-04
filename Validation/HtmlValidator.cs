namespace Validation.Html
{
    using System;
    using System.Collections.Generic;

    using CsQuery.ExtensionMethods.Internal;

    using Ganss.XSS;

    public sealed class HtmlValidator
    {
        private static readonly Lazy<HtmlValidator> Lazy = new Lazy<HtmlValidator>(() => new HtmlValidator());

        public static HtmlValidator Instance => Lazy.Value;

        #region Public Properties for default HtmlValidator whitelists

        /// <summary>
        /// The default list of allowed HTML tags. Any other HTML tags will be stripped.
        /// </summary>
        public ISet<string> DefaultAllowedTags => HtmlSanitizer.DefaultAllowedTags;

        /// <summary>
        /// The default list of allowed URI schemes. Any other URI schemes will be stripped.
        /// </summary>
        public ISet<string> DefaultUrlSchemes => HtmlSanitizer.DefaultAllowedSchemes;

        /// <summary>
        /// The default list of allowed HTML attributes. Any other HTML attributes will be stripped.
        /// </summary>
        public ISet<string> DefaultAttributes => HtmlSanitizer.DefaultAllowedAttributes;

        /// <summary>
        /// The default list of HTML attributes that can contain URIs (like "src" or "href"). All other URIs will be stripped.
        /// </summary>
        public ISet<string> DefaultUrlAttributes => HtmlSanitizer.DefaultUriAttributes;

        /// <summary>
        /// The default list of allowed CSS property names. All other CSS property names will be stripped.
        /// </summary>
        public ISet<string> DefaultAllowedCssProperties => HtmlSanitizer.DefaultAllowedCssProperties;

        #endregion

        private HtmlValidator() {}

        public void IsValidHtml(
            string html,
            out string sanitizedHtml,
            out bool wasModified,
            ISet<string> allowedTags = null,
            ISet<string> allowedUrlSchemes = null,
            ISet<string> allowedAttributes = null,
            ISet<string> allowedUriAttributes = null,
            ISet<string> allowedCssProperties = null,
            bool appendAllowedToDefaults = true)
        {
            if (appendAllowedToDefaults)
            {
                allowedTags?.AddRange(this.DefaultAllowedTags);
                allowedUrlSchemes?.AddRange(this.DefaultUrlSchemes);
                allowedAttributes?.AddRange(this.DefaultAttributes);
                allowedUriAttributes?.AddRange(this.DefaultUrlAttributes);
                allowedCssProperties?.AddRange(this.DefaultAllowedCssProperties);
            }

            var sanitizer = new HtmlSanitizer(
                allowedTags,
                allowedUrlSchemes,
                allowedAttributes,
                allowedUriAttributes,
                allowedCssProperties);

            sanitizedHtml = sanitizer.Sanitize(html);

            wasModified = html != sanitizedHtml;
        }
    }
}
