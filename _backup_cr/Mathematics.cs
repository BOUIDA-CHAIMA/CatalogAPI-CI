namespace CatalogAPI_CHAIMA.Classes
{
    public class Mathematics
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
        public int Factorial(int n)
        {
            if (n < 0)
            {
                throw new ArgumentException("Input must be a non-negative integer.", nameof(n));
            }
            else
            if (n == 0 || n == 1)
            {
                return 1;
            }
            int result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }
}
