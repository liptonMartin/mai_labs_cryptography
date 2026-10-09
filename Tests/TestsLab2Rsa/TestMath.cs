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
}