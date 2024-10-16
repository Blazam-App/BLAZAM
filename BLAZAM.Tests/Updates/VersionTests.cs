﻿using BLAZAM.Common.Data;
using BLAZAM.Server.Data;
using BLAZAM.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Updates
{
    public class VersionTests
    {
        private ApplicationVersion basicLow = new ApplicationVersion("0.5.1");
        private ApplicationVersion basicHigh = new ApplicationVersion("0.5.2");
        private ApplicationVersion longLow = new ApplicationVersion("0.5.1.2023.2.2.1053");
        private ApplicationVersion longHigh = new ApplicationVersion("0.5.2.2023.2.3.1053");

        [Fact]
        public void Basic_Comparison_Valid()
        {
            Assert.True(basicLow.CompareTo(basicHigh) < 0);
        }
        [Fact]
        public void Long_Comparison_Returns_LessThanZero_When_Older()
        {
            Assert.True(longLow.CompareTo(longHigh) < 0);
        }
        [Fact]
        public void Long_Basic_Comparison_Returns_LessThanZero_When_Other_Is_Older()
        {
            Assert.True(longLow.CompareTo(basicHigh) < 0);
        }

        [Fact]
        public void Long_Basic_Comparison_Returns_GreaterThanZero_When_Other_Is_Older()
        {
            Assert.True(longHigh.CompareTo(basicLow) > 0);
        }
        [Fact]
        public void Long_Comparison_Returns_GreaterThanZero_When_Other_Is_Newer()
        {
            Assert.True(longLow.CompareTo(longHigh) < 0);
        }

        [Fact]
        public void Long_Basic_Comparison_Returns_Zero_When_Same()
        {
            Assert.True(longLow.CompareTo(basicLow) == 0);
        }
        [Fact]
        public void CompareVersions_Returns_Zero_When_Same()
        {
            Assert.True(longLow.CompareTo(longLow) == 0);
        }
        [Fact]
        public void CompareVersions_Returns_GreaterThanZero_When_Other_Is_Older()
        {
            Assert.True(longHigh.CompareTo(longLow) > 0);
        }
        [Fact]
        public void CompareVersions_Returns_LessThanZero_When_Other_Is_Newer()
        {
            Assert.True(longLow.CompareTo(longHigh) < 0);
        }
    }
}
