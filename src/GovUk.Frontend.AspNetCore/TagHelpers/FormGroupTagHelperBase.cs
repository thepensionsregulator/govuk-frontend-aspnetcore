﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GovUk.Frontend.AspNetCore.TagHelpers
{
    public abstract class FormGroupTagHelperBase : TagHelper
    {
        protected const string AspForAttributeName = "asp-for";
        protected const string DescribedByAttributeName = "described-by";
        protected const string FormGroupAttributesPrefix = "form-group-";
        protected const string IgnoreModelStateErrorsAttributeName = "ignore-modelstate-errors";
        protected const string NameAttributeName = "name";

        [HtmlAttributeName(AspForAttributeName)]
        public ModelExpression AspFor { get; set; }

        [HtmlAttributeName(DescribedByAttributeName)]
        public string DescribedBy { get; set; }

        [HtmlAttributeName(DictionaryAttributePrefix = FormGroupAttributesPrefix)]
        public IDictionary<string, string> FormGroupAttributes { get; set; } = new Dictionary<string, string>();

        [HtmlAttributeName(IgnoreModelStateErrorsAttributeName)]
        public bool? IgnoreModelStateErrors { get; set; }

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IGovUkHtmlGenerator Generator { get; }

        protected string ResolvedId => GetIdPrefix() ?? TagBuilder.CreateSanitizedId(ResolvedName, Constants.IdAttributeDotReplacement);

        protected string ResolvedName => Name ?? Generator.GetFullHtmlFieldName(ViewContext, AspFor.Name);

        protected FormGroupTagHelperBase(IGovUkHtmlGenerator htmlGenerator)
        {
            Generator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.ThrowIfOutputHasAttributes();

            if (Name == null && AspFor == null)
            {
                throw new InvalidOperationException(
                    $"At least one of the '{NameAttributeName}' and '{AspForAttributeName}' attributes must be specified.");
            }

            var builder = CreateFormGroupBuilder();

            using (context.SetScopedContextItem(FormGroupBuilder.ContextName, builder))
            {
                await output.GetChildContentAsync();
            }

            var tagBuilder = GenerateContent(context, builder);

            output.TagName = tagBuilder.TagName;
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.Clear();
            output.MergeAttributes(tagBuilder);
            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
        }

        protected void AppendToDescribedBy(string value)
        {
            if (value == null)
            {
                return;
            }

            if (DescribedBy == null)
            {
                DescribedBy = value;
            }
            else
            {
                DescribedBy += $" {value}";
            }
        }

        protected virtual FormGroupBuilder CreateFormGroupBuilder() => new FormGroupBuilder();

        protected virtual TagBuilder GenerateContent(TagHelperContext context, FormGroupBuilder builder)
        {
            // We need some content for the label; if AspFor is null then label content must have been specified
            if (AspFor == null && (!builder.Label.HasValue || builder.Label.Value.content == null))
            {
                throw new InvalidOperationException(
                    $"Label content must be specified when the '{AspForAttributeName}' attribute is not specified.");
            }

            var contentBuilder = new HtmlContentBuilder();

            var label = GenerateLabel(builder);
            contentBuilder.AppendHtml(label);

            var hint = GenerateHint(builder);
            if (hint != null)
            {
                contentBuilder.AppendHtml(hint);
            }

            var errorMessage = GenerateErrorMessage(builder);
            if (errorMessage != null)
            {
                contentBuilder.AppendHtml(errorMessage);
            }

            var haveError = errorMessage != null;

            var elementCtx = new FormGroupElementContext(haveError);
            var element = GenerateElement(builder, elementCtx);

            contentBuilder.AppendHtml(element);

            return Generator.GenerateFormGroup(haveError, FormGroupAttributes, contentBuilder);
        }

        protected virtual TagBuilder GenerateElement(FormGroupBuilder builder, FormGroupElementContext context)
        {
            // For deriving classes to implement when required
            throw new NotImplementedException();
        }

        protected abstract string GetIdPrefix();

        private protected IHtmlContent GenerateErrorMessage(FormGroupBuilder builder)
        {
            var visuallyHiddenText = builder.ErrorMessage?.visuallyHiddenText;
            var content = builder.ErrorMessage?.content;
            var attributes = builder.ErrorMessage?.attributes;

            if (content == null && AspFor != null && IgnoreModelStateErrors != true)
            {
                var validationMessage = Generator.GetValidationMessage(ViewContext, AspFor.ModelExplorer, AspFor.Name);

                if (validationMessage != null)
                {
                    content = new HtmlString(validationMessage);
                }
            }

            if (content != null)
            {
                var errorId = ResolvedId + "-error";
                AppendToDescribedBy(errorId);
                return Generator.GenerateErrorMessage(visuallyHiddenText, errorId, attributes, content);
            }
            else
            {
                return null;
            }
        }

        private protected virtual TagBuilder GenerateHint(FormGroupBuilder builder)
        {
            if (builder.Hint != null)
            {
                var hintId = ResolvedId + "-hint";
                AppendToDescribedBy(hintId);
                return Generator.GenerateHint(hintId, builder.Hint.Value.attributes, builder.Hint.Value.content);
            }
            else
            {
                return null;
            }
        }

        private protected virtual IHtmlContent GenerateLabel(FormGroupBuilder builder)
        {
            var isPageHeading = builder.Label?.isPageHeading ?? false;
            var content = builder.Label?.content;
            var attributes = builder.Label?.attributes;

            var resolvedContent = content ??
                new HtmlString(Generator.GetDisplayName(ViewContext, AspFor.ModelExplorer, AspFor.Name));

            return Generator.GenerateLabel(ResolvedId, isPageHeading, attributes, resolvedContent);
        }
    }

    public abstract class FormGroupLabelTagHelperBase : TagHelper
    {
        private const string AttributesPrefix = "label-";
        private const string IsPageHeadingAttributeName = "is-page-heading";

        private protected FormGroupLabelTagHelperBase()
        {
        }

        [HtmlAttributeName(DictionaryAttributePrefix = AttributesPrefix)]
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        [HtmlAttributeName(IsPageHeadingAttributeName)]
        public bool IsPageHeading { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.ThrowIfOutputHasAttributes();

            var childContent = output.TagMode == TagMode.StartTagAndEndTag ? await output.GetChildContentAsync() : null;

            var formGroupContext = (FormGroupBuilder)context.Items[FormGroupBuilder.ContextName];
            if (!formGroupContext.TrySetLabel(IsPageHeading, Attributes, childContent.Snapshot()))
            {
                throw new InvalidOperationException($"Cannot render <{context.TagName}> here.");
            }

            output.SuppressOutput();
        }
    }

    public abstract class FormGroupHintTagHelperBase : TagHelper
    {
        private const string AttributesPrefix = "hint-";

        [HtmlAttributeName(DictionaryAttributePrefix = AttributesPrefix)]
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        private protected FormGroupHintTagHelperBase()
        {
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.ThrowIfOutputHasAttributes();

            var childContent = output.TagMode == TagMode.StartTagAndEndTag ? await output.GetChildContentAsync() : null;

            var formGroupContext = (FormGroupBuilder)context.Items[FormGroupBuilder.ContextName];
            if (!formGroupContext.TrySetHint(Attributes, childContent.Snapshot()))
            {
                throw new InvalidOperationException($"Cannot render <{context.TagName}> here.");
            }

            output.SuppressOutput();
        }
    }

    public abstract class FormGroupErrorMessageTagHelperBase : TagHelper
    {
        private const string AttributesPrefix = "error-message-";
        private const string VisuallyHiddenTextAttributeName = "visually-hidden-text";

        private protected FormGroupErrorMessageTagHelperBase()
        {
        }

        [HtmlAttributeName(DictionaryAttributePrefix = AttributesPrefix)]
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        [HtmlAttributeName(VisuallyHiddenTextAttributeName)]
        public string VisuallyHiddenText { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.ThrowIfOutputHasAttributes();

            var childContent = output.TagMode == TagMode.StartTagAndEndTag ? await output.GetChildContentAsync() : null;

            var formGroupContext = (FormGroupBuilder)context.Items[FormGroupBuilder.ContextName];
            if (!formGroupContext.TrySetErrorMessage(VisuallyHiddenText, Attributes, childContent.Snapshot()))
            {
                throw new InvalidOperationException($"Cannot render <{context.TagName}> here.");
            }

            output.SuppressOutput();
        }
    }

    internal enum FormGroupRenderStage
    {
        None = 0,
        Label = 1,
        Hint = 2,
        ErrorMessage = 3,
        Element = 4
    }

    public class FormGroupBuilder
    {
        public const string ContextName = nameof(FormGroupBuilder);

        internal FormGroupBuilder()
        {
        }

        public (string visuallyHiddenText, IDictionary<string, string> attributes, IHtmlContent content)? ErrorMessage { get; private set; }

        public (IDictionary<string, string> attributes, IHtmlContent content)? Hint { get; private set; }

        public (bool isPageHeading, IDictionary<string, string> attributes, IHtmlContent content)? Label { get; private set; }

        // Internal for testing
        internal FormGroupRenderStage RenderStage { get; private set; } = FormGroupRenderStage.None;

        public bool TrySetErrorMessage(
            string visuallyHiddenText,
            IDictionary<string, string> attributes,
            IHtmlContent content)
        {
            if (RenderStage >= FormGroupRenderStage.ErrorMessage)
            {
                return false;
            }

            RenderStage = FormGroupRenderStage.ErrorMessage;
            ErrorMessage = (visuallyHiddenText, attributes, content);

            return true;
        }

        public bool TrySetHint(IDictionary<string, string> attributes, IHtmlContent content)
        {
            if (RenderStage >= FormGroupRenderStage.Hint)
            {
                return false;
            }

            RenderStage = FormGroupRenderStage.Hint;
            Hint = (attributes, content);

            return true;
        }

        public bool TrySetLabel(bool isPageHeading, IDictionary<string, string> attributes, IHtmlContent content)
        {
            if (RenderStage >= FormGroupRenderStage.Label)
            {
                return false;
            }

            RenderStage = FormGroupRenderStage.Label;
            Label = (isPageHeading, attributes, content);

            return true;
        }

        protected bool TrySetElementRenderStage()
        {
            if (RenderStage >= FormGroupRenderStage.Element)
            {
                return false;
            }

            RenderStage = FormGroupRenderStage.Element;

            return true;
        }
    }

    public class FormGroupElementContext
    {
        internal FormGroupElementContext(bool haveError)
        {
            HaveError = haveError;
        }

        public bool HaveError { get; }
    }
}
