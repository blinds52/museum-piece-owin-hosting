﻿using System.Linq;
using Owin.Types.Helpers;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Owin.Types.Tests
{
    public class HeaderSegmentsTests
    {
        [Fact]
        public void HeaderEnumerableReturnsOneItemForSingleValue()
        {
            var count = 0;
            foreach (var segment in new HeaderSegments(new[] { "value" }))
            {
                ++count;
                segment.Data.Value.ShouldBe("value");
            }
            count.ShouldBe(1);
        }

        [Fact]
        public void HeaderEnumerableReturnsLeadingWhitespace()
        {
            var count = 0;
            foreach (var segment in new HeaderSegments(new[] { " \r\n \t value" }))
            {
                ++count;
                segment.Formatting.Value.ShouldBe(" \r\n \t ");
                segment.Data.Value.ShouldBe("value");
            }
            count.ShouldBe(1);
        }

        [Fact]
        public void CommasSplitUp()
        {
            var segments = new HeaderSegments(new[] { " \r\n \t value, yep" }).ToArray();

            segments.Count().ShouldBe(2);
            segments[0].Formatting.Value.ShouldBe(" \r\n \t ");
            segments[0].Data.Value.ShouldBe("value");
            segments[1].Formatting.Value.ShouldBe(", ");
            segments[1].Data.Value.ShouldBe("yep");
        }

        [Fact]
        public void WhitespaceAddedToNextFormatting()
        {
            var segments = new HeaderSegments(new[] { "value  ,  yep" }).ToArray();

            segments.Count().ShouldBe(2);
            segments[0].Formatting.Value.ShouldBe("");
            segments[0].Data.Value.ShouldBe("value");
            segments[1].Formatting.Value.ShouldBe("  ,  ");
            segments[1].Data.Value.ShouldBe("yep");
        }

        [Fact]
        public void TrailingWhitespaceCausesFormattingSegmentWithNoData()
        {
            var segments = new HeaderSegments(new[] { "x " }).ToArray();

            segments.Count().ShouldBe(2);
            segments[0].Formatting.Value.ShouldBe("");
            segments[0].Data.Value.ShouldBe("x");
            segments[1].Formatting.Value.ShouldBe(" ");
            segments[1].Data.Value.ShouldBe(null);
        }

        [Fact]
        public void TailingWhitespaceHasThatEffectOn()
        {
            var segments = new HeaderSegments(new[] { "   value  ,  yep   " }).ToArray();

            segments.Count().ShouldBe(3);
            segments[0].Formatting.Value.ShouldBe("   ");
            segments[0].Data.Value.ShouldBe("value");
            segments[1].Formatting.Value.ShouldBe("  ,  ");
            segments[1].Data.Value.ShouldBe("yep");
            segments[2].Formatting.Value.ShouldBe("   ");
            segments[2].Data.Value.ShouldBe(null);
        }

        [Fact]
        public void QuotedCommasArePartOfValue()
        {
            var segments = new HeaderSegments(new[] { "\"   value  ,  yep   \"" }).ToArray();

            segments.Count().ShouldBe(1);
            segments[0].Formatting.Value.ShouldBe("");
            segments[0].Data.Value.ShouldBe("\"   value  ,  yep   \"");
        }

        [Theory]
        [InlineData(
            new[]
                {
                    "  this is very suspicious  "
                },
            new[]
                {
                    "  ",
                    "this is very suspicious",
                    "  ",
                    null
                })]
        public void SpacesInsideValueAreTolerated(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Theory]
        [InlineData(
            new[]
                {
                    "  \"this , is \" *very* suspicious , \"that , was \" *very* suspicious  "
                },
            new[]
                {
                    "  ",
                    "\"this , is \" *very* suspicious",
                    " , ", 
                    "\"that , was \" *very* suspicious",
                    "  ",
                    null
                })]
        public void MixingQuotedCommasAndSpacesInsideSingleValue(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Theory]
        [InlineData(
            new[] { "multiple", "simple", "values" },
            new[] { "", "multiple", "", "simple", "", "values" })]
        [InlineData(
            new[] { "\"multiple\"", "\"quoted\"", "\"values\"" },
            new[] { "", "\"multiple\"", "", "\"quoted\"", "", "\"values\"" })]
        public void MultipleHeadersAreDelimitedAutomatically(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Theory]
        [InlineData(
            new[] { "  multiple  ", "  simple  ", "  values  " },
            new[] { "  ", "multiple", "  ", null, "  ", "simple", "  ", null, "  ", "values", "  ", null })]
        public void MultipleHeadersAreMayBeIndividuallyComplex(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Theory]
        [InlineData(
            new[] { "" },
            new[] { "", null })]
        [InlineData(
            new[] { "  \r\n  " },
            new[] { "  \r\n  ", null })]
        [InlineData(
            new[] { "", "", "" },
            new[] { "", null, "", null, "", null })]
        public void EmptyStringsMayOccur(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Theory]
        [InlineData(
            new[] { (string)null },
            new[] { "", null })]
        [InlineData(
            new[] { "  \r\n  " },
            new[] { "  \r\n  ", null })]
        [InlineData(
            new[] { "", null, "" },
            new[] { "", null, "", null, "", null })]
        public void NullStringsMayOccur(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        [Fact]
        public void NullArrayMayOccur()
        {
            AssertCorrectness(null, new string[0]);
        }

        [Theory]
        [InlineData(
            new[] { "one\"two" },
            new[] { "", "one\"two" })]
        [InlineData(
            new[] { "\"" },
            new[] { "", "\"" })]
        [InlineData(
            new[] { "   one\"two   " },
            new[] { "   ", "one\"two   " })]
        [InlineData(
            new[] { "   one\"two   ", "  three  \"  " },
            new[] { "   ", "one\"two   ", "  ", "three  \"  " })]
        public void HeaderMayEndInsideQuote(string[] headers, string[] expected)
        {
            AssertCorrectness(headers, expected);
        }

        private void AssertCorrectness(string[] headers, string[] expected)
        {
            var segments = new HeaderSegments(headers);
            var strings = segments.SelectMany(seg => new[] { seg.Formatting.Value, seg.Data.Value }).ToArray();
            strings.ShouldBe(expected);
        }
    }
}
