using System.Numerics;
using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa;

[TestFixture]
public class TestKeyGenerator
{
    private KeyGenerator _keyGenerator;

    [SetUp]
    public void SetUp()
    {
        _keyGenerator = new KeyGenerator(PrimaryTestType.Fermat, 0.99, 8);
    }

    [Test]
    public void BaseTest()
    {
        var result = _keyGenerator.GenerateKeys();
        Assert.That(result.PublicKey.N, Is.GreaterThan(BigInteger.Zero));
    }
    
    // ---------- Корректность ключей ----------

    [TestCase(PrimaryTestType.Fermat)]
    [TestCase(PrimaryTestType.SolovayStrassen)]
    [TestCase(PrimaryTestType.MellerRabin)]
    public void GenerateKeys_ShouldReturnValidExponents_ForAllTestTypes(PrimaryTestType testType)
    {
        var generator = new KeyGenerator(testType, 0.99, 8);
        var keys = generator.GenerateKeys();

        var e = keys.PublicKey.Exp;
        var d = keys.PrivateKey.Exp;
        var n = keys.PublicKey.N;

        // открытый и закрытый ключи используют один и тот же модуль
        Assert.That(keys.PrivateKey.N, Is.EqualTo(n));

        // e и d должны быть положительными
        Assert.That(e, Is.GreaterThan(BigInteger.Zero));
        Assert.That(d, Is.GreaterThan(BigInteger.Zero));

        // e < n, d < n
        Assert.That(e, Is.LessThan(n));
        Assert.That(d, Is.LessThan(n));
    }

    [Test]
    public void GenerateKeys_EncryptionDecryption_ShouldReturnOriginalMessage()
    {
        var keys = _keyGenerator.GenerateKeys();
        var e = keys.PublicKey.Exp;
        var d = keys.PrivateKey.Exp;
        var n = keys.PublicKey.N;

        // берём сообщение, меньшее n
        var message = n / 2;

        var encrypted = BigInteger.ModPow(message, e, n);
        var decrypted = BigInteger.ModPow(encrypted, d, n);

        Assert.That(decrypted, Is.EqualTo(message));
    }

    [Test]
    public void GenerateKeys_EDModPhi_ShouldEqualOne()
    {
        var keys = _keyGenerator.GenerateKeys();
        var e = keys.PublicKey.Exp;
        var d = keys.PrivateKey.Exp;
        var n = keys.PublicKey.N;

        // Напрямую φ(n) мы не знаем, но можем проверить,
        // что для случайного m выполняется m^(e*d) ≡ m (mod n).
        // Это косвенная проверка e*d ≡ 1 (mod φ(n)).
        var message = n / 3;
        var result = BigInteger.ModPow(message, e * d, n);
        Assert.That(result, Is.EqualTo(message));
    }

    [Test]
    public void GenerateKeys_ShouldProduceDifferentKeys_OnRepeatedCalls()
    {
        var keys1 = _keyGenerator.GenerateKeys();
        var keys2 = _keyGenerator.GenerateKeys();

        // Чрезвычайно маловероятно, чтобы дважды получились одинаковые модули
        Assert.That(keys1.PublicKey.N, Is.Not.EqualTo(keys2.PublicKey.N));
    }

    [TestCase(8u)]
    [TestCase(16u)]
    [TestCase(32u)]
    public void GenerateKeys_ShouldWork_ForVariousBitLengths(uint bitLength)
    {
        var generator = new KeyGenerator(PrimaryTestType.MellerRabin, 0.99, bitLength);
        var keys = generator.GenerateKeys();

        // n = p * q, каждое из p и q имеет ~bitLength бит,
        // значит n имеет примерно 2*bitLength бит.
        var nBitLength = keys.PublicKey.N.GetBitLength();
        Assert.That(nBitLength, Is.GreaterThan(bitLength));
        Assert.That(nBitLength, Is.LessThanOrEqualTo(2 * bitLength));
    }

    [Test]
    public void GenerateKeys_N_ShouldBeComposite_AndGreaterThanOne()
    {
        var keys = _keyGenerator.GenerateKeys();
        var n = keys.PublicKey.N;

        Assert.That(n, Is.GreaterThan(BigInteger.One));

        // n = p * q, где p, q — простые, значит n не простое
        // (для 8-битных p, q это очевидно, но проверим хотя бы делители)
        Assert.That(IsPrimeSimple(n), Is.False);
    }

    // ---------- Вспомогательный метод ----------

    private static bool IsPrimeSimple(BigInteger n)
    {
        if (n < 2) return false;
        if (n < 4) return true;
        if (n % 2 == 0) return false;

        for (BigInteger i = 3; i * i <= n; i += 2)
            if (n % i == 0)
                return false;

        return true;
    }
}