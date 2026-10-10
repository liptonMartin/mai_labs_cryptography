using System.Numerics;
using RsaMath = Core.Lab_2_RSA.Math;

namespace Tests.TestsLab2Rsa
{
    [TestFixture]
    public class FastBinaryPowModuleTests
    {
        private Random _random;

        [SetUp]
        public void SetUp()
        {
            _random = new Random(12345);
        }

        [Test]
        public void ExponentZero_ReturnsOne()
        {
            Assert.That(RsaMath.FastBinaryPowModule(2, 0, 13), Is.EqualTo(1));
            Assert.That(RsaMath.FastBinaryPowModule(123, 0, 1000), Is.EqualTo(1));
            Assert.That(RsaMath.FastBinaryPowModule(0, 0, 13), Is.EqualTo(1));
        }

        [Test]
        public void BaseZero_PositiveExponent_ReturnsZero()
        {
            Assert.That(RsaMath.FastBinaryPowModule(0, 1, 13), Is.EqualTo(0));
            Assert.That(RsaMath.FastBinaryPowModule(0, 10, 13), Is.EqualTo(0));
        }

        [Test]
        public void SimpleCases()
        {
            Assert.That(RsaMath.FastBinaryPowModule(2, 10, 1000), Is.EqualTo(24));
            Assert.That(RsaMath.FastBinaryPowModule(3, 4, 5), Is.EqualTo(1));
            Assert.That(RsaMath.FastBinaryPowModule(5, 3, 13), Is.EqualTo(8));
        }

        [Test]
        public void ModulusOne_ReturnsZero()
        {
            Assert.That(RsaMath.FastBinaryPowModule(2, 0, 1), Is.EqualTo(0));
            Assert.That(RsaMath.FastBinaryPowModule(2, 1, 1), Is.EqualTo(0));
            Assert.That(RsaMath.FastBinaryPowModule(2, 2, 1), Is.EqualTo(0));
        }

        [Test]
        public void CompareWithBigIntegerModPow_AllSmallValues()
        {
            for (uint a = 0; a <= 30; a++)
            {
                for (uint b = 0; b <= 30; b++)
                {
                    for (uint mod = 1; mod <= 30; mod++)
                    {
                        uint expected = (uint)BigInteger.ModPow(a, b, mod);
                        uint actual = RsaMath.FastBinaryPowModule(a, b, mod);

                        Assert.That(
                            actual,
                            Is.EqualTo(expected),
                            $"a={a}, b={b}, mod={mod}"
                        );
                    }
                }
            }
        }

        [Test]
        public void CompareWithBigIntegerModPow_RandomValues()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint a = (uint)_random.Next(0, int.MaxValue);
                uint b = (uint)_random.Next(0, 1000);
                uint mod = (uint)_random.Next(1, int.MaxValue);

                uint expected = (uint)BigInteger.ModPow(a, b, mod);
                uint actual = RsaMath.FastBinaryPowModule(a, b, mod);

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"a={a}, b={b}, mod={mod}"
                );
            }
        }

        [Test]
        public void MaxValues_CompareWithBigIntegerModPow()
        {
            uint a = uint.MaxValue;
            uint b = 2;
            uint mod = uint.MaxValue;

            uint expected = (uint)BigInteger.ModPow(a, b, mod);
            uint actual = RsaMath.FastBinaryPowModule(a, b, mod);

            Assert.That(actual, Is.EqualTo(expected), $"a={a}, b={b}, mod={mod}");
        }

        [Test]
        public void ModulusZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() =>
                RsaMath.FastBinaryPowModule(2, 3, 0)
            );
        }
    }

    [TestFixture]
    public class LegendreSymbolTests
    {
        [TestCase(0, 3, 0)]
        [TestCase(1, 3, 1)]
        [TestCase(2, 3, -1)]
        [TestCase(0, 5, 0)]
        [TestCase(1, 5, 1)]
        [TestCase(4, 5, 1)]
        [TestCase(2, 5, -1)]
        [TestCase(3, 5, -1)]
        [TestCase(0, 7, 0)]
        [TestCase(1, 7, 1)]
        [TestCase(2, 7, 1)]
        [TestCase(4, 7, 1)]
        [TestCase(3, 7, -1)]
        [TestCase(5, 7, -1)]
        [TestCase(6, 7, -1)]
        [TestCase(7, 7, 0)]
        [TestCase(14, 7, 0)]
        [TestCase(8, 7, 1)] // 8 mod 7 = 1
        [TestCase(10, 7, -1)] // 10 mod 7 = 3
        [TestCase(0, 13, 0)]
        [TestCase(1, 13, 1)]
        [TestCase(3, 13, 1)]
        [TestCase(4, 13, 1)]
        [TestCase(9, 13, 1)]
        [TestCase(10, 13, 1)]
        [TestCase(12, 13, 1)]
        [TestCase(2, 13, -1)]
        [TestCase(5, 13, -1)]
        [TestCase(6, 13, -1)]
        [TestCase(7, 13, -1)]
        [TestCase(8, 13, -1)]
        [TestCase(11, 13, -1)]
        public void KnownValues(int a, int p, int expected)
        {
            Assert.That(
                RsaMath.CalcLegendreSymbol((uint)a, (uint)p),
                Is.EqualTo(expected)
            );
        }

        [Test]
        public void QuadraticResiduesAndNonResidues_ForSmallOddPrimes()
        {
            uint[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (uint p in primes)
            {
                var residues = new HashSet<uint>();

                for (uint x = 0; x < p; x++)
                {
                    residues.Add((x * x) % p);
                }

                for (uint a = 0; a < p; a++)
                {
                    int expected = a == 0
                        ? 0
                        : residues.Contains(a)
                            ? 1
                            : -1;

                    int actual = RsaMath.CalcLegendreSymbol(a, p);

                    Assert.That(
                        actual,
                        Is.EqualTo(expected),
                        $"a={a}, p={p}"
                    );
                }
            }
        }

        [Test]
        public void CompareWithEulerCriterion_AllSmallValues()
        {
            uint[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (uint p in primes)
            {
                for (uint a = 0; a <= 50; a++)
                {
                    int expected = EulerCriterion(a, p);
                    int actual = RsaMath.CalcLegendreSymbol(a, p);

                    Assert.That(
                        actual,
                        Is.EqualTo(expected),
                        $"a={a}, p={p}"
                    );
                }
            }
        }

        [Test]
        public void CompareWithEulerCriterion_RandomValues()
        {
            var random = new Random(12345);

            uint[] primes =
            {
                3, 5, 7, 11, 13, 17, 19, 23, 29, 31,
                37, 41, 43, 47, 53, 59, 61, 67, 71, 73,
                79, 83, 89, 97
            };

            for (int i = 0; i < 1000; i++)
            {
                uint p = primes[random.Next(primes.Length)];
                uint a = (uint)random.Next(0, int.MaxValue);

                int expected = EulerCriterion(a, p);
                int actual = RsaMath.CalcLegendreSymbol(a, p);

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"a={a}, p={p}"
                );
            }
        }

        [Test]
        public void ModulusZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() =>
                RsaMath.CalcLegendreSymbol(2, 0)
            );
        }

        private static int EulerCriterion(uint a, uint p)
        {
            uint mod = a % p;
            if (mod == 0)
                return 0;

            uint pow = (uint)BigInteger.ModPow(a, (p - 1) / 2, p);
            return pow == p - 1 ? -1 : (int)pow;
        }
    }

    [TestFixture]
    public class JacobiSymbolTests
    {
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 1)]
        [TestCase(5, 1, 1)]
        [TestCase(100, 1, 1)]
        public void ModulusOne_ReturnsOne(int a, int n, int expected)
        {
            Assert.That(
                RsaMath.CalcJacobiSymbol((uint)a, (uint)n),
                Is.EqualTo(expected)
            );
        }

        [TestCase(0, 3, 0)]
        [TestCase(1, 3, 1)]
        [TestCase(2, 3, -1)]
        [TestCase(0, 5, 0)]
        [TestCase(1, 5, 1)]
        [TestCase(4, 5, 1)]
        [TestCase(2, 5, -1)]
        [TestCase(3, 5, -1)]
        [TestCase(0, 7, 0)]
        [TestCase(1, 7, 1)]
        [TestCase(2, 7, 1)]
        [TestCase(4, 7, 1)]
        [TestCase(3, 7, -1)]
        [TestCase(5, 7, -1)]
        [TestCase(6, 7, -1)]
        [TestCase(7, 7, 0)]
        [TestCase(14, 7, 0)]
        [TestCase(8, 7, 1)]
        [TestCase(10, 7, -1)]
        // n = 9 = 3^2: (a/9) = (a/3)^2
        [TestCase(0, 9, 0)]
        [TestCase(1, 9, 1)]
        [TestCase(2, 9, 1)]
        [TestCase(3, 9, 0)]
        [TestCase(4, 9, 1)]
        [TestCase(5, 9, 1)]
        [TestCase(6, 9, 0)]
        [TestCase(7, 9, 1)]
        [TestCase(8, 9, 1)]
        [TestCase(9, 9, 0)]
        [TestCase(18, 9, 0)]
        // n = 15 = 3 * 5
        [TestCase(0, 15, 0)]
        [TestCase(1, 15, 1)]
        [TestCase(2, 15, 1)] // (2/3)(2/5) = (-1)(-1)
        [TestCase(3, 15, 0)]
        [TestCase(4, 15, 1)]
        [TestCase(5, 15, 0)]
        [TestCase(6, 15, 0)]
        [TestCase(7, 15, -1)] // (1/3)(2/5) = 1 * -1
        [TestCase(8, 15, 1)] // (2/3)^3(3/5) = -1 * -1
        [TestCase(11, 15, -1)] // (2/3)(1/5) = -1 * 1
        [TestCase(13, 15, -1)] // (1/3)(3/5) = 1 * -1
        [TestCase(14, 15, -1)] // (2/3)(4/5) = -1 * 1
        // n = 21 = 3 * 7
        [TestCase(0, 21, 0)]
        [TestCase(1, 21, 1)]
        [TestCase(2, 21, -1)] // (2/3)(2/7) = -1 * 1
        [TestCase(3, 21, 0)]
        [TestCase(4, 21, 1)]
        [TestCase(5, 21, 1)] // (2/3)(5/7) = -1 * -1
        [TestCase(7, 21, 0)]
        [TestCase(8, 21, -1)] // (2/3)(1/7) = -1 * 1
        [TestCase(10, 21, -1)] // (1/3)(3/7) = 1 * -1
        // n = 25 = 5^2
        [TestCase(0, 25, 0)]
        [TestCase(1, 25, 1)]
        [TestCase(2, 25, 1)] // (2/5)^2
        [TestCase(3, 25, 1)] // (3/5)^2
        [TestCase(4, 25, 1)]
        [TestCase(5, 25, 0)]
        // n = 27 = 3^3
        [TestCase(0, 27, 0)]
        [TestCase(1, 27, 1)]
        [TestCase(2, 27, -1)] // (2/3)^3
        [TestCase(3, 27, 0)]
        [TestCase(4, 27, 1)] // (1/3)^3
        [TestCase(8, 27, -1)] // (2/3)^9
        // n = 45 = 3^2 * 5
        [TestCase(0, 45, 0)]
        [TestCase(1, 45, 1)]
        [TestCase(2, 45, -1)] // (2/3)^2 (2/5) = 1 * -1
        [TestCase(4, 45, 1)] // (1/3)^2 (4/5) = 1 * 1
        [TestCase(7, 45, -1)] // (1/3)^2 (2/5) = 1 * -1
        public void KnownValues(int a, int n, int expected)
        {
            Assert.That(
                RsaMath.CalcJacobiSymbol((uint)a, (uint)n),
                Is.EqualTo(expected),
                $"a={a}, n={n}"
            );
        }

        [Test]
        public void AgreementWithLegendre_ForPrimeModuli()
        {
            uint[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (uint p in primes)
            {
                for (uint a = 0; a <= 50; a++)
                {
                    int legendre = RsaMath.CalcLegendreSymbol(a, p);
                    int jacobi = RsaMath.CalcJacobiSymbol(a, p);

                    Assert.That(
                        jacobi,
                        Is.EqualTo(legendre),
                        $"a={a}, p={p}"
                    );
                }
            }
        }

        [Test]
        public void AgreementWithLegendre_AllResiduesForOddPrimes()
        {
            uint[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (uint p in primes)
            {
                for (uint a = 0; a < p; a++)
                {
                    int legendre = RsaMath.CalcLegendreSymbol(a, p);
                    int jacobi = RsaMath.CalcJacobiSymbol(a, p);

                    Assert.That(
                        jacobi,
                        Is.EqualTo(legendre),
                        $"a={a}, p={p}"
                    );
                }
            }
        }

        [Test]
        public void CompareWithDirectDefinition_AllOddCompositeModuli()
        {
            // Проверяем все нечётные n от 3 до 199 и все a от 0 до 99
            for (uint n = 3; n <= 199; n += 2)
            {
                for (uint a = 0; a <= 99; a++)
                {
                    int expected = JacobiByDefinition(a, n);
                    int actual = RsaMath.CalcJacobiSymbol(a, n);

                    Assert.That(
                        actual,
                        Is.EqualTo(expected),
                        $"a={a}, n={n}"
                    );
                }
            }
        }

        [Test]
        public void CompareWithDirectDefinition_RandomValues()
        {
            var random = new Random(54321);

            for (int i = 0; i < 2000; i++)
            {
                // Случайное нечётное n от 3 до 10_000
                uint n = (uint)(random.Next(1, 5000) * 2 + 1);
                uint a = (uint)random.Next(0, int.MaxValue);

                int expected = JacobiByDefinition(a, n);
                int actual = RsaMath.CalcJacobiSymbol(a, n);

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"a={a}, n={n}"
                );
            }
        }

        [Test]
        public void Multiplicativity_InFirstArgument()
        {
            // (a*b / n) = (a/n) * (b/n)
            var random = new Random(777);

            for (int i = 0; i < 500; i++)
            {
                uint n = (uint)(random.Next(1, 500) * 2 + 1);
                uint a = (uint)random.Next(0, 10_000);
                uint b = (uint)random.Next(0, 10_000);

                int left = RsaMath.CalcJacobiSymbol(a * b, n);
                int right = RsaMath.CalcJacobiSymbol(a, n)
                            * RsaMath.CalcJacobiSymbol(b, n);

                Assert.That(
                    left,
                    Is.EqualTo(right),
                    $"a={a}, b={b}, n={n}"
                );
            }
        }

        [Test]
        public void Multiplicativity_InSecondArgument()
        {
            // (a / m*n) = (a/m) * (a/n), при m,n нечётных
            var random = new Random(888);

            for (int i = 0; i < 500; i++)
            {
                uint m = (uint)(random.Next(1, 50) * 2 + 1);
                uint n = (uint)(random.Next(1, 50) * 2 + 1);
                uint a = (uint)random.Next(0, 10_000);

                int left = RsaMath.CalcJacobiSymbol(a, m * n);
                int right = RsaMath.CalcJacobiSymbol(a, m)
                            * RsaMath.CalcJacobiSymbol(a, n);

                Assert.That(
                    left,
                    Is.EqualTo(right),
                    $"a={a}, m={m}, n={n}"
                );
            }
        }

        [Test]
        public void Zero_WhenNotCoprime()
        {
            // Если gcd(a, n) > 1, символ Якоби равен 0
            var random = new Random(999);

            for (int i = 0; i < 500; i++)
            {
                uint n = (uint)(random.Next(2, 100) * 2 + 1);
                uint a = n * (uint)random.Next(1, 10);

                int actual = RsaMath.CalcJacobiSymbol(a, n);

                Assert.That(
                    actual,
                    Is.EqualTo(0),
                    $"a={a}, n={n}"
                );
            }
        }

        [Test]
        public void ModulusZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() =>
                RsaMath.CalcJacobiSymbol(2, 0)
            );
        }

        // ---- Вспомогательные функции ----

        // Прямое вычисление символа Якоби через факторизацию n
        // и символ Лежандра для каждого простого множителя.
        private static int JacobiByDefinition(uint a, uint n)
        {
            if (n == 0 || n % 2 == 0)
                throw new ArgumentException("n должно быть положительным нечётным");

            uint m = n;
            int result = 1;

            for (uint p = 3; p * p <= m; p += 2)
            {
                while (m % p == 0)
                {
                    result *= LegendreByEuler(a, p);
                    m /= p;
                }
            }

            if (m > 1)
                result *= LegendreByEuler(a, m);

            return result;
        }

        private static int LegendreByEuler(uint a, uint p)
        {
            uint mod = a % p;
            if (mod == 0)
                return 0;

            uint pow = (uint)BigInteger.ModPow(a, (p - 1) / 2, p);
            return pow == p - 1 ? -1 : (int)pow;
        }
    }

    [TestFixture]
    public class EuclidAlgorithmTests
    {
        private Random _random;

        [SetUp]
        public void SetUp()
        {
            _random = new Random(12345);
        }

        [Test]
        public void BothZero_ReturnsZero()
        {
            Assert.That(RsaMath.EuclidAlgorithm(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void FirstZero_ReturnsSecond()
        {
            Assert.That(RsaMath.EuclidAlgorithm(0, 1), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(0, 5), Is.EqualTo(5));
            Assert.That(RsaMath.EuclidAlgorithm(0, 123), Is.EqualTo(123));
        }

        [Test]
        public void SecondZero_ReturnsFirst()
        {
            Assert.That(RsaMath.EuclidAlgorithm(1, 0), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(5, 0), Is.EqualTo(5));
            Assert.That(RsaMath.EuclidAlgorithm(123, 0), Is.EqualTo(123));
        }

        [Test]
        public void EqualValues_ReturnsSameValue()
        {
            Assert.That(RsaMath.EuclidAlgorithm(1, 1), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(7, 7), Is.EqualTo(7));
            Assert.That(RsaMath.EuclidAlgorithm(1000, 1000), Is.EqualTo(1000));
        }

        [Test]
        public void SimpleCases()
        {
            Assert.That(RsaMath.EuclidAlgorithm(48, 18), Is.EqualTo(6));
            Assert.That(RsaMath.EuclidAlgorithm(18, 48), Is.EqualTo(6));
            Assert.That(RsaMath.EuclidAlgorithm(54, 24), Is.EqualTo(6));
            Assert.That(RsaMath.EuclidAlgorithm(17, 5), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(100, 75), Is.EqualTo(25));
        }

        [Test]
        public void OneIsMultipleOfOther_ReturnsSmaller()
        {
            Assert.That(RsaMath.EuclidAlgorithm(10, 5), Is.EqualTo(5));
            Assert.That(RsaMath.EuclidAlgorithm(5, 10), Is.EqualTo(5));
            Assert.That(RsaMath.EuclidAlgorithm(100, 1), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(1, 100), Is.EqualTo(1));
        }

        [Test]
        public void ArgumentOrder_DoesNotMatter()
        {
            Assert.That(
                RsaMath.EuclidAlgorithm(48, 18),
                Is.EqualTo(RsaMath.EuclidAlgorithm(18, 48))
            );

            Assert.That(
                RsaMath.EuclidAlgorithm(123456, 7890),
                Is.EqualTo(RsaMath.EuclidAlgorithm(7890, 123456))
            );
        }

        [Test]
        public void CompareWithBigIntegerGreatestCommonDivisor_AllSmallValues()
        {
            for (uint a = 0; a <= 30; a++)
            {
                for (uint b = 0; b <= 30; b++)
                {
                    uint expected = (uint)BigInteger.GreatestCommonDivisor(a, b);
                    uint actual = RsaMath.EuclidAlgorithm(a, b);

                    Assert.That(
                        actual,
                        Is.EqualTo(expected),
                        $"a={a}, b={b}"
                    );
                }
            }
        }

        [Test]
        public void CompareWithBigIntegerGreatestCommonDivisor_RandomValues()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint a = (uint)_random.Next(0, int.MaxValue);
                uint b = (uint)_random.Next(0, int.MaxValue);

                uint expected = (uint)BigInteger.GreatestCommonDivisor(a, b);
                uint actual = RsaMath.EuclidAlgorithm(a, b);

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"a={a}, b={b}"
                );
            }
        }

        [Test]
        public void RandomValues_WithCommonFactor()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint factor = (uint)_random.Next(1, 1000);
                uint a = factor * (uint)_random.Next(1, 1000);
                uint b = factor * (uint)_random.Next(1, 1000);

                uint expected = (uint)BigInteger.GreatestCommonDivisor(a, b);
                uint actual = RsaMath.EuclidAlgorithm(a, b);

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"a={a}, b={b}, factor={factor}"
                );
            }
        }

        [Test]
        public void MaxValues_CompareWithBigIntegerGreatestCommonDivisor()
        {
            uint a = uint.MaxValue;
            uint b = uint.MaxValue - 1;

            uint expected = (uint)BigInteger.GreatestCommonDivisor(a, b);
            uint actual = RsaMath.EuclidAlgorithm(a, b);

            Assert.That(actual, Is.EqualTo(expected), $"a={a}, b={b}");
        }

        [Test]
        public void MaxValueAndZero_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            Assert.That(RsaMath.EuclidAlgorithm(a, 0), Is.EqualTo(a));
            Assert.That(RsaMath.EuclidAlgorithm(0, a), Is.EqualTo(a));
        }

        [Test]
        public void MaxValueAndOne_ReturnsOne()
        {
            Assert.That(RsaMath.EuclidAlgorithm(uint.MaxValue, 1), Is.EqualTo(1));
            Assert.That(RsaMath.EuclidAlgorithm(1, uint.MaxValue), Is.EqualTo(1));
        }

        [Test]
        public void MaxValueWithItself_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            Assert.That(RsaMath.EuclidAlgorithm(a, a), Is.EqualTo(a));
        }

        private static void AssertBezout(uint a, uint b, RsaMath.BezoutEquationCoefficients coeffs)
        {
            uint expectedGcd = (uint)BigInteger.GreatestCommonDivisor(a, b);
            Assert.That(
                coeffs.Gcd,
                Is.EqualTo(expectedGcd),
                $"Gcd для a={a}, b={b}"
            );

            BigInteger left = (BigInteger)a * coeffs.X + (BigInteger)b * coeffs.Y;
            Assert.That(
                left,
                Is.EqualTo((BigInteger)coeffs.Gcd),
                $"Тождество Безу для a={a}, b={b}: " +
                $"{a}*{coeffs.X} + {b}*{coeffs.Y} != {coeffs.Gcd}"
            );
        }

        [Test]
        public void ExtendedEuclid_BothZero_ReturnsGcdZero()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(0, 0);

            Assert.That(c.Gcd, Is.EqualTo(0));
            Assert.That(c.X, Is.EqualTo(1));
            Assert.That(c.Y, Is.EqualTo(0));
            AssertBezout(0, 0, c);
        }

        [Test]
        public void ExtendedEuclid_FirstZero_ReturnsSecondAsGcd()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(0, 5);

            Assert.That(c.Gcd, Is.EqualTo(5));
            Assert.That(c.X, Is.EqualTo(0));
            Assert.That(c.Y, Is.EqualTo(1));
            AssertBezout(0, 5, c);
        }

        [Test]
        public void ExtendedEuclid_SecondZero_ReturnsFirstAsGcd()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(5, 0);

            Assert.That(c.Gcd, Is.EqualTo(5));
            Assert.That(c.X, Is.EqualTo(1));
            Assert.That(c.Y, Is.EqualTo(0));
            AssertBezout(5, 0, c);
        }

        [Test]
        public void ExtendedEuclid_EqualValues_ReturnsSameValue()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(7, 7);

            Assert.That(c.Gcd, Is.EqualTo(7));
            Assert.That(c.X, Is.EqualTo(0));
            Assert.That(c.Y, Is.EqualTo(1));
            AssertBezout(7, 7, c);
        }

        [Test]
        public void ExtendedEuclid_SimpleCases_ExactCoefficients()
        {
            var cases = new (uint a, uint b, uint gcd, long x, long y)[]
            {
                (48, 18, 6, -1, 3),
                (18, 48, 6, 3, -1),
                (30, 21, 3, -2, 3),
                (17, 5, 1, -2, 7),
                (100, 75, 25, 1, -1),
            };

            foreach (var (a, b, gcd, x, y) in cases)
            {
                var c = RsaMath.ExtendedEuclidAlgorithm(a, b);

                Assert.That(c.Gcd, Is.EqualTo(gcd), $"Gcd для {a}, {b}");
                Assert.That(c.X, Is.EqualTo(x), $"X для {a}, {b}");
                Assert.That(c.Y, Is.EqualTo(y), $"Y для {a}, {b}");
                AssertBezout(a, b, c);
            }
        }

        [Test]
        public void ExtendedEuclid_ArgumentOrder_GcdSame_IdentityHolds()
        {
            var c1 = RsaMath.ExtendedEuclidAlgorithm(48, 18);
            var c2 = RsaMath.ExtendedEuclidAlgorithm(18, 48);

            Assert.That(c1.Gcd, Is.EqualTo(c2.Gcd));
            AssertBezout(48, 18, c1);
            AssertBezout(18, 48, c2);
        }

        [Test]
        public void ExtendedEuclid_CompareWithBigIntegerGcd_AllSmallValues()
        {
            for (uint a = 0; a <= 30; a++)
            {
                for (uint b = 0; b <= 30; b++)
                {
                    var c = RsaMath.ExtendedEuclidAlgorithm(a, b);
                    AssertBezout(a, b, c);
                }
            }
        }

        [Test]
        public void ExtendedEuclid_CompareWithBigIntegerGcd_RandomValues()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint a = (uint)_random.Next(0, int.MaxValue);
                uint b = (uint)_random.Next(0, int.MaxValue);

                var c = RsaMath.ExtendedEuclidAlgorithm(a, b);
                AssertBezout(a, b, c);
            }
        }

        [Test]
        public void ExtendedEuclid_RandomValues_WithCommonFactor()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint factor = (uint)_random.Next(1, 1000);
                uint a = factor * (uint)_random.Next(1, 1000);
                uint b = factor * (uint)_random.Next(1, 1000);

                var c = RsaMath.ExtendedEuclidAlgorithm(a, b);

                AssertBezout(a, b, c);
                Assert.That(
                    c.Gcd % factor,
                    Is.EqualTo(0),
                    $"Gcd должен делиться на общий множитель factor={factor}"
                );
            }
        }

        [Test]
        public void ExtendedEuclid_MaxValues_CompareWithBigIntegerGcd()
        {
            uint a = uint.MaxValue;
            uint b = uint.MaxValue - 1;

            var c = RsaMath.ExtendedEuclidAlgorithm(a, b);
            AssertBezout(a, b, c);
        }

        [Test]
        public void ExtendedEuclid_MaxValueAndZero_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            var c1 = RsaMath.ExtendedEuclidAlgorithm(a, 0);
            Assert.That(c1.Gcd, Is.EqualTo(a));
            Assert.That(c1.X, Is.EqualTo(1));
            Assert.That(c1.Y, Is.EqualTo(0));
            AssertBezout(a, 0, c1);

            var c2 = RsaMath.ExtendedEuclidAlgorithm(0, a);
            Assert.That(c2.Gcd, Is.EqualTo(a));
            Assert.That(c2.X, Is.EqualTo(0));
            Assert.That(c2.Y, Is.EqualTo(1));
            AssertBezout(0, a, c2);
        }

        [Test]
        public void ExtendedEuclid_MaxValueAndOne_ReturnsOne()
        {
            uint a = uint.MaxValue;

            var c1 = RsaMath.ExtendedEuclidAlgorithm(a, 1);
            Assert.That(c1.Gcd, Is.EqualTo(1));
            AssertBezout(a, 1, c1);

            var c2 = RsaMath.ExtendedEuclidAlgorithm(1, a);
            Assert.That(c2.Gcd, Is.EqualTo(1));
            AssertBezout(1, a, c2);
        }

        [Test]
        public void ExtendedEuclid_MaxValueWithItself_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            var c = RsaMath.ExtendedEuclidAlgorithm(a, a);

            Assert.That(c.Gcd, Is.EqualTo(a));
            Assert.That(c.X, Is.EqualTo(0));
            Assert.That(c.Y, Is.EqualTo(1));
            AssertBezout(a, a, c);
        }

        private static void AssertBinaryGcd(uint a, uint b, uint actualGcd)
        {
            uint expectedGcd = (uint)BigInteger.GreatestCommonDivisor(a, b);
            Assert.That(
                actualGcd,
                Is.EqualTo(expectedGcd),
                $"Gcd для a={a}, b={b}"
            );
        }

        [Test]
        public void BinaryEuclid_BothZero_ReturnsZero()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(0, 0);

            Assert.That(gcd, Is.EqualTo(0));
            AssertBinaryGcd(0, 0, gcd);
        }

        [Test]
        public void BinaryEuclid_FirstZero_ReturnsSecond()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(0, 5);

            Assert.That(gcd, Is.EqualTo(5));
            AssertBinaryGcd(0, 5, gcd);
        }

        [Test]
        public void BinaryEuclid_SecondZero_ReturnsFirst()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(5, 0);

            Assert.That(gcd, Is.EqualTo(5));
            AssertBinaryGcd(5, 0, gcd);
        }

        [Test]
        public void BinaryEuclid_EqualValues_ReturnsSameValue()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(7, 7);

            Assert.That(gcd, Is.EqualTo(7));
            AssertBinaryGcd(7, 7, gcd);
        }

        [Test]
        public void BinaryEuclid_SimpleCases_ExactGcd()
        {
            var cases = new (uint a, uint b, uint gcd)[]
            {
                (48, 18, 6),
                (18, 48, 6),
                (30, 21, 3),
                (17, 5, 1),
                (100, 75, 25),
                (12, 8, 4),
                (270, 192, 6),
                (1071, 462, 21),
            };

            foreach (var (a, b, expectedGcd) in cases)
            {
                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);

                Assert.That(gcd, Is.EqualTo(expectedGcd), $"Gcd для {a}, {b}");
                AssertBinaryGcd(a, b, gcd);
            }
        }

        [Test]
        public void BinaryEuclid_ArgumentOrder_GcdSame()
        {
            var gcd1 = RsaMath.BinaryEuclidAlgorithm(48, 18);
            var gcd2 = RsaMath.BinaryEuclidAlgorithm(18, 48);

            Assert.That(gcd1, Is.EqualTo(gcd2));
            AssertBinaryGcd(48, 18, gcd1);
            AssertBinaryGcd(18, 48, gcd2);
        }

        [Test]
        public void BinaryEuclid_CompareWithBigIntegerGcd_AllSmallValues()
        {
            for (uint a = 0; a <= 30; a++)
            {
                for (uint b = 0; b <= 30; b++)
                {
                    var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);
                    AssertBinaryGcd(a, b, gcd);
                }
            }
        }

        [Test]
        public void BinaryEuclid_CompareWithBigIntegerGcd_RandomValues()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint a = (uint)_random.Next(0, int.MaxValue);
                uint b = (uint)_random.Next(0, int.MaxValue);

                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);
                AssertBinaryGcd(a, b, gcd);
            }
        }

        [Test]
        public void BinaryEuclid_RandomValues_WithCommonFactor()
        {
            for (int i = 0; i < 1000; i++)
            {
                uint factor = (uint)_random.Next(1, 1000);
                uint a = factor * (uint)_random.Next(1, 1000);
                uint b = factor * (uint)_random.Next(1, 1000);

                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);

                AssertBinaryGcd(a, b, gcd);
                Assert.That(
                    gcd % factor,
                    Is.EqualTo(0),
                    $"Gcd должен делиться на общий множитель factor={factor}"
                );
            }
        }

        [Test]
        public void BinaryEuclid_MaxValues_CompareWithBigIntegerGcd()
        {
            uint a = uint.MaxValue;
            uint b = uint.MaxValue - 1;

            var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);
            AssertBinaryGcd(a, b, gcd);
        }

        [Test]
        public void BinaryEuclid_MaxValueAndZero_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            var gcd1 = RsaMath.BinaryEuclidAlgorithm(a, 0);
            Assert.That(gcd1, Is.EqualTo(a));
            AssertBinaryGcd(a, 0, gcd1);

            var gcd2 = RsaMath.BinaryEuclidAlgorithm(0, a);
            Assert.That(gcd2, Is.EqualTo(a));
            AssertBinaryGcd(0, a, gcd2);
        }

        [Test]
        public void BinaryEuclid_MaxValueAndOne_ReturnsOne()
        {
            uint a = uint.MaxValue;

            var gcd1 = RsaMath.BinaryEuclidAlgorithm(a, 1);
            Assert.That(gcd1, Is.EqualTo(1));
            AssertBinaryGcd(a, 1, gcd1);

            var gcd2 = RsaMath.BinaryEuclidAlgorithm(1, a);
            Assert.That(gcd2, Is.EqualTo(1));
            AssertBinaryGcd(1, a, gcd2);
        }

        [Test]
        public void BinaryEuclid_MaxValueWithItself_ReturnsMaxValue()
        {
            uint a = uint.MaxValue;

            var gcd = RsaMath.BinaryEuclidAlgorithm(a, a);

            Assert.That(gcd, Is.EqualTo(a));
            AssertBinaryGcd(a, a, gcd);
        }

        [Test]
        public void BinaryEuclid_PowersOfTwo_ReturnsSmallestPower()
        {
            // Классический кейс для бинарного алгоритма: общие множители 2
            var cases = new (uint a, uint b, uint gcd)[]
            {
                (8, 4, 4),
                (16, 24, 8),
                (1024, 512, 512),
                (2, 4, 2),
                (1u << 31, 1u << 30, 1u << 30),
            };

            foreach (var (a, b, expectedGcd) in cases)
            {
                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);

                Assert.That(gcd, Is.EqualTo(expectedGcd), $"Gcd для {a}, {b}");
                AssertBinaryGcd(a, b, gcd);
            }
        }

        [TestFixture]
        public class EulerFunctionTests
        {
            private Random _random;

            [SetUp]
            public void SetUp()
            {
                _random = new Random(54321);
            }

            private static uint EulerPhiReference(uint n)
            {
                uint result = n;
                uint m = n;

                if (m % 2 == 0)
                {
                    while (m % 2 == 0) m /= 2;
                    result -= result / 2;
                }

                for (uint p = 3; (ulong)p * p <= m; p += 2)
                {
                    if (m % p == 0)
                    {
                        while (m % p == 0) m /= p;
                        result -= result / p;
                    }
                }

                if (m > 1)
                    result -= result / m;

                return result;
            }

            private static bool IsPrime(uint n)
            {
                if (n < 2) return false;
                if (n == 2) return true;
                if (n % 2 == 0) return false;
                for (uint d = 3; (ulong)d * d <= n; d += 2)
                    if (n % d == 0)
                        return false;
                return true;
            }

            // =====================================================================
            // CalcFunctionEulerByDpf
            // =====================================================================

            [Test]
            public void Dpf_One_ReturnsOne()
            {
                Assert.That(
                    RsaMath.CalcFunctionEulerByDpf(1),
                    Is.EqualTo(1.0).Within(1e-9)
                );
            }

            [Test]
            public void Dpf_AllSmallValues_MatchReference()
            {
                for (uint n = 1; n <= 200; n++)
                {
                    double actual = RsaMath.CalcFunctionEulerByDpf(n);
                    double expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected).Within(1e-9), $"n={n}");
                }
            }

            [Test]
            public void Dpf_RandomValues_MatchReference()
            {
                for (int i = 0; i < 200; i++)
                {
                    uint n = (uint)_random.Next(1, 5000);
                    double actual = RsaMath.CalcFunctionEulerByDpf(n);
                    double expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected).Within(1e-6), $"n={n}");
                }
            }

            [Test]
            public void Dpf_Primes_ReturnPminusOne()
            {
                uint[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101, 997 };
                foreach (uint p in primes)
                {
                    double actual = RsaMath.CalcFunctionEulerByDpf(p);
                    Assert.That(actual, Is.EqualTo((double)(p - 1)).Within(1e-9), $"p={p}");
                }
            }

            [Test]
            public void Dpf_PowersOfTwo_ReturnHalf()
            {
                for (int k = 1; k <= 15; k++)
                {
                    uint n = 1u << k;
                    double actual = RsaMath.CalcFunctionEulerByDpf(n);
                    Assert.That(actual, Is.EqualTo(n / 2.0).Within(1e-6), $"n={n}");
                }
            }

            [Test]
            public void Dpf_ResultIsIntegerWithinTolerance()
            {
                for (uint n = 1; n <= 100; n++)
                {
                    double actual = RsaMath.CalcFunctionEulerByDpf(n);
                    double rounded = Math.Round(actual);
                    Assert.That(actual, Is.EqualTo(rounded).Within(1e-6), $"n={n}");
                }
            }

            // =====================================================================
            // CalcFunctionEulerByFactorization
            // =====================================================================

            [Test]
            public void FactorizationEuler_One_ReturnsOne()
            {
                Assert.That(
                    RsaMath.CalcFunctionEulerByFactorization(1),
                    Is.EqualTo(1)
                );
            }

            [Test]
            public void FactorizationEuler_AllSmallValues_MatchReference()
            {
                for (uint n = 1; n <= 1000; n++)
                {
                    uint actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    uint expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_RandomValues_MatchReference()
            {
                for (int i = 0; i < 2000; i++)
                {
                    uint n = (uint)_random.Next(1, 1_000_000);
                    uint actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    uint expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_Primes_ReturnPminusOne()
            {
                uint[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101, 997, 7919 };
                foreach (uint p in primes)
                {
                    uint actual = RsaMath.CalcFunctionEulerByFactorization(p);
                    Assert.That(actual, Is.EqualTo(p - 1), $"p={p}");
                }
            }

            [Test]
            public void FactorizationEuler_KnownValues()
            {
                var cases = new (uint n, uint expected)[]
                {
                    (1, 1),
                    (2, 1),
                    (4, 2),
                    (8, 4),
                    (12, 4),
                    (36, 12),
                    (100, 40),
                    (360, 96),
                    (1000, 400),
                    (65536, 32768),
                };

                foreach (var (n, expected) in cases)
                {
                    uint actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_PowersOfPrime_UsePhiOfPowerFormula()
            {
                var cases = new (uint n, uint expected)[]
                {
                    (4, 2), (8, 4), (16, 8), (32, 16), (1024, 512),
                    (9, 6), (27, 18), (81, 54),
                    (25, 20), (125, 100),
                };

                foreach (var (n, expected) in cases)
                {
                    uint actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            // =====================================================================
            // CalcFunctionEulerSumUnit
            // =====================================================================

            [Test]
            public void SumUnit_AllSmallValues_MatchReference()
            {
                for (uint n = 2; n <= 200; n++)
                {
                    uint actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    uint expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void SumUnit_RandomValues_MatchReference()
            {
                for (int i = 0; i < 300; i++)
                {
                    uint n = (uint)_random.Next(2, 5000);
                    uint actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    uint expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void SumUnit_Primes_ReturnPminusOne()
            {
                uint[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101 };
                foreach (uint p in primes)
                {
                    uint actual = RsaMath.CalcFunctionEulerSumUnit(p);
                    Assert.That(actual, Is.EqualTo(p - 1), $"p={p}");
                }
            }

            [Test]
            public void SumUnit_KnownValues()
            {
                var cases = new (uint n, uint expected)[]
                {
                    (2, 1),
                    (3, 2),
                    (4, 2),
                    (5, 4),
                    (6, 2),
                    (10, 4),
                    (12, 4),
                    (100, 40),
                };

                foreach (var (n, expected) in cases)
                {
                    uint actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            // =====================================================================
            // CalcFactorization
            // =====================================================================

            [Test]
            public void Factor_One_ReturnsEmpty()
            {
                var result = RsaMath.CalcFactorization(1);
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void Factor_Primes_ReturnSingleEntryPowerOne()
            {
                uint[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 97, 101, 9973 };
                foreach (uint p in primes)
                {
                    var result = RsaMath.CalcFactorization(p);
                    Assert.That(result.Count, Is.EqualTo(1), $"p={p}");
                    Assert.That(result[0].Number, Is.EqualTo(p), $"p={p}");
                    Assert.That(result[0].Power, Is.EqualTo(1), $"p={p}");
                }
            }

            [Test]
            public void Factor_KnownValues_ExactExpected()
            {
                var cases = new (uint n, (uint p, uint e)[] expected)[]
                {
                    (4, new[] { (2u, 2u) }),
                    (8, new[] { (2u, 3u) }),
                    (12, new[] { (2u, 2u), (3u, 1u) }),
                    (36, new[] { (2u, 2u), (3u, 2u) }),
                    (100, new[] { (2u, 2u), (5u, 2u) }),
                    (360, new[] { (2u, 3u), (3u, 2u), (5u, 1u) }),
                    (1000, new[] { (2u, 3u), (5u, 3u) }),
                    (1024, new[] { (2u, 10u) }),
                };

                foreach (var (n, expected) in cases)
                {
                    var result = RsaMath.CalcFactorization(n);

                    Assert.That(result.Count, Is.EqualTo(expected.Length), $"Count for n={n}");

                    for (int i = 0; i < expected.Length; i++)
                    {
                        Assert.That(result[i].Number, Is.EqualTo(expected[i].p), $"p[{i}] for n={n}");
                        Assert.That(result[i].Power, Is.EqualTo(expected[i].e), $"e[{i}] for n={n}");
                    }
                }
            }

            [Test]
            public void Factor_AllSmallValues_ProductReconstructsNumber()
            {
                for (uint n = 2; n <= 3000; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);

                    uint product = 1;
                    foreach (var np in factors)
                        for (uint k = 0; k < np.Power; k++)
                            product *= np.Number;

                    Assert.That(product, Is.EqualTo(n), $"Reconstruct n={n}");
                }
            }

            [Test]
            public void Factor_RandomValues_ProductReconstructsNumber()
            {
                for (int i = 0; i < 500; i++)
                {
                    uint n = (uint)_random.Next(1, 1_000_000);
                    var factors = RsaMath.CalcFactorization(n);

                    uint product = 1;
                    foreach (var np in factors)
                        for (uint k = 0; k < np.Power; k++)
                            product *= np.Number;

                    Assert.That(product, Is.EqualTo(n), $"Reconstruct n={n}");
                }
            }

            [Test]
            public void Factor_AllFactorsArePrime()
            {
                for (uint n = 2; n <= 2000; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);
                    foreach (var np in factors)
                    {
                        Assert.That(
                            IsPrime(np.Number),
                            Is.True,
                            $"Factor {np.Number} of {n} is not prime"
                        );
                    }
                }
            }

            [Test]
            public void Factor_SortedAscending()
            {
                for (uint n = 2; n <= 2000; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);
                    for (int i = 1; i < factors.Count; i++)
                    {
                        Assert.That(
                            factors[i].Number,
                            Is.GreaterThan(factors[i - 1].Number),
                            $"Not sorted for n={n}"
                        );
                    }
                }
            }

            [Test]
            public void Factor_PowersMatchDirectCount()
            {
                for (uint n = 2; n <= 500; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);
                    foreach (var np in factors)
                    {
                        uint count = 0;
                        uint m = n;
                        while (m % np.Number == 0)
                        {
                            count++;
                            m /= np.Number;
                        }

                        Assert.That(
                            np.Power,
                            Is.EqualTo(count),
                            $"Power mismatch for p={np.Number}, n={n}"
                        );
                    }
                }
            }
        }
    }
}