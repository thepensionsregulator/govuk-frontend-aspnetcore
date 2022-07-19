using System.Linq;
using GovUk.Frontend.AspNetCore.HtmlGeneration;
using GovUk.Frontend.AspNetCore.TestCommon;
using Xunit;

namespace GovUk.Frontend.AspNetCore.ConformanceTests
{
    public partial class ComponentTests
    {
        [Theory]
        [ComponentFixtureData(
            "tabs",
            typeof(OptionsJson.Tabs),
            exclude: new[] { "with falsey values", "empty item list", "no item list" })]
        public void Tabs(ComponentTestCaseData<OptionsJson.Tabs> data) =>
            CheckComponentHtmlMatchesExpectedHtml(
                data,
                (generator, options) =>
                {
                    var title = options.Title ?? ComponentGenerator.TabsDefaultTitle;

                    var attributes = options.Attributes.ToAttributesDictionary()
                        .MergeAttribute("class", options.Classes);

                    var items = (options.Items?.Select(i => new TabsItem()
                        {
                            Id = i.Id,
                            Label = i.Label,
                            LinkAttributes = i.Attributes.ToAttributesDictionary(),
                            PanelAttributes = i.Panel.Attributes.ToAttributesDictionary(),
                            PanelContent = TextOrHtmlHelper.GetHtmlContent(i.Panel.Text, i.Panel.Html)
                        }))
                        .OrEmpty();

                    return generator.GenerateTabs(options.Id, options.IdPrefix, title, attributes, items)
                        .RenderToString();
                });
    }
}
