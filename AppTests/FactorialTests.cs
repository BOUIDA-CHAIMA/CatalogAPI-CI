using CatalogAPI_CHAIMA.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTests
{
    public class FactorialTests
    {
        [Fact]
        public void Factorial_PositiveInteger_ReturnsCorrectResult()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Factorial(5);
            Assert.Equal(120, result);
        }
        [Fact]
        public void Factorial_Zero_ReturnsOne()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Factorial(0);
            Assert.Equal(1, result);
        }
        [Fact]
        public void Factorial_One_ReturnsOne()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Factorial(1);
            Assert.Equal(1, result);
        }
        [Fact]
        public void Factorial_NegativeInteger_ThrowsArgumentException()
        {
            var mathematics = new Mathematics();
            Assert.Throws<ArgumentException>(() => mathematics.Factorial(-3));
        }

        [Theory]
        [InlineData(5, 120)]
        [InlineData(1, 1)]
        [InlineData(0, 1)]
        public void Factorial_ValidInputs_ReturnsExpectedResult(int input, int expected)
        {
            var mathematics = new Mathematics();
            var result = mathematics.Factorial(input);
            Assert.Equal(expected, result);
        }
        [Theory]
        [InlineData(-1)]
        [InlineData(-3)]
        [InlineData(-10)]
        public void Factorial_NegativeInputs_ThrowsArgumentException(int input)
        {
            var mathematics = new Mathematics();
            Assert.Throws<ArgumentException>(() => mathematics.Factorial(input));
        }

    }
}
