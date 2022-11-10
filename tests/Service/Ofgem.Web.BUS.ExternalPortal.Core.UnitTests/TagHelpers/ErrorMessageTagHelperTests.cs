using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Ofgem.Web.BUS.ExternalPortal.Core.TagHelpers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ofgem.Web.BUS.ExternalPortal.Core.UnitTests.TagHelpers;

/// <remarks>
/// See https://github.com/aspnet/Mvc/blob/release/2.2/test/Microsoft.AspNetCore.Mvc.TagHelpers.Test/ValidationMessageTagHelperTest.cs
/// </remarks>
[TestFixture]
public class ErrorMessageTagHelperTests
{
    [Test]
    public void Process_Creates_Output_When_ModelState_Invalid()
    {
        // Arrange
        var errorFieldName = "BrokenField";
        var errorMessage = "Something broke";
        var expectedTag = "p";

        var modelExpression = CreateModelExpression(errorFieldName);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(errorFieldName, errorMessage);

        var viewContext = CreateViewContext(modelState);

        var systemUnderTest = new ErrorMessageTagHelper
        {
            For = modelExpression,
            ViewContext = viewContext
        };

        var tagHelperContext = new TagHelperContext(expectedTag, new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");
        var output = new TagHelperOutput(expectedTag, new TagHelperAttributeList(), (cachedResult, encoder) =>
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetContent("Something");
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        });

        // Act
        systemUnderTest.Process(tagHelperContext, output);

        // Assert
        using (new AssertionScope())
        {
            output.TagName.Should().Be(expectedTag);
            output.PreContent.GetContent().Should().NotBeNullOrEmpty().And.StartWith("<span");
            output.Content.GetContent().Should().NotBeNullOrEmpty().And.Be(errorMessage);
        }
    }

    [Test]
    public void Process_Creates_Empty_Output_When_ModelState_Valid()
    {
        // Arrange
        var errorFieldName = "BrokenField";
        var expectedTag = "p";

        var modelExpression = CreateModelExpression(errorFieldName);
        var modelState = new ModelStateDictionary();

        var viewContext = CreateViewContext(modelState);

        var systemUnderTest = new ErrorMessageTagHelper
        {
            For = modelExpression,
            ViewContext = viewContext
        };

        var tagHelperContext = new TagHelperContext(expectedTag, new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");
        var output = new TagHelperOutput(expectedTag, new TagHelperAttributeList(), (cachedResult, encoder) =>
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetContent("Something");
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        });

        // Act
        systemUnderTest.Process(tagHelperContext, output);

        // Assert
        using (new AssertionScope())
        {
            output.TagName.Should().BeNullOrEmpty();
            output.PreContent.GetContent().Should().BeNullOrEmpty();
            output.Content.GetContent().Should().BeNullOrEmpty();
        }
    }

    private ModelExpression CreateModelExpression(string name)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        return new ModelExpression(name, metadataProvider.GetModelExplorerForType(typeof(object), null));
    }

    private ViewContext CreateViewContext(ModelStateDictionary modelState)
    {
        var actionContext = new ActionContext(new DefaultHttpContext(),
                                              new RouteData(),
                                              new PageActionDescriptor(),
                                              modelState);

        var viewContext = new ViewContext(actionContext,
                                          Mock.Of<IView>(),
                                          new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState),
                                          Mock.Of<ITempDataDictionary>(),
                                          TextWriter.Null,
                                          new HtmlHelperOptions());

        return viewContext;
    }
}
