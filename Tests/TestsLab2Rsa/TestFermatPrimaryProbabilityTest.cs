using System.Numerics;
using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa
{

    // =========================================================================
    // Тесты FermatPrimaryProbabilityTest
    // =========================================================================

    [TestFixture]
    public class FermatPrimaryProbabilityTestTests
    {
        private FermatPrimaryProbabilityTest _test;
        private TestableFermat _testable;

        [SetUp]
        public void SetUp()
        {
            _test = new FermatPrimaryProbabilityTest();
            _testable = new TestableFermat();
        }

        private class TestableFermat : FermatPrimaryProbabilityTest
        {
            public bool Step(BigInteger n, BigInteger a) => PrimaryProbabilityTestStep(n, a);
            public double ErrorProb => ErrorProbability;
        }

        // =====================================================================
        // ErrorProbability
        // =====================================================================

        [Test]
        public void ErrorProbability_EqualsHalf()
        {
            Assert.That(_testable.ErrorProb, Is.EqualTo(0.5));
        }

        // =====================================================================
        // PrimaryProbabilityTestStep — детерминированные случаи
        // =====================================================================

        [TestCase(7, 2, true)]
        [TestCase(7, 3, true)]
        [TestCase(7, 7, false)]
        [TestCase(7, 14, false)]
        [TestCase(15, 2, false)]
        [TestCase(15, 4, true)]
        [TestCase(15, 11, true)]
        [TestCase(15, 14, true)]
        [TestCase(15, 3, false)]
        [TestCase(4, 1, true)]
        [TestCase(4, 3, false)]
        [TestCase(4, 2, false)]
        [TestCase(2, 1, true)]
        [TestCase(2, 2, false)]
        [TestCase(9, 1, true)]
        [TestCase(9, 8, true)]
        [TestCase(9, 3, false)]
        [TestCase(9, 2, false)]
        public void Step_KnownValues(int n, int a, bool expected)
        {
            Assert.That(_testable.Step(n, a), Is.EqualTo(expected), $"n={n}, a={a}");
        }

        [Test]
        public void Step_Prime_AllBasesBehaveAccordingToGcd()
        {
            int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97 };

            foreach (int p in primes)
            {
                for (int a = 0; a <= p + 5; a++)
                {
                    bool expected = BigInteger.GreatestCommonDivisor(p, a) == 1;
                    Assert.That(
                        _testable.Step(p, a),
                        Is.EqualTo(expected),
                        $"p={p}, a={a}"
                    );
                }
            }
        }

        [Test]
        public void Step_Prime_AllCoprimeBasesPass()
        {
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101, 997 };

            foreach (int p in primes)
            {
                for (int a = 1; a < p; a++)
                {
                    if (BigInteger.GreatestCommonDivisor(p, a) != 1)
                        continue;

                    Assert.That(
                        _testable.Step(p, a),
                        Is.True,
                        $"p={p}, a={a}"
                    );
                }
            }
        }

        [Test]
        public void Step_Composite_FailsForAtLeastOneBase()
        {
            // Для любого составного n, не числа Кармайкла, найдётся база a с gcd=1 и a^(n-1) != 1
            int[] nonCarmichaelComposites =
            {
                4, 6, 8, 9, 10, 12, 14, 16, 18, 20, 21, 22, 24, 25, 26, 27, 28, 30,
                33, 34, 35, 36, 38, 39, 40, 42, 44, 45, 46, 48, 49, 50
            };

            foreach (int n in nonCarmichaelComposites)
            {
                bool foundFailingBase = false;
                for (int a = 1; a < n; a++)
                {
                    if (BigInteger.GreatestCommonDivisor(n, a) != 1)
                        continue;
                    if (!_testable.Step(n, a))
                    {
                        foundFailingBase = true;
                        break;
                    }
                }

                Assert.That(foundFailingBase, Is.True, $"n={n}: не найдена база, разоблачающая составное");
            }
        }

        // =====================================================================
        // Полный тест PrimaryProbabilityTest — простые числа
        // =====================================================================

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(11)]
        [TestCase(13)]
        [TestCase(17)]
        [TestCase(19)]
        [TestCase(23)]
        [TestCase(29)]
        [TestCase(31)]
        [TestCase(97)]
        [TestCase(101)]
        [TestCase(997)]
        [TestCase(7919)]
        public void Prime_AlwaysReturnsTrue(int prime)
        {
            for (int trial = 0; trial < 20; trial++)
            {
                Assert.That(
                    _test.PrimaryProbabilityTest(prime, 0.99),
                    Is.True,
                    $"prime={prime}, trial={trial}"
                );
            }
        }

        [Test]
        public void Prime_AllSmallPrimesReturnTrue()
        {
            for (int n = 2; n <= 300; n++)
            {
                if (!IsPrime(n))
                    continue;

                for (int trial = 0; trial < 5; trial++)
                {
                    Assert.That(
                        _test.PrimaryProbabilityTest(n, 0.99),
                        Is.True,
                        $"n={n}"
                    );
                }
            }
        }

        // =====================================================================
        // Полный тест PrimaryProbabilityTest — составные числа
        // =====================================================================

        [Test]
        public void Composite_DetectedInMostTrials()
        {
            int[] composites =
            {
                4, 6, 8, 9, 10, 12, 14, 15, 16, 18, 20, 21, 22, 24, 25, 26, 27, 28, 30,
                33, 34, 35, 36, 38, 39, 40, 42, 44, 45, 46, 48, 49, 50,
                77, 91, 121, 143, 169, 187
            };

            const int trials = 200;
            const int minDetections = 50;

            foreach (int n in composites)
            {
                int detected = 0;
                for (int i = 0; i < trials; i++)
                {
                    if (!_test.PrimaryProbabilityTest(n, 0.99))
                        detected++;
                }

                Assert.That(
                    detected,
                    Is.GreaterThanOrEqualTo(minDetections),
                    $"Составное {n} обнаружено только {detected}/{trials} раз"
                );
            }
        }

        [Test]
        public void Composite_AllNonCarmichaelFrom1To100_DetectedAtLeastOnce()
        {
            for (int n = 4; n <= 100; n++)
            {
                if (IsPrime(n))
                    continue;

                bool detected = false;
                for (int trial = 0; trial < 100; trial++)
                {
                    if (!_test.PrimaryProbabilityTest(n, 0.99))
                    {
                        detected = true;
                        break;
                    }
                }

                Assert.That(detected, Is.True, $"Составное {n} ни разу не обнаружено за 100 попыток");
            }
        }

        // =====================================================================
        // Крайние случаи
        // =====================================================================

        [Test]
        public void One_AlwaysReturnsFalse()
        {
            // n=1: a^0 mod 1 = 0 ≠ 1 при n=1
            for (int i = 0; i < 100; i++)
            {
                Assert.That(_test.PrimaryProbabilityTest(1, 0.99), Is.False);
            }
        }

        [Test]
        public void Two_AlwaysReturnsTrue()
        {
            for (int i = 0; i < 100; i++)
            {
                Assert.That(_test.PrimaryProbabilityTest(2, 0.99), Is.True);
            }
        }

        // =====================================================================
        // Числа Кармайкла
        // =====================================================================

        [Test]
        public void CarmichaelNumbers_FrequentlyPassFermatTest()
        {
            int[] carmichael = { 561, 1105, 1729, 2465, 2821, 6601 };

            foreach (int n in carmichael)
            {
                int passes = 0;
                const int trials = 100;

                for (int i = 0; i < trials; i++)
                {
                    if (_test.PrimaryProbabilityTest(n, 0.99))
                        passes++;
                }

                Assert.That(
                    passes,
                    Is.GreaterThan(0),
                    $"Число Кармайкла {n} ни разу не прошло тест за {trials} попыток"
                );
            }
        }

        [Test]
        public void CarmichaelNumbers_NotAlwaysPass()
        {
            int[] carmichael = { 561, 1105, 1729 };

            foreach (int n in carmichael)
            {
                int fails = 0;
                const int trials = 300;

                for (int i = 0; i < trials; i++)
                {
                    if (!_test.PrimaryProbabilityTest(n, 0.99))
                        fails++;
                }

                Assert.That(
                    fails,
                    Is.GreaterThan(0),
                    $"Число Кармайкла {n} ни разу не было разоблачено за {trials} попыток"
                );
            }
        }

        // =====================================================================
        // Согласованность с эталонной проверкой на простоту
        // =====================================================================

        [Test]
        public void SmallNumbers_PrimeAlwaysTrue_CompositeSometimesFalse()
        {
            for (int n = 2; n <= 200; n++)
            {
                bool isPrime = IsPrime(n);
                bool anyTrue = false;
                bool anyFalse = false;
                const int trials = 50;

                for (int i = 0; i < trials; i++)
                {
                    bool result = _test.PrimaryProbabilityTest(n, 0.99);
                    if (result) anyTrue = true;
                    else anyFalse = true;
                }

                if (isPrime)
                {
                    Assert.That(anyTrue, Is.True, $"Простое {n} ни разу не вернуло true");
                    Assert.That(anyFalse, Is.False, $"Простое {n} вернуло false");
                }
                else
                {
                    Assert.That(anyFalse, Is.True, $"Составное {n} ни разу не вернуло false");
                }
            }
        }

        [Test]
        public void MaxSmallValues_CompareWithReferenceFermatStep()
        {
            for (int n = 2; n <= 300; n++)
            {
                for (int a = 0; a <= 50; a++)
                {
                    bool expected = ReferenceFermatStep(n, a);
                    bool actual = _testable.Step(n, a);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}, a={a}");
                }
            }
        }

        // =====================================================================
        // Вспомогательные методы
        // =====================================================================

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

        private static bool ReferenceFermatStep(BigInteger n, BigInteger a)
        {
            if (BigInteger.GreatestCommonDivisor(n, a) != 1)
                return false;

            if (n <= 1)
                return false;

            return BigInteger.ModPow(a, n - 1, n) == 1;
        }
    }
}