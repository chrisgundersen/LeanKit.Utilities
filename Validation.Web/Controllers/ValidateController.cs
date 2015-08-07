namespace Validation.Web.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Web.Http.Results;

    using NLog;

    using Html;
    using Validation.Web.Models;

    using static System.String;

    public class ValidateController : ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        public IHttpActionResult GetValidatorAllowedDefaults()
        {
            try
            {
                Logger.Info("Begin GetValidatorAllowedDefaults()");

                var content = new HtmlValidationAllowedMetadata()
                                  {
                                      AllowedHtmlAttributes =
                                          HtmlValidator.Instance.DefaultAttributes,
                                      AllowedUrlSchemes =
                                          HtmlValidator.Instance.DefaultUrlSchemes,
                                      DefaultAllowedCssProperties =
                                          HtmlValidator.Instance
                                          .DefaultAllowedCssProperties,
                                      DefaultAllowedTags =
                                          HtmlValidator.Instance.DefaultAllowedTags,
                                      DefaultUrlAttributes =
                                          HtmlValidator.Instance.DefaultUrlAttributes
                                  };

                return Ok(content);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in GetValidatorAllowedDefaults()");

                return new ExceptionResult(ex, this);
            }
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
        /// <response code="201">The value provided was not valid, and the new, valid version will be returned in the response content</response>
        /// <response code="204">No value was provided, so the response was intentionally left blank</response>
        /// <response code="413">Value contained too many characters</response>
        /// <returns>Sanitized HTML as text/plain</returns>
        [HttpPost]
        [Route("validate/html")]
        public IHttpActionResult FixHtml([FromBody]string value)
        {
            try
            {
                if (IsNullOrEmpty(value)) return new StatusCodeResult(HttpStatusCode.NoContent, this);

                int maxLength;

                if (!int.TryParse(
                    WebConfigurationManager.AppSettings.Get("MaxHtmlValidationBodyPayloadLength"),
                    out maxLength))
                {
                    maxLength = 40000;
                }

                if (value.Length > maxLength) return new StatusCodeResult(HttpStatusCode.RequestEntityTooLarge, this);

                string sanitized;
                bool wasModified;

                HtmlValidator.Instance.IsValidHtml(value, out sanitized, out wasModified);

                if (!wasModified) return new OkResult(this);

                var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(sanitized, Encoding.UTF8, @"text/plain") };

                return new ResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in FixHtml");

                return new InternalServerErrorResult(this);
            }
            
        }
    }
}
