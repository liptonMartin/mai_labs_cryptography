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
            Assert.That(RsaMath.FastBinaryPowModule(2, 0, 13), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.FastBinaryPowModule(123, 0, 1000), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.FastBinaryPowModule(0, 0, 13), Is.EqualTo(BigInteger.One));
        }

        [Test]
        public void BaseZero_PositiveExponent_ReturnsZero()
        {
            Assert.That(RsaMath.FastBinaryPowModule(0, 1, 13), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.FastBinaryPowModule(0, 10, 13), Is.EqualTo(BigInteger.Zero));
        }

        [Test]
        public void SimpleCases()
        {
            Assert.That(RsaMath.FastBinaryPowModule(2, 10, 1000), Is.EqualTo(new BigInteger(24)));
            Assert.That(RsaMath.FastBinaryPowModule(3, 4, 5), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.FastBinaryPowModule(5, 3, 13), Is.EqualTo(new BigInteger(8)));
        }

        [Test]
        public void ModulusOne_ReturnsZero()
        {
            Assert.That(RsaMath.FastBinaryPowModule(2, 0, 1), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.FastBinaryPowModule(2, 1, 1), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.FastBinaryPowModule(2, 2, 1), Is.EqualTo(BigInteger.Zero));
        }

        [Test]
        public void CompareWithBigIntegerModPow_AllSmallValues()
        {
            for (BigInteger a = 0; a <= 30; a++)
            {
                for (BigInteger b = 0; b <= 30; b++)
                {
                    for (BigInteger mod = 1; mod <= 30; mod++)
                    {
                        BigInteger expected = BigInteger.ModPow(a, b, mod);
                        BigInteger actual = RsaMath.FastBinaryPowModule(a, b, mod);

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
                BigInteger a = _random.Next(0, int.MaxValue);
                BigInteger b = _random.Next(0, 1000);
                BigInteger mod = _random.Next(1, int.MaxValue);

                BigInteger expected = BigInteger.ModPow(a, b, mod);
                BigInteger actual = RsaMath.FastBinaryPowModule(a, b, mod);

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
            BigInteger a = uint.MaxValue;
            BigInteger b = 2;
            BigInteger mod = uint.MaxValue;

            BigInteger expected = BigInteger.ModPow(a, b, mod);
            BigInteger actual = RsaMath.FastBinaryPowModule(a, b, mod);

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
        [TestCase(8, 7, 1)]
        [TestCase(10, 7, -1)]
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
                RsaMath.CalcLegendreSymbol(a, p),
                Is.EqualTo(new BigInteger(expected))
            );
        }

        [Test]
        public void QuadraticResiduesAndNonResidues_ForSmallOddPrimes()
        {
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (int p in primes)
            {
                var residues = new HashSet<BigInteger>();

                for (BigInteger x = 0; x < p; x++)
                {
                    residues.Add((x * x) % p);
                }

                for (BigInteger a = 0; a < p; a++)
                {
                    BigInteger expected = a == 0
                        ? BigInteger.Zero
                        : residues.Contains(a)
                            ? BigInteger.One
                            : new BigInteger(-1);

                    BigInteger actual = RsaMath.CalcLegendreSymbol(a, p);

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
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (int p in primes)
            {
                for (BigInteger a = 0; a <= 50; a++)
                {
                    BigInteger expected = EulerCriterion(a, p);
                    BigInteger actual = RsaMath.CalcLegendreSymbol(a, p);

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

            int[] primes =
            {
                3, 5, 7, 11, 13, 17, 19, 23, 29, 31,
                37, 41, 43, 47, 53, 59, 61, 67, 71, 73,
                79, 83, 89, 97
            };

            for (int i = 0; i < 1000; i++)
            {
                BigInteger p = primes[random.Next(primes.Length)];
                BigInteger a = random.Next(0, int.MaxValue);

                BigInteger expected = EulerCriterion(a, p);
                BigInteger actual = RsaMath.CalcLegendreSymbol(a, p);

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

        private static BigInteger EulerCriterion(BigInteger a, BigInteger p)
        {
            BigInteger mod = a % p;
            if (mod == 0)
                return BigInteger.Zero;

            BigInteger pow = BigInteger.ModPow(a, (p - 1) / 2, p);
            return pow == p - 1 ? new BigInteger(-1) : pow;
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
                RsaMath.CalcJacobiSymbol(a, n),
                Is.EqualTo(new BigInteger(expected))
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
        [TestCase(0, 15, 0)]
        [TestCase(1, 15, 1)]
        [TestCase(2, 15, 1)]
        [TestCase(3, 15, 0)]
        [TestCase(4, 15, 1)]
        [TestCase(5, 15, 0)]
        [TestCase(6, 15, 0)]
        [TestCase(7, 15, -1)]
        [TestCase(8, 15, 1)]
        [TestCase(11, 15, -1)]
        [TestCase(13, 15, -1)]
        [TestCase(14, 15, -1)]
        [TestCase(0, 21, 0)]
        [TestCase(1, 21, 1)]
        [TestCase(2, 21, -1)]
        [TestCase(3, 21, 0)]
        [TestCase(4, 21, 1)]
        [TestCase(5, 21, 1)]
        [TestCase(7, 21, 0)]
        [TestCase(8, 21, -1)]
        [TestCase(10, 21, -1)]
        [TestCase(0, 25, 0)]
        [TestCase(1, 25, 1)]
        [TestCase(2, 25, 1)]
        [TestCase(3, 25, 1)]
        [TestCase(4, 25, 1)]
        [TestCase(5, 25, 0)]
        [TestCase(0, 27, 0)]
        [TestCase(1, 27, 1)]
        [TestCase(2, 27, -1)]
        [TestCase(3, 27, 0)]
        [TestCase(4, 27, 1)]
        [TestCase(8, 27, -1)]
        [TestCase(0, 45, 0)]
        [TestCase(1, 45, 1)]
        [TestCase(2, 45, -1)]
        [TestCase(4, 45, 1)]
        [TestCase(7, 45, -1)]
        public void KnownValues(int a, int n, int expected)
        {
            Assert.That(
                RsaMath.CalcJacobiSymbol(a, n),
                Is.EqualTo(new BigInteger(expected)),
                $"a={a}, n={n}"
            );
        }

        [Test]
        public void AgreementWithLegendre_ForPrimeModuli()
        {
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (int p in primes)
            {
                for (BigInteger a = 0; a <= 50; a++)
                {
                    BigInteger legendre = RsaMath.CalcLegendreSymbol(a, p);
                    BigInteger jacobi = RsaMath.CalcJacobiSymbol(a, p);

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
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };

            foreach (int p in primes)
            {
                for (BigInteger a = 0; a < p; a++)
                {
                    BigInteger legendre = RsaMath.CalcLegendreSymbol(a, p);
                    BigInteger jacobi = RsaMath.CalcJacobiSymbol(a, p);

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
            for (BigInteger n = 3; n <= 199; n += 2)
            {
                for (BigInteger a = 0; a <= 99; a++)
                {
                    BigInteger expected = JacobiByDefinition(a, n);
                    BigInteger actual = RsaMath.CalcJacobiSymbol(a, n);

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
                BigInteger n = random.Next(1, 5000) * 2 + 1;
                BigInteger a = random.Next(0, int.MaxValue);

                BigInteger expected = JacobiByDefinition(a, n);
                BigInteger actual = RsaMath.CalcJacobiSymbol(a, n);

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
            var random = new Random(777);

            for (int i = 0; i < 500; i++)
            {
                BigInteger n = random.Next(1, 500) * 2 + 1;
                BigInteger a = random.Next(0, 10_000);
                BigInteger b = random.Next(0, 10_000);

                BigInteger left = RsaMath.CalcJacobiSymbol(a * b, n);
                BigInteger right = RsaMath.CalcJacobiSymbol(a, n)
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
            var random = new Random(888);

            for (int i = 0; i < 500; i++)
            {
                BigInteger m = random.Next(1, 50) * 2 + 1;
                BigInteger n = random.Next(1, 50) * 2 + 1;
                BigInteger a = random.Next(0, 10_000);

                BigInteger left = RsaMath.CalcJacobiSymbol(a, m * n);
                BigInteger right = RsaMath.CalcJacobiSymbol(a, m)
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
            var random = new Random(999);

            for (int i = 0; i < 500; i++)
            {
                BigInteger n = random.Next(2, 100) * 2 + 1;
                BigInteger a = n * random.Next(1, 10);

                BigInteger actual = RsaMath.CalcJacobiSymbol(a, n);

                Assert.That(
                    actual,
                    Is.EqualTo(BigInteger.Zero),
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

        private static BigInteger JacobiByDefinition(BigInteger a, BigInteger n)
        {
            if (n == 0 || n % 2 == 0)
                throw new ArgumentException("n должно быть положительным нечётным");

            BigInteger m = n;
            BigInteger result = BigInteger.One;

            for (BigInteger p = 3; p * p <= m; p += 2)
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

        private static BigInteger LegendreByEuler(BigInteger a, BigInteger p)
        {
            BigInteger mod = a % p;
            if (mod == 0)
                return BigInteger.Zero;

            BigInteger pow = BigInteger.ModPow(a, (p - 1) / 2, p);
            return pow == p - 1 ? new BigInteger(-1) : pow;
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
            Assert.That(RsaMath.EuclidAlgorithm(0, 0), Is.EqualTo(BigInteger.Zero));
        }

        [Test]
        public void FirstZero_ReturnsSecond()
        {
            Assert.That(RsaMath.EuclidAlgorithm(0, 1), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(0, 5), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.EuclidAlgorithm(0, 123), Is.EqualTo(new BigInteger(123)));
        }

        [Test]
        public void SecondZero_ReturnsFirst()
        {
            Assert.That(RsaMath.EuclidAlgorithm(1, 0), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(5, 0), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.EuclidAlgorithm(123, 0), Is.EqualTo(new BigInteger(123)));
        }

        [Test]
        public void EqualValues_ReturnsSameValue()
        {
            Assert.That(RsaMath.EuclidAlgorithm(1, 1), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(7, 7), Is.EqualTo(new BigInteger(7)));
            Assert.That(RsaMath.EuclidAlgorithm(1000, 1000), Is.EqualTo(new BigInteger(1000)));
        }

        [Test]
        public void SimpleCases()
        {
            Assert.That(RsaMath.EuclidAlgorithm(48, 18), Is.EqualTo(new BigInteger(6)));
            Assert.That(RsaMath.EuclidAlgorithm(18, 48), Is.EqualTo(new BigInteger(6)));
            Assert.That(RsaMath.EuclidAlgorithm(54, 24), Is.EqualTo(new BigInteger(6)));
            Assert.That(RsaMath.EuclidAlgorithm(17, 5), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(100, 75), Is.EqualTo(new BigInteger(25)));
        }

        [Test]
        public void OneIsMultipleOfOther_ReturnsSmaller()
        {
            Assert.That(RsaMath.EuclidAlgorithm(10, 5), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.EuclidAlgorithm(5, 10), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.EuclidAlgorithm(100, 1), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(1, 100), Is.EqualTo(BigInteger.One));
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
            for (BigInteger a = 0; a <= 30; a++)
            {
                for (BigInteger b = 0; b <= 30; b++)
                {
                    BigInteger expected = BigInteger.GreatestCommonDivisor(a, b);
                    BigInteger actual = RsaMath.EuclidAlgorithm(a, b);

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
                BigInteger a = _random.Next(0, int.MaxValue);
                BigInteger b = _random.Next(0, int.MaxValue);

                BigInteger expected = BigInteger.GreatestCommonDivisor(a, b);
                BigInteger actual = RsaMath.EuclidAlgorithm(a, b);

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
                BigInteger factor = _random.Next(1, 1000);
                BigInteger a = factor * _random.Next(1, 1000);
                BigInteger b = factor * _random.Next(1, 1000);

                BigInteger expected = BigInteger.GreatestCommonDivisor(a, b);
                BigInteger actual = RsaMath.EuclidAlgorithm(a, b);

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
            BigInteger a = uint.MaxValue;
            BigInteger b = uint.MaxValue - 1;

            BigInteger expected = BigInteger.GreatestCommonDivisor(a, b);
            BigInteger actual = RsaMath.EuclidAlgorithm(a, b);

            Assert.That(actual, Is.EqualTo(expected), $"a={a}, b={b}");
        }

        [Test]
        public void MaxValueAndZero_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

            Assert.That(RsaMath.EuclidAlgorithm(a, 0), Is.EqualTo(a));
            Assert.That(RsaMath.EuclidAlgorithm(0, a), Is.EqualTo(a));
        }

        [Test]
        public void MaxValueAndOne_ReturnsOne()
        {
            Assert.That(RsaMath.EuclidAlgorithm(uint.MaxValue, 1), Is.EqualTo(BigInteger.One));
            Assert.That(RsaMath.EuclidAlgorithm(1, uint.MaxValue), Is.EqualTo(BigInteger.One));
        }

        [Test]
        public void MaxValueWithItself_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

            Assert.That(RsaMath.EuclidAlgorithm(a, a), Is.EqualTo(a));
        }

        private static void AssertBezout(BigInteger a, BigInteger b, RsaMath.BezoutEquationCoefficients coeffs)
        {
            BigInteger expectedGcd = BigInteger.GreatestCommonDivisor(a, b);
            Assert.That(
                coeffs.Gcd,
                Is.EqualTo(expectedGcd),
                $"Gcd для a={a}, b={b}"
            );

            BigInteger left = a * coeffs.X + b * coeffs.Y;
            Assert.That(
                left,
                Is.EqualTo(coeffs.Gcd),
                $"Тождество Безу для a={a}, b={b}: " +
                $"{a}*{coeffs.X} + {b}*{coeffs.Y} != {coeffs.Gcd}"
            );
        }

        [Test]
        public void ExtendedEuclid_BothZero_ReturnsGcdZero()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(0, 0);

            Assert.That(c.Gcd, Is.EqualTo(BigInteger.Zero));
            Assert.That(c.X, Is.EqualTo(BigInteger.One));
            Assert.That(c.Y, Is.EqualTo(BigInteger.Zero));
            AssertBezout(0, 0, c);
        }

        [Test]
        public void ExtendedEuclid_FirstZero_ReturnsSecondAsGcd()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(0, 5);

            Assert.That(c.Gcd, Is.EqualTo(new BigInteger(5)));
            Assert.That(c.X, Is.EqualTo(BigInteger.Zero));
            Assert.That(c.Y, Is.EqualTo(BigInteger.One));
            AssertBezout(0, 5, c);
        }

        [Test]
        public void ExtendedEuclid_SecondZero_ReturnsFirstAsGcd()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(5, 0);

            Assert.That(c.Gcd, Is.EqualTo(new BigInteger(5)));
            Assert.That(c.X, Is.EqualTo(BigInteger.One));
            Assert.That(c.Y, Is.EqualTo(BigInteger.Zero));
            AssertBezout(5, 0, c);
        }

        [Test]
        public void ExtendedEuclid_EqualValues_ReturnsSameValue()
        {
            var c = RsaMath.ExtendedEuclidAlgorithm(7, 7);

            Assert.That(c.Gcd, Is.EqualTo(new BigInteger(7)));
            Assert.That(c.X, Is.EqualTo(BigInteger.Zero));
            Assert.That(c.Y, Is.EqualTo(BigInteger.One));
            AssertBezout(7, 7, c);
        }

        [Test]
        public void ExtendedEuclid_SimpleCases_ExactCoefficients()
        {
            var cases = new (int a, int b, int gcd, int x, int y)[]
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

                Assert.That(c.Gcd, Is.EqualTo(new BigInteger(gcd)), $"Gcd для {a}, {b}");
                Assert.That(c.X, Is.EqualTo(new BigInteger(x)), $"X для {a}, {b}");
                Assert.That(c.Y, Is.EqualTo(new BigInteger(y)), $"Y для {a}, {b}");
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
            for (BigInteger a = 0; a <= 30; a++)
            {
                for (BigInteger b = 0; b <= 30; b++)
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
                BigInteger a = _random.Next(0, int.MaxValue);
                BigInteger b = _random.Next(0, int.MaxValue);

                var c = RsaMath.ExtendedEuclidAlgorithm(a, b);
                AssertBezout(a, b, c);
            }
        }

        [Test]
        public void ExtendedEuclid_RandomValues_WithCommonFactor()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger factor = _random.Next(1, 1000);
                BigInteger a = factor * _random.Next(1, 1000);
                BigInteger b = factor * _random.Next(1, 1000);

                var c = RsaMath.ExtendedEuclidAlgorithm(a, b);

                AssertBezout(a, b, c);
                Assert.That(
                    c.Gcd % factor,
                    Is.EqualTo(BigInteger.Zero),
                    $"Gcd должен делиться на общий множитель factor={factor}"
                );
            }
        }

        [Test]
        public void ExtendedEuclid_MaxValues_CompareWithBigIntegerGcd()
        {
            BigInteger a = uint.MaxValue;
            BigInteger b = uint.MaxValue - 1;

            var c = RsaMath.ExtendedEuclidAlgorithm(a, b);
            AssertBezout(a, b, c);
        }

        [Test]
        public void ExtendedEuclid_MaxValueAndZero_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

            var c1 = RsaMath.ExtendedEuclidAlgorithm(a, 0);
            Assert.That(c1.Gcd, Is.EqualTo(a));
            Assert.That(c1.X, Is.EqualTo(BigInteger.One));
            Assert.That(c1.Y, Is.EqualTo(BigInteger.Zero));
            AssertBezout(a, 0, c1);

            var c2 = RsaMath.ExtendedEuclidAlgorithm(0, a);
            Assert.That(c2.Gcd, Is.EqualTo(a));
            Assert.That(c2.X, Is.EqualTo(BigInteger.Zero));
            Assert.That(c2.Y, Is.EqualTo(BigInteger.One));
            AssertBezout(0, a, c2);
        }

        [Test]
        public void ExtendedEuclid_MaxValueAndOne_ReturnsOne()
        {
            BigInteger a = uint.MaxValue;

            var c1 = RsaMath.ExtendedEuclidAlgorithm(a, 1);
            Assert.That(c1.Gcd, Is.EqualTo(BigInteger.One));
            AssertBezout(a, 1, c1);

            var c2 = RsaMath.ExtendedEuclidAlgorithm(1, a);
            Assert.That(c2.Gcd, Is.EqualTo(BigInteger.One));
            AssertBezout(1, a, c2);
        }

        [Test]
        public void ExtendedEuclid_MaxValueWithItself_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

            var c = RsaMath.ExtendedEuclidAlgorithm(a, a);

            Assert.That(c.Gcd, Is.EqualTo(a));
            Assert.That(c.X, Is.EqualTo(BigInteger.Zero));
            Assert.That(c.Y, Is.EqualTo(BigInteger.One));
            AssertBezout(a, a, c);
        }

        private static void AssertBinaryGcd(BigInteger a, BigInteger b, BigInteger actualGcd)
        {
            BigInteger expectedGcd = BigInteger.GreatestCommonDivisor(a, b);
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

            Assert.That(gcd, Is.EqualTo(BigInteger.Zero));
            AssertBinaryGcd(0, 0, gcd);
        }

        [Test]
        public void BinaryEuclid_FirstZero_ReturnsSecond()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(0, 5);

            Assert.That(gcd, Is.EqualTo(new BigInteger(5)));
            AssertBinaryGcd(0, 5, gcd);
        }

        [Test]
        public void BinaryEuclid_SecondZero_ReturnsFirst()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(5, 0);

            Assert.That(gcd, Is.EqualTo(new BigInteger(5)));
            AssertBinaryGcd(5, 0, gcd);
        }

        [Test]
        public void BinaryEuclid_EqualValues_ReturnsSameValue()
        {
            var gcd = RsaMath.BinaryEuclidAlgorithm(7, 7);

            Assert.That(gcd, Is.EqualTo(new BigInteger(7)));
            AssertBinaryGcd(7, 7, gcd);
        }

        [Test]
        public void BinaryEuclid_SimpleCases_ExactGcd()
        {
            var cases = new (int a, int b, int gcd)[]
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

                Assert.That(gcd, Is.EqualTo(new BigInteger(expectedGcd)), $"Gcd для {a}, {b}");
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
            for (BigInteger a = 0; a <= 30; a++)
            {
                for (BigInteger b = 0; b <= 30; b++)
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
                BigInteger a = _random.Next(0, int.MaxValue);
                BigInteger b = _random.Next(0, int.MaxValue);

                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);
                AssertBinaryGcd(a, b, gcd);
            }
        }

        [Test]
        public void BinaryEuclid_RandomValues_WithCommonFactor()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger factor = _random.Next(1, 1000);
                BigInteger a = factor * _random.Next(1, 1000);
                BigInteger b = factor * _random.Next(1, 1000);

                var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);

                AssertBinaryGcd(a, b, gcd);
                Assert.That(
                    gcd % factor,
                    Is.EqualTo(BigInteger.Zero),
                    $"Gcd должен делиться на общий множитель factor={factor}"
                );
            }
        }

        [Test]
        public void BinaryEuclid_MaxValues_CompareWithBigIntegerGcd()
        {
            BigInteger a = uint.MaxValue;
            BigInteger b = uint.MaxValue - 1;

            var gcd = RsaMath.BinaryEuclidAlgorithm(a, b);
            AssertBinaryGcd(a, b, gcd);
        }

        [Test]
        public void BinaryEuclid_MaxValueAndZero_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

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
            BigInteger a = uint.MaxValue;

            var gcd1 = RsaMath.BinaryEuclidAlgorithm(a, 1);
            Assert.That(gcd1, Is.EqualTo(BigInteger.One));
            AssertBinaryGcd(a, 1, gcd1);

            var gcd2 = RsaMath.BinaryEuclidAlgorithm(1, a);
            Assert.That(gcd2, Is.EqualTo(BigInteger.One));
            AssertBinaryGcd(1, a, gcd2);
        }

        [Test]
        public void BinaryEuclid_MaxValueWithItself_ReturnsMaxValue()
        {
            BigInteger a = uint.MaxValue;

            var gcd = RsaMath.BinaryEuclidAlgorithm(a, a);

            Assert.That(gcd, Is.EqualTo(a));
            AssertBinaryGcd(a, a, gcd);
        }

        [Test]
        public void BinaryEuclid_PowersOfTwo_ReturnsSmallestPower()
        {
            var cases = new (BigInteger a, BigInteger b, BigInteger gcd)[]
            {
                (8, 4, 4),
                (16, 24, 8),
                (1024, 512, 512),
                (2, 4, 2),
                (BigInteger.One << 31, BigInteger.One << 30, BigInteger.One << 30),
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

            private static BigInteger EulerPhiReference(BigInteger n)
            {
                BigInteger result = n;
                BigInteger m = n;

                if (m % 2 == 0)
                {
                    while (m % 2 == 0) m /= 2;
                    result -= result / 2;
                }

                for (BigInteger p = 3; p * p <= m; p += 2)
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

            private static bool IsPrime(BigInteger n)
            {
                if (n < 2) return false;
                if (n == 2) return true;
                if (n % 2 == 0) return false;
                for (BigInteger d = 3; d * d <= n; d += 2)
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
                    Is.EqualTo(BigInteger.One)
                );
            }

            [Test]
            public void Dpf_AllSmallValues_MatchReference()
            {
                for (BigInteger n = 1; n <= 200; n++)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerByDpf(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void Dpf_RandomValues_MatchReference()
            {
                for (int i = 0; i < 200; i++)
                {
                    BigInteger n = _random.Next(1, 5000);
                    BigInteger actual = RsaMath.CalcFunctionEulerByDpf(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void Dpf_Primes_ReturnPminusOne()
            {
                int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101, 997 };
                foreach (int p in primes)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerByDpf(p);
                    Assert.That(actual, Is.EqualTo(new BigInteger(p - 1)), $"p={p}");
                }
            }

            [Test]
            public void Dpf_PowersOfTwo_ReturnHalf()
            {
                for (int k = 1; k <= 15; k++)
                {
                    BigInteger n = BigInteger.One << k;
                    BigInteger actual = RsaMath.CalcFunctionEulerByDpf(n);
                    Assert.That(actual, Is.EqualTo(n / 2), $"n={n}");
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
                    Is.EqualTo(BigInteger.One)
                );
            }

            [Test]
            public void FactorizationEuler_AllSmallValues_MatchReference()
            {
                for (BigInteger n = 1; n <= 1000; n++)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_RandomValues_MatchReference()
            {
                for (int i = 0; i < 2000; i++)
                {
                    BigInteger n = _random.Next(1, 1_000_000);
                    BigInteger actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_Primes_ReturnPminusOne()
            {
                int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101, 997, 7919 };
                foreach (int p in primes)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerByFactorization(p);
                    Assert.That(actual, Is.EqualTo(new BigInteger(p - 1)), $"p={p}");
                }
            }

            [Test]
            public void FactorizationEuler_KnownValues()
            {
                var cases = new (int n, int expected)[]
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
                    BigInteger actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    Assert.That(actual, Is.EqualTo(new BigInteger(expected)), $"n={n}");
                }
            }

            [Test]
            public void FactorizationEuler_PowersOfPrime_UsePhiOfPowerFormula()
            {
                var cases = new (int n, int expected)[]
                {
                    (4, 2), (8, 4), (16, 8), (32, 16), (1024, 512),
                    (9, 6), (27, 18), (81, 54),
                    (25, 20), (125, 100),
                };

                foreach (var (n, expected) in cases)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerByFactorization(n);
                    Assert.That(actual, Is.EqualTo(new BigInteger(expected)), $"n={n}");
                }
            }

            // =====================================================================
            // CalcFunctionEulerSumUnit
            // =====================================================================

            [Test]
            public void SumUnit_AllSmallValues_MatchReference()
            {
                for (BigInteger n = 2; n <= 200; n++)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void SumUnit_RandomValues_MatchReference()
            {
                for (int i = 0; i < 300; i++)
                {
                    BigInteger n = _random.Next(2, 5000);
                    BigInteger actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    BigInteger expected = EulerPhiReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n}");
                }
            }

            [Test]
            public void SumUnit_Primes_ReturnPminusOne()
            {
                int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 97, 101 };
                foreach (int p in primes)
                {
                    BigInteger actual = RsaMath.CalcFunctionEulerSumUnit(p);
                    Assert.That(actual, Is.EqualTo(new BigInteger(p - 1)), $"p={p}");
                }
            }

            [Test]
            public void SumUnit_KnownValues()
            {
                var cases = new (int n, int expected)[]
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
                    BigInteger actual = RsaMath.CalcFunctionEulerSumUnit(n);
                    Assert.That(actual, Is.EqualTo(new BigInteger(expected)), $"n={n}");
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
                int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 97, 101, 9973 };
                foreach (int p in primes)
                {
                    var result = RsaMath.CalcFactorization(p);
                    Assert.That(result.Count, Is.EqualTo(1), $"p={p}");
                    Assert.That(result[0].Number, Is.EqualTo(new BigInteger(p)), $"p={p}");
                    Assert.That(result[0].Power, Is.EqualTo(BigInteger.One), $"p={p}");
                }
            }

            [Test]
            public void Factor_KnownValues_ExactExpected()
            {
                var cases = new (int n, (int p, int e)[] expected)[]
                {
                    (4, new[] { (2, 2) }),
                    (8, new[] { (2, 3) }),
                    (12, new[] { (2, 2), (3, 1) }),
                    (36, new[] { (2, 2), (3, 2) }),
                    (100, new[] { (2, 2), (5, 2) }),
                    (360, new[] { (2, 3), (3, 2), (5, 1) }),
                    (1000, new[] { (2, 3), (5, 3) }),
                    (1024, new[] { (2, 10) }),
                };

                foreach (var (n, expected) in cases)
                {
                    var result = RsaMath.CalcFactorization(n);

                    Assert.That(result.Count, Is.EqualTo(expected.Length), $"Count for n={n}");

                    for (int i = 0; i < expected.Length; i++)
                    {
                        Assert.That(result[i].Number, Is.EqualTo(new BigInteger(expected[i].p)), $"p[{i}] for n={n}");
                        Assert.That(result[i].Power, Is.EqualTo(new BigInteger(expected[i].e)), $"e[{i}] for n={n}");
                    }
                }
            }

            [Test]
            public void Factor_AllSmallValues_ProductReconstructsNumber()
            {
                for (BigInteger n = 2; n <= 3000; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);

                    BigInteger product = BigInteger.One;
                    foreach (var np in factors)
                        for (BigInteger k = 0; k < np.Power; k++)
                            product *= np.Number;

                    Assert.That(product, Is.EqualTo(n), $"Reconstruct n={n}");
                }
            }

            [Test]
            public void Factor_RandomValues_ProductReconstructsNumber()
            {
                for (int i = 0; i < 500; i++)
                {
                    BigInteger n = _random.Next(1, 1_000_000);
                    var factors = RsaMath.CalcFactorization(n);

                    BigInteger product = BigInteger.One;
                    foreach (var np in factors)
                        for (BigInteger k = 0; k < np.Power; k++)
                            product *= np.Number;

                    Assert.That(product, Is.EqualTo(n), $"Reconstruct n={n}");
                }
            }

            [Test]
            public void Factor_AllFactorsArePrime()
            {
                for (BigInteger n = 2; n <= 2000; n++)
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
                for (BigInteger n = 2; n <= 2000; n++)
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
                for (BigInteger n = 2; n <= 500; n++)
                {
                    var factors = RsaMath.CalcFactorization(n);
                    foreach (var np in factors)
                    {
                        BigInteger count = 0;
                        BigInteger m = n;
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

    [TestFixture]
    public class LcmTests
    {
        private Random _random;

        [SetUp]
        public void SetUp()
        {
            _random = new Random(11111);
        }

        [Test]
        public void ZeroFirst_ReturnsZero()
        {
            Assert.That(RsaMath.CalcLcm(0, 5), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.CalcLcm(0, 1), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.CalcLcm(0, uint.MaxValue), Is.EqualTo(BigInteger.Zero));
        }

        [Test]
        public void ZeroSecond_ReturnsZero()
        {
            Assert.That(RsaMath.CalcLcm(5, 0), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.CalcLcm(1, 0), Is.EqualTo(BigInteger.Zero));
            Assert.That(RsaMath.CalcLcm(uint.MaxValue, 0), Is.EqualTo(BigInteger.Zero));
        }

        [Test]
        public void OneAndValue_ReturnsValue()
        {
            Assert.That(RsaMath.CalcLcm(1, 5), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.CalcLcm(5, 1), Is.EqualTo(new BigInteger(5)));
            Assert.That(RsaMath.CalcLcm(1, 1), Is.EqualTo(BigInteger.One));
        }

        [Test]
        public void SimpleCases()
        {
            Assert.That(RsaMath.CalcLcm(4, 6), Is.EqualTo(new BigInteger(12)));
            Assert.That(RsaMath.CalcLcm(6, 4), Is.EqualTo(new BigInteger(12)));
            Assert.That(RsaMath.CalcLcm(21, 6), Is.EqualTo(new BigInteger(42)));
            Assert.That(RsaMath.CalcLcm(8, 9), Is.EqualTo(new BigInteger(72)));
            Assert.That(RsaMath.CalcLcm(12, 15), Is.EqualTo(new BigInteger(60)));
        }

        [Test]
        public void OneIsMultipleOfOther_ReturnsLarger()
        {
            Assert.That(RsaMath.CalcLcm(5, 10), Is.EqualTo(new BigInteger(10)));
            Assert.That(RsaMath.CalcLcm(10, 5), Is.EqualTo(new BigInteger(10)));
            Assert.That(RsaMath.CalcLcm(3, 9), Is.EqualTo(new BigInteger(9)));
        }

        [Test]
        public void ArgumentOrder_DoesNotMatter()
        {
            Assert.That(
                RsaMath.CalcLcm(4, 6),
                Is.EqualTo(RsaMath.CalcLcm(6, 4))
            );
            Assert.That(
                RsaMath.CalcLcm(100, 75),
                Is.EqualTo(RsaMath.CalcLcm(75, 100))
            );
        }

        [Test]
        public void CompareWithFormulaABDivGcd_AllSmallValues()
        {
            for (BigInteger a = 1; a <= 50; a++)
            {
                for (BigInteger b = 1; b <= 50; b++)
                {
                    BigInteger gcd = RsaMath.EuclidAlgorithm(a, b);
                    BigInteger expected = a / gcd * b;
                    BigInteger actual = RsaMath.CalcLcm(a, b);

                    Assert.That(actual, Is.EqualTo(expected), $"a={a}, b={b}");
                }
            }
        }

        [Test]
        public void CompareWithFormulaABDivGcd_RandomValues()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger a = _random.Next(1, int.MaxValue);
                BigInteger b = _random.Next(1, int.MaxValue);

                BigInteger gcd = RsaMath.EuclidAlgorithm(a, b);
                BigInteger expected = a / gcd * b;
                BigInteger actual = RsaMath.CalcLcm(a, b);

                Assert.That(actual, Is.EqualTo(expected), $"a={a}, b={b}");
            }
        }

        [Test]
        public void ResultIsDivisibleByBothArguments()
        {
            for (int i = 0; i < 500; i++)
            {
                BigInteger a = _random.Next(1, 100_000);
                BigInteger b = _random.Next(1, 100_000);

                BigInteger lcm = RsaMath.CalcLcm(a, b);

                Assert.That(lcm % a, Is.EqualTo(BigInteger.Zero),
                    $"lcm({a},{b})={lcm} not divisible by {a}");
                Assert.That(lcm % b, Is.EqualTo(BigInteger.Zero),
                    $"lcm({a},{b})={lcm} not divisible by {b}");
            }
        }

        [Test]
        public void IsMinimalMultiple_NoSmallerMultipleExists()
        {
            for (BigInteger a = 1; a <= 40; a++)
            {
                for (BigInteger b = 1; b <= 40; b++)
                {
                    BigInteger lcm = RsaMath.CalcLcm(a, b);

                    for (BigInteger candidate = 1; candidate < lcm; candidate++)
                    {
                        Assert.That(
                            candidate % a != 0 || candidate % b != 0,
                            Is.True,
                            $"Найден меньший общий кратный {candidate} для ({a},{b}), lcm={lcm}"
                        );
                    }
                }
            }
        }
    }

    [TestFixture]
    public class CarmichaelFunctionTests
    {
        private Random _random;

        [SetUp]
        public void SetUp()
        {
            _random = new Random(24680);
        }

        // ---- Референсная реализация λ(n) ----

        private static BigInteger GcdReference(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                var t = b;
                b = a % b;
                a = t;
            }

            return a;
        }

        private static BigInteger LcmReference(BigInteger a, BigInteger b)
        {
            return a / GcdReference(a, b) * b;
        }

        private static BigInteger CarmichaelReference(BigInteger n)
        {
            if (n == 1)
                return BigInteger.One;

            BigInteger result = BigInteger.One;
            BigInteger m = n;

            if (m % 2 == 0)
            {
                int power = 0;
                while (m % 2 == 0)
                {
                    m /= 2;
                    power++;
                }

                BigInteger lambda2 = power switch
                {
                    1 => BigInteger.One,
                    2 => new BigInteger(2),
                    _ => BigInteger.One << (power - 2)
                };
                result = LcmReference(result, lambda2);
            }

            for (BigInteger p = 3; p * p <= m; p += 2)
            {
                if (m % p == 0)
                {
                    int power = 0;
                    while (m % p == 0)
                    {
                        m /= p;
                        power++;
                    }

                    BigInteger pPow = BigInteger.Pow(p, power - 1);
                    BigInteger lambdaP = pPow * (p - 1);
                    result = LcmReference(result, lambdaP);
                }
            }

            if (m > 1)
                result = LcmReference(result, m - 1);

            return result;
        }

        // =====================================================================
        // Базовые значения
        // =====================================================================

        [Test]
        public void One_ReturnsOne()
        {
            Assert.That(
                RsaMath.CalcFunctionCarmichaelByFactorization(1),
                Is.EqualTo(BigInteger.One)
            );
        }

        [Test]
        public void KnownValues()
        {
            var cases = new (int n, int expected)[]
            {
                (1, 1),
                (2, 1),
                (3, 2),
                (4, 2),
                (5, 4),
                (6, 2),
                (7, 6),
                (8, 2),
                (9, 6),
                (10, 4),
                (12, 2),
                (15, 4),
                (16, 4),
                (27, 18),
                (32, 8),
                (36, 6),
                (81, 54),
                (100, 20),
                (360, 12),
                (1000, 100),
            };

            foreach (var (n, expected) in cases)
            {
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                Assert.That(actual, Is.EqualTo(new BigInteger(expected)), $"n={n}");
            }
        }

        // =====================================================================
        // Простые, степени простых и степеней двойки
        // =====================================================================

        [Test]
        public void Primes_ReturnPminusOne()
        {
            int[] primes =
            {
                2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31,
                97, 101, 997, 7919
            };

            foreach (int p in primes)
            {
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(p);
                BigInteger expected = p == 2 ? BigInteger.One : new BigInteger(p - 1);

                Assert.That(actual, Is.EqualTo(expected), $"p={p}");
            }
        }

        [Test]
        public void PowersOfTwo_FollowCarmichaelRule()
        {
            var cases = new (BigInteger n, BigInteger expected)[]
            {
                (2, 1),
                (4, 2),
                (8, 2),
                (16, 4),
                (32, 8),
                (64, 16),
                (128, 32),
                (256, 64),
                (512, 128),
                (1024, 256),
                (2048, 512),
                (4096, 1024),
                (BigInteger.One << 20, BigInteger.One << 18),
                (BigInteger.One << 30, BigInteger.One << 28),
            };

            foreach (var (n, expected) in cases)
            {
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                Assert.That(actual, Is.EqualTo(expected), $"n={n}");
            }
        }

        [Test]
        public void PowersOfOddPrimes_UsePhiOfPowerFormula()
        {
            var cases = new (int n, int expected)[]
            {
                (3, 2),
                (9, 6),
                (27, 18),
                (81, 54),
                (243, 162),
                (5, 4),
                (25, 20),
                (125, 100),
                (7, 6),
                (49, 42),
                (11, 10),
                (121, 110),
            };

            foreach (var (n, expected) in cases)
            {
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                Assert.That(actual, Is.EqualTo(new BigInteger(expected)), $"n={n}");
            }
        }

        // =====================================================================
        // Сравнение с эталоном
        // =====================================================================

        [Test]
        public void AllSmallValues_MatchReference()
        {
            for (BigInteger n = 1; n <= 1000; n++)
            {
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger expected = CarmichaelReference(n);

                Assert.That(actual, Is.EqualTo(expected), $"n={n}");
            }
        }

        [Test]
        public void RandomValues_MatchReference()
        {
            for (int i = 0; i < 2000; i++)
            {
                BigInteger n = _random.Next(1, 1_000_000);

                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger expected = CarmichaelReference(n);

                Assert.That(actual, Is.EqualTo(expected), $"n={n}");
            }
        }

        [Test]
        public void LargePrimeSquares_MatchReference()
        {
            int[] primes = { 101, 211, 307, 401, 503, 601, 701, 31607 };
            foreach (int p in primes)
            {
                BigInteger n = (BigInteger)p * p;
                BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger expected = CarmichaelReference(n);

                Assert.That(actual, Is.EqualTo(expected), $"n={n} (= {p}^2)");
            }
        }

        [Test]
        public void Semiprimes_MatchReference()
        {
            int[] primes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 101, 997 };
            foreach (int p in primes)
            {
                foreach (int q in primes)
                {
                    BigInteger n = (BigInteger)p * q;
                    BigInteger actual = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                    BigInteger expected = CarmichaelReference(n);

                    Assert.That(actual, Is.EqualTo(expected), $"n={n} ({p}*{q})");
                }
            }
        }

        // =====================================================================
        // Свойства λ(n)
        // =====================================================================

        [Test]
        public void DividesEulerPhi()
        {
            for (BigInteger n = 1; n <= 500; n++)
            {
                BigInteger lambda = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger phi = RsaMath.CalcFunctionEulerByFactorization(n);

                Assert.That(
                    phi % lambda,
                    Is.EqualTo(BigInteger.Zero),
                    $"λ({n})={lambda} не делит φ({n})={phi}"
                );
            }
        }

        [Test]
        public void CarmichaelProperty_APowLambdaModNIsOne_ForCoprimeA()
        {
            for (BigInteger n = 2; n <= 200; n++)
            {
                BigInteger lambda = RsaMath.CalcFunctionCarmichaelByFactorization(n);

                for (BigInteger a = 1; a < n; a++)
                {
                    if (RsaMath.EuclidAlgorithm(a, n) != 1)
                        continue;

                    BigInteger pow = BigInteger.ModPow(a, lambda, n);

                    Assert.That(
                        pow,
                        Is.EqualTo(BigInteger.One),
                        $"a={a}, n={n}, λ={lambda}: a^λ mod n = {pow}"
                    );
                }
            }
        }

        [Test]
        public void LambdaIsSmallestSuchExponent_SmallN()
        {
            for (BigInteger n = 2; n <= 60; n++)
            {
                BigInteger lambda = RsaMath.CalcFunctionCarmichaelByFactorization(n);

                for (BigInteger e = 1; e < lambda; e++)
                {
                    bool allOnes = true;

                    for (BigInteger a = 1; a < n; a++)
                    {
                        if (RsaMath.EuclidAlgorithm(a, n) != 1)
                            continue;

                        BigInteger pow = BigInteger.ModPow(a, e, n);
                        if (pow != 1)
                        {
                            allOnes = false;
                            break;
                        }
                    }

                    Assert.That(
                        allOnes,
                        Is.False,
                        $"Для n={n} найден меньший показатель e={e} < λ={lambda}"
                    );
                }
            }
        }

        [Test]
        public void LambdaOfCoprimeProduct_EqualsLcmOfLambdas()
        {
            var pairs = new (int m, int n)[]
            {
                (3, 5),
                (4, 9),
                (8, 27),
                (25, 49),
                (5, 16),
                (7, 11),
                (13, 4),
            };

            foreach (var (m, n) in pairs)
            {
                BigInteger lambdaM = RsaMath.CalcFunctionCarmichaelByFactorization(m);
                BigInteger lambdaN = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger lambdaMN = RsaMath.CalcFunctionCarmichaelByFactorization((BigInteger)m * n);

                BigInteger expected = RsaMath.CalcLcm(lambdaM, lambdaN);

                Assert.That(
                    lambdaMN,
                    Is.EqualTo(expected),
                    $"λ({m}*{n}) = λ({(BigInteger)m * n}) не равно lcm(λ({m}), λ({n}))"
                );
            }
        }

        [Test]
        public void AlwaysPositive_ForAllSmallN()
        {
            for (BigInteger n = 1; n <= 2000; n++)
            {
                BigInteger lambda = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                Assert.That(lambda, Is.GreaterThan(BigInteger.Zero), $"λ({n}) = 0");
            }
        }

        [Test]
        public void NeverExceedsN_ForAllSmallN()
        {
            for (BigInteger n = 1; n <= 2000; n++)
            {
                BigInteger lambda = RsaMath.CalcFunctionCarmichaelByFactorization(n);
                BigInteger phi = RsaMath.CalcFunctionEulerByFactorization(n);

                Assert.That(
                    lambda,
                    Is.LessThanOrEqualTo(phi),
                    $"λ({n})={lambda} > φ({n})={phi}"
                );
            }
        }
    }
}