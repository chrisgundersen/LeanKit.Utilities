using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Validation.Web.Tests
{
    using System.Collections.Generic;

    using Validation.Web.Controllers;
    using Validation.Web.Models;

    [TestClass]
    public class ValidateControllerTests
    {
        [TestMethod]
        public void GetValidate_ShouldReturnJson()
        {
            var controller = new ValidateController();

            var result = controller.GetValidatorAllowedDefaults();

            Assert.IsNotNull(result);
        }

        //[TestMethod]
        //public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        //{
        //    var testProducts = GetTestProducts();
        //    var controller = new SimpleProductController(testProducts);

        //    var result = await controller.GetAllProductsAsync() as List<Product>;
        //    Assert.AreEqual(testProducts.Count, result.Count);
        //}

        //[TestMethod]
        //public void GetProduct_ShouldReturnCorrectProduct()
        //{
        //    var testProducts = GetTestProducts();
        //    var controller = new SimpleProductController(testProducts);

        //    var result = controller.GetProduct(4) as OkNegotiatedContentResult<Product>;
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(testProducts[3].Name, result.Content.Name);
        //}

        //[TestMethod]
        //public async Task GetProductAsync_ShouldReturnCorrectProduct()
        //{
        //    var testProducts = GetTestProducts();
        //    var controller = new SimpleProductController(testProducts);

        //    var result = await controller.GetProductAsync(4) as OkNegotiatedContentResult<Product>;
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(testProducts[3].Name, result.Content.Name);
        //}

        //[TestMethod]
        //public void GetProduct_ShouldNotFindProduct()
        //{
        //    var controller = new SimpleProductController(GetTestProducts());

        //    var result = controller.GetProduct(999);
        //    Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        //}
    }
}
