namespace Validation.Web.Tests
{
    using System.IO;
    using System.Net;
    using System.Web.Http.Results;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Validation.Web.Controllers;

    [TestClass]
    public class ValidateControllerTests
    {
        private readonly ValidateController controller = new ValidateController();

        [TestMethod]
        public void GetValidate_ShouldReturnJson()
        {
            var response = this.controller.GetValidatorAllowedDefaults();

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void GetValidatedHtml_ShouldReturn204WhenNoValueProvided()
        {
            var response = this.controller.FixHtml(string.Empty) as StatusCodeResult;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.NoContent);
        }

        [TestMethod]
        public void GetValidatedHtml_ShouldReturn200WhenOk()
        {
            var testValue = @"<p></p>";

            var response = this.controller.FixHtml(testValue) as OkResult;

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void GetValidatedHtml_ShouldReturn201WhenModified()
        {
            var testValue = @"<div>Test</div><SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>";
            var expected = @"<div>Test</div>";

            var responseMessage = this.controller.FixHtml(testValue) as ResponseMessageResult;

            Assert.IsNotNull(responseMessage);
            Assert.AreEqual(responseMessage.Response.StatusCode, HttpStatusCode.Created);

            var responseContent = responseMessage.Response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(responseContent, expected);
        }

        [TestMethod]
        public void GetValidatedHtml_ShouldReturn413WhenValueTooLarge()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var testValue = File.ReadAllText($@"{currentDirectory}\{@"40k1.txt"}");

            var response = this.controller.FixHtml(testValue) as StatusCodeResult;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.RequestEntityTooLarge);
        }
    }
}
