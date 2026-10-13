using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa
{
    [TestFixture]
    public class MellerRabinPrimaryProbabilityTestTests
    {
        private MellerRabinPrimaryProbabilityTest _test;

        [SetUp]
        public void SetUp()
        {
            _test = new MellerRabinPrimaryProbabilityTest();
        }

        // =====================================================================
        // Простые числа — всегда true
        // =====================================================================

        [Test]
        public void PrimeNumbers_ReturnTrue()
        {
            // 100 первых простых чисел
            int[] primes = GetFirstPrimes(100);

            foreach (int p in primes)
            {
                for (int trial = 0; trial < 10; trial++)
                {
                    Assert.That(
                        _test.PrimaryProbabilityTest(p, 0.999),
                        Is.True,
                        $"Простое {p} вернуло false"
                    );
                }
            }
        }

        // =====================================================================
        // Составные числа — всегда false
        // =====================================================================

        [Test]
        public void CompositeNumbers_ReturnFalse()
        {
            int[] composites = GetComposites(100);

            foreach (int n in composites)
            {
                for (int trial = 0; trial < 10; trial++)
                {
                    Assert.That(
                        _test.PrimaryProbabilityTest(n, 0.999999999999),
                        Is.False,
                        $"Составное {n} вернуло true"
                    );
                }
            }
        }

        // =====================================================================
        // Числа Кармайкла — тоже false (главное преимущество над Ферма)
        // =====================================================================

        [Test]
        public void CarmichaelNumbers_ReturnFalse()
        {
            int[] carmichael = { 561, 1105, 1729, 2465, 2821, 6601, 8911, 10585, 15841 };

            foreach (int n in carmichael)
            {
                for (int trial = 0; trial < 10; trial++)
                {
                    Assert.That(
                        _test.PrimaryProbabilityTest(n, 0.999),
                        Is.False,
                        $"Число Кармайкла {n} вернуло true"
                    );
                }
            }
        }

        // =====================================================================
        // Крайние случаи
        // =====================================================================

        [Test]
        public void One_ReturnsFalse()
        {
            Assert.That(_test.PrimaryProbabilityTest(1, 0.999), Is.False);
        }

        [Test]
        public void Two_ReturnsTrue()
        {
            Assert.That(_test.PrimaryProbabilityTest(2, 0.999), Is.True);
        }

        // =====================================================================
        // Вспомогательные методы
        // =====================================================================

        private static int[] GetFirstPrimes(int count)
        {
            var primes = new List<int>();
            int n = 2;
            while (primes.Count < count)
            {
                if (IsPrime(n))
                    primes.Add(n);
                n++;
            }

            return primes.ToArray();
        }

        private static int[] GetComposites(int count)
        {
            var composites = new List<int>();
            int n = 4;
            while (composites.Count < count)
            {
                if (!IsPrime(n))
                    composites.Add(n);
                n++;
            }

            return composites.ToArray();
        }

        private static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;
            for (int d = 3; d * d <= n; d += 2)
                if (n % d == 0)
                    return false;
            return true;
        }
    }
}