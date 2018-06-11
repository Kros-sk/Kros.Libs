﻿using FluentAssertions;
using Kros.Extensions;
using System;
using Xunit;

namespace Kros.Utils.UnitTests.Extensions
{
    public class StringExtensionsShould
    {
        [Fact]
        public void RemoveDiacriticsFromString()
        {
            const string value = "áäčďéěíľĺňńóôŕřšťúýž ÁÄČĎÉĚÍĽĹŇŃÓÔŔŘŠŤÚÝŽ";
            const string expected = "aacdeeillnnoorrstuyz AACDEEILLNNOORRSTUYZ";
            string actual = value.RemoveDiacritics();
            actual.Should().Be(expected);
        }

        [Fact]
        public void ReturnTrueWhenStringIsNullOrEmpty()
        {
            string value = null;
            bool actual = value.IsNullOrEmpty();
            actual.Should().Be(true, "value = null");

            value = "";
            actual = value.IsNullOrEmpty();
            actual.Should().Be(true, "value = \"\"");

            value = "lorem";
            actual = value.IsNullOrEmpty();
            actual.Should().Be(false, "value = \"lorem\"");
        }

        [Fact]
        public void ReturnTrueWhenStringIsNullOrWhitespace()
        {
            string value = null;
            bool actual = value.IsNullOrWhiteSpace();
            actual.Should().Be(true, "value = null");

            value = "";
            actual = value.IsNullOrWhiteSpace();
            actual.Should().Be(true, "value = \"\"");

            value = " \t \r \n ";
            actual = value.IsNullOrWhiteSpace();
            actual.Should().Be(true, "value = \" \\t \\r \\n \"");

            value = "lorem";
            actual = value.IsNullOrWhiteSpace();
            actual.Should().Be(false, "value = \"lorem\"");
        }

        [Fact]
        public void ReturnCorrectStartOfString()
        {
            const string value = "lorem ipsum dolor";
            value.Left(5).Should().Be("lorem", "Length = 5");
            value.Left(500).Should().Be(value, "Length is greater than string length.");
            Action action = () => value.Left(-5);
            action.Should().Throw<ArgumentException>("Length is negative.");
        }

        [Fact]
        public void ReturnCorrectEndOfString()
        {
            const string value = "lorem ipsum dolor";
            value.Right(5).Should().Be("dolor", "Length = 5");
            value.Right(500).Should().Be(value, "Length is greater than string length.");
            Action action = () => value.Right(-5);
            action.Should().Throw<ArgumentException>("Length is negative.");
        }

#region RemoveNewLines

        [Fact]
        public void ReturnNullWhenInputIsNull()
        {
            string actual = null;

            actual.RemoveNewLines().Should().BeNull();
        }

        [Fact]
        public void ReturEmptyStringWhenInputIsEmptyString()
        {
            string.Empty.RemoveNewLines().Should().BeEmpty();
        }

        [Fact]
        public void ReturnSameStringIfItDoesNotContainNewLines()
        {
            string actual = "I ate all new lines. Yummy! No left for you";

            actual.RemoveNewLines().Should().Be(actual);
        }

        [Fact]
        public void RemoveStandardEnvironmentNewLines()
        {
            string actual = string.Format("{0}Lorem{0}Ipsum{0}", Environment.NewLine);

            actual.RemoveNewLines().Should().Be("LoremIpsum");
        }

        [Fact]
        public void RemoveNewLineCharacters()
        {
            string actual = "Meanwhile\r\n when copying\r from\n Excel\r\n";

            actual.RemoveNewLines().Should().Be("Meanwhile when copying from Excel");
        }

        #endregion
    }
}
