using CatalogAPI_CHAIMA.Classes;

namespace AppTests
{
    public class SumTests
    {
        [Fact]
        public void Sum_PositiveNumbers_ReturnsCorrectResult()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Sum(5, 10);
            Assert.Equal(15, result);
        }
        [Fact]
        public void Sum_NegativeAndPositiveNumbers_ReturnsCorrectResult()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Sum(-3, 7);
            Assert.Equal(4, result);
        }
        [Fact]
        public void Sum_NegativeNumbers_ReturnsCorrectResult()
        {
            var mathematics = new Mathematics();
            var result = mathematics.Sum(-6, -21);
            Assert.Equal(-27, result);
        }
    }
}