using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

namespace Ofgem.Web.BUS.ExternalPortal.Core.TagHelpers;

/// <summary>
/// Creates an error message for form inputs based on GDS styles.
/// </summary>
/// <remarks>
/// See https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.TagHelpers/src/ValidationMessageTagHelper.cs
/// </remarks>
[HtmlTargetElement("p", Attributes = "gds-validation-for")]
public class ErrorMessageTagHelper : TagHelper
{
    /// <summary>
    /// A model expression identifying the input to validate.
    /// </summary>
    [HtmlAttributeName("gds-validation-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// The view context.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (For != null)
        {
            var fieldName = For.Name;
            if (ViewContext!.ModelState.HasError(fieldName))
            {
                var modelErrorsResult = ViewContext.ModelState.TryGetValue(fieldName, out var modelEntry);
                var modelError = modelErrorsResult ? modelEntry?.Errors.First() : null;

                if (modelError != null)
                {
                    output.PreContent.AppendHtml("<span class=\"govuk-visually-hidden\">Error:</span>");
                    output.Content.SetContent(modelError.ErrorMessage);
                }
            }
            else
            {
                output.SuppressOutput();
            }
        }
    }
}
