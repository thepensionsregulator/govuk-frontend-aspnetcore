#nullable enable
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GovUk.Frontend.AspNetCore.HtmlGeneration
{
    public class TabsItem
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public AttributeDictionary? LinkAttributes { get; set; }
        public AttributeDictionary? PanelAttributes { get; set; }
        public IHtmlContent? PanelContent { get; set; }
    }
}
