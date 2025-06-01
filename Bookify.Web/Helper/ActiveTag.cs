using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bookify.Web.Helper
{
    [HtmlTargetElement("a",Attributes ="active-when")]
    public class ActiveTag :TagHelper
    {
        //The value that will be given to the attribute
        public string? ActiveWhen { get; set; }   // this name must be the same name as the attribute above but in Pascal Case.

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(ActiveWhen))
                return;
            var controller = ViewContextData?.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            if(controller!.Equals(ActiveWhen))
            {
                if (output.Attributes.ContainsName("class"))
                    output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} active");
                else
                    output.Attributes.SetAttribute("class", "active");
            }
        }
    }
}
