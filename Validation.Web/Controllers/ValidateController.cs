﻿namespace Validation.Web.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Http;

    using NLog.Internal;

    using Validation.Html;

    using static System.String;

    public class ValidateController : ApiController
    {
        /// <summary>
        /// Gets the allowed HTML attributes, URL schemes, CSS properties, tags, and URL attributes
        /// </summary>
        /// <remarks>
        /// Anything not included in these whitelists will be removed in the response from this service.
        /// Note also that anything that the validation can't optimize or remove will be HTML encoded.
        /// </remarks>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("validate/html")]
        public IHttpActionResult Get()
        {
            var content =
                new
                    {
                        allowedHtmlAttributes = HtmlValidator.Instance.DefaultAttributes,
                        allowedUrlSchemes = HtmlValidator.Instance.DefaultUrlSchemes,
                        defaultAllowedCssProperties = HtmlValidator.Instance.DefaultAllowedCssProperties,
                        defaultAllowedTags = HtmlValidator.Instance.DefaultAllowedTags,
                        defaultUrlAttributes = HtmlValidator.Instance.DefaultUrlAttributes
                    };

            return Json(content);
        }

        // POST: api/Validate
        /// <summary>
        /// Takes a value containing HTML for parsing and sanitization
        /// </summary>
        /// <remarks>
        /// Sanitization will be performed on any non-null and non-empty value provided. If no modifications are
        /// necessary, then the original value can be used (and is not returned in the response).
        /// </remarks>
        /// <param name="value"></param>
        /// <response code="200">The value provided did not need to be modified or sanitized</response>
        /// <response code="204">No value was provided, so the response was intentionally left blank</response>
        /// <response code="413">Value contained too many characters</response>
        /// <returns>Sanitized HTML as text/plain</returns>
        [HttpPost]
        [Route("validate/html")]
        public HttpResponseMessage Post([FromBody]string value)
        {
            if (IsNullOrEmpty(value)) return new HttpResponseMessage(HttpStatusCode.NoContent);

            int maxLength;

            if (!int.TryParse(
                WebConfigurationManager.AppSettings.Get("MaxHtmlValidationBodyPayloadLength"),
                out maxLength))
            {
                maxLength = 40000;
            }

            if (value.Length > maxLength) return new HttpResponseMessage(HttpStatusCode.RequestEntityTooLarge);

            string sanitized;
            bool wasModified;

            HtmlValidator.Instance.IsValidHtml(value, out sanitized, out wasModified);

            if (!wasModified) return new HttpResponseMessage(HttpStatusCode.OK);

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(sanitized, Encoding.UTF8, @"text/plain") };

            return response;
        }
    }
}
