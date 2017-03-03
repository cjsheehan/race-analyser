using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RacingWebScraper;

namespace Racing_Domain_Tests
{
    [TestClass]
    public class DistanceTests
    {
        [TestMethod]
        public void MilesFurlongsYardsConvertedToYards()
        {
            // Arrange
            string distance = "1m 2f 50yds";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 2250;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MilesOnlyConvertedToYards()
        {
            // Arrange
            string distance = "2m";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 3520;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MilesFurlongsConvertedToYards()
        {
            // Arrange
            string distance = "1m2f";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 2200;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FurlongsYardsConvertedToYards()
        {
            // Arrange
            string distance = "2f 110yds";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 550;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void YardsOnlyConvertedToYards()
        {
            // Arrange
            string distance = "120yds";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 120;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MilesYardsConvertedToYards()
        {
            // Arrange
            string distance = "1m 120yds";

            // Act
            double actual = Distance.ToYards(distance);

            // Assert
            const double expected = 1880;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BeatenWholeLengthsConvertedToDouble()
        {
            // Arrange
            string distance = "5";

            // Act
            double actual = Distance.ConvertLengthsToDouble(distance);

            // Assert
            const double expected = 5;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BeatenDecimalLengthsConvertedToDouble()
        {
            // Arrange
            string distance = "5.25";

            // Act
            double actual = Distance.ConvertLengthsToDouble(distance);

            // Assert
            const double expected = 5.25;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BeatenWholeAndFracLengthsConvertedToDouble()
        {
            // Arrange
            string distance = "1 1/2";

            // Act
            double actual = Distance.ConvertLengthsToDouble(distance);

            // Assert
            const double expected = 1.5;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BeatenUnicodeFracConvertedToDouble()
        {
            // Arrange
            String distance = new String(new char[] { '\u00BE' }); // char 3/4

            // Act
            double actual = Distance.ConvertLengthsToDouble(distance);

            // Assert
            const double expected = 0.75;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BeatenLengthsUnicodeWholeFracConvertedToDouble()
        {
            // Arrange
            String frac = new String(new char[] { '\u00BE' }); // char 3/4
            String distance = 10 + " " + frac;

            // Act
            double actual = Distance.ConvertLengthsToDouble(distance);

            // Assert
            const double expected = 10.75;
            Assert.AreEqual(expected, actual);
        }

    }
}
