using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Infrastructure.SharedResources {
    [UsedImplicitly]
    public class InlineExpression {
        public static readonly DependencyProperty InlineExpressionProperty = DependencyProperty.RegisterAttached(
            "InlineExpression", typeof(string), typeof(TextBlock),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetInlineExpression(TextBlock textBlock, string value) {
            textBlock.SetValue(InlineExpressionProperty, value);

            textBlock.Inlines.Clear();

            if(string.IsNullOrEmpty(value))
                return;

            InlineDescription[] descriptions = GetInlineDescriptions(value);
            if(descriptions.Length == 0)
                return;

            Inline[] inlines = GetInlines(textBlock, descriptions);
            if(inlines.Length == 0)
                return;

            textBlock.Inlines.AddRange(inlines);
        }

        public static string GetInlineExpression(TextBlock textBlock) {
            return (string) textBlock.GetValue(InlineExpressionProperty);
        }

        private static Inline[] GetInlines(FrameworkElement element, IEnumerable<InlineDescription> inlineDescriptions) {
            return inlineDescriptions.Select(description => GetInline(element, description)).Where(inline => inline != null)
                                     .ToArray();
        }

        private static Inline GetInline(FrameworkElement element, InlineDescription description) {
            Style style = null;
            if(!string.IsNullOrEmpty(description.StyleName)) {
                style = element.FindResource(description.StyleName) as Style;
                if(style == null)
                    throw new InvalidOperationException("The style '" + description.StyleName + "' cannot be found");
            }

            Inline inline = null;
            switch(description.Type) {
                case InlineType.Run:
                    Run run = new(description.Text);
                    inline = run;
                    break;
                case InlineType.LineBreak:
                    LineBreak lineBreak = new();
                    inline = lineBreak;
                    break;
                case InlineType.Span:
                    Span span = new();
                    inline = span;
                    break;
                case InlineType.Bold:
                    Bold bold = new();
                    inline = bold;
                    break;
                case InlineType.Italic:
                    Italic italic = new();
                    inline = italic;
                    break;
                case InlineType.Hyperlink:
                    Hyperlink hyperlink = new();
                    inline = hyperlink;
                    break;
                case InlineType.Underline:
                    Underline underline = new();
                    inline = underline;
                    break;
            }

            if(inline != null) {
                if(inline is Span span) {
                    List<Inline> childInlines = description.Inlines
                                                           .Select(inlineDescription =>
                                                                           GetInline(element, inlineDescription))
                                                           .Where(childInline => childInline != null).ToList();

                    span.Inlines.AddRange(childInlines);
                }

                if(style != null) inline.Style = style;
            }

            return inline;
        }

        private static InlineDescription[] GetInlineDescriptions(string inlineExpression) {
            if(inlineExpression == null)
                return Array.Empty<InlineDescription>();

            inlineExpression = inlineExpression.Trim();
            if(inlineExpression.Length == 0)
                return Array.Empty<InlineDescription>();

            inlineExpression = inlineExpression.Insert(0, @"<root>");
            inlineExpression = inlineExpression.Insert(inlineExpression.Length, @"</root>");

            XmlTextReader xmlTextReader = new(new StringReader(inlineExpression));
            XmlDocument xmlDocument = new();
            xmlDocument.Load(xmlTextReader);

            var rootElement = xmlDocument.DocumentElement;
            return rootElement == null ?
                    Array.Empty<InlineDescription>() :
                    (from XmlNode childNode in rootElement.ChildNodes select GetInlineDescription(childNode) into description
                     where description != null select description).ToArray();
        }

        private static InlineDescription GetInlineDescription(XmlNode node) {
            return node switch {
                XmlElement element => GetInlineDescription(element),
                XmlText text => GetInlineDescription(text),
                _ => null
            };
        }

        private static InlineDescription GetInlineDescription(XmlElement element) {
            InlineType type;
            string elementName = element.Name.ToLower();
            switch(elementName) {
                case "run":
                    type = InlineType.Run;
                    break;
                case "linebreak":
                    type = InlineType.LineBreak;
                    break;
                case "span":
                    type = InlineType.Span;
                    break;
                case "bold":
                    type = InlineType.Bold;
                    break;
                case "italic":
                    type = InlineType.Italic;
                    break;
                case "hyperlink":
                    type = InlineType.Hyperlink;
                    break;
                case "underline":
                    type = InlineType.Underline;
                    break;
                default:
                    return null;
            }

            string styleName = null;
            var attribute = element.GetAttributeNode("style");
            if(attribute != null)
                styleName = attribute.Value;

            string text = null;
            var childDescriptions = new List<InlineDescription>();

            if(type == InlineType.Run || type == InlineType.LineBreak)
                text = element.InnerText;
            else
                childDescriptions.AddRange(from XmlNode childNode in element.ChildNodes
                                           select GetInlineDescription(childNode) into childDescription
                                           where childDescription != null select childDescription);

            InlineDescription inlineDescription = new()  {
                Type = type,
                StyleName = styleName,
                Text = text,
                Inlines = childDescriptions.ToArray()
            };

            return inlineDescription;
        }

        private static InlineDescription GetInlineDescription(XmlText text) {
            string value = text.Value;
            if(string.IsNullOrEmpty(value))
                return null;

            InlineDescription inlineDescription = new()  {
                Type = InlineType.Run,
                Text = value
            };
            return inlineDescription;
        }

        private enum InlineType {
            Run,
            LineBreak,
            Span,
            Bold,
            Italic,
            Hyperlink,
            Underline
        }

        private class InlineDescription {
            public InlineType Type { get; init; }
            public string Text { get; init; }
            public InlineDescription[] Inlines { get; init; }
            public string StyleName { get; init; }
        }
    }
}