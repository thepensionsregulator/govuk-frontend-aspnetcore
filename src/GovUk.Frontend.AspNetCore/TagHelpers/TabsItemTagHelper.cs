#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GovUk.Frontend.AspNetCore.HtmlGeneration;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GovUk.Frontend.AspNetCore.TagHelpers
{
    /// <summary>
    /// Represents an item in a GDS tabs component.
    /// </summary>
    [HtmlTargetElement(TagName, ParentTag = TabsTagHelper.TagName)]
    [OutputElementHint(ComponentGenerator.TabsItemPanelElement)]
    public class TabsItemTagHelper : TagHelper
    {
        internal const string TagName = "govuk-tabs-item";
        internal const string IdAttributeName = "id";

        private const string LabelAttributeName = "label";
        private const string LinkAttributesPrefix = "link-";

        private IDictionary<string, string>? _linkAttributes;

        /// <summary>
        /// The <c>id</c> attribute for the item.
        /// </summary>
        /// <remarks>
        /// Requires unless <see cref="TabsTagHelper.IdPrefix"/> is specified on the parent.
        /// </remarks>
        [HtmlAttributeName(IdAttributeName)]
        public string? Id { get; set; }

        /// <summary>
        /// The text label of the tab item.
        /// </summary>
        [HtmlAttributeName(LabelAttributeName)]
        [DisallowNull]
        public string? Label { get; set; }

        /// <summary>
        /// Additional attribute to add to the tab.
        /// </summary>
        [HtmlAttributeName(DictionaryAttributePrefix = LinkAttributesPrefix)]
        public IDictionary<string, string> LinkAttributes
        {
            get => _linkAttributes ??= new Dictionary<string, string>();
            set => _linkAttributes = Guard.ArgumentNotNull(nameof(value), value);
        }

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Label == null)
            {
                throw ExceptionHelper.TheAttributeMustBeSpecified(LabelAttributeName);
            }

            var tabsContext = context.GetContextItem<TabsContext>();

            var content = await output.GetChildContentAsync();

            tabsContext.AddItem(new TabsItem()
            {
                Id = Id,
                Label = Label,
                LinkAttributes = _linkAttributes.ToAttributeDictionary(),
                PanelAttributes = output.Attributes.ToAttributeDictionary(),
                PanelContent = content.Snapshot()
            });

            output.SuppressOutput();
        }
    }
}
