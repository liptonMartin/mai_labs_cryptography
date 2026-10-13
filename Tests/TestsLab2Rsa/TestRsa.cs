using System.Numerics;
using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa;

[TestFixture]
public class TestRsa
{
    private Rsa _rsa;

    [SetUp]
    public void SetUp()
    {
        _rsa = new Rsa();
    }

    [Test]
    public void BaseTest()
    {
        var message = new BigInteger(10);
        var cipherText = _rsa.Encrypt(message);
        var decryptedMessage = _rsa.Decrypt(cipherText);
        Assert.That(message, Is.EqualTo(decryptedMessage));
    }

    [Test]
    public void Encrypt_ShouldChangeMessage()
    {
        // Проверяем, что шифротекст отличается от исходного сообщения
        // (иначе шифрование бесполезно). Может не выполняться для тривиальных случаев,
        // поэтому берём сообщение подальше от границ.
        var message = new BigInteger(12345);
        var cipherText = _rsa.Encrypt(message);
        Assert.That(cipherText, Is.Not.EqualTo(message));
    }

    [Test]
    public void EncryptDecrypt_Zero_ShouldReturnZero()
    {
        var message = BigInteger.Zero;
        var cipherText = _rsa.Encrypt(message);
        var decryptedMessage = _rsa.Decrypt(cipherText);
        Assert.That(decryptedMessage, Is.EqualTo(message));
    }

    [Test]
    public void EncryptDecrypt_One_ShouldReturnOne()
    {
        var message = BigInteger.One;
        var cipherText = _rsa.Encrypt(message);
        var decryptedMessage = _rsa.Decrypt(cipherText);
        Assert.That(decryptedMessage, Is.EqualTo(message));
    }

    [Test]
    public void EncryptDecrypt_LargeMessage_ShouldReturnSameMessage()
    {
        // Большое число, но всё ещё меньше модуля n.
        // Если n достаточно большое, тест пройдёт.
        var message = new BigInteger(987654321);
        var cipherText = _rsa.Encrypt(message);
        var decryptedMessage = _rsa.Decrypt(cipherText);
        Assert.That(decryptedMessage, Is.EqualTo(message));
    }

    [Test]
    public void Encrypt_ShouldBeDeterministic()
    {
        // Классический RSA без padding детерминирован:
        // одно и то же сообщение шифруется в один и тот же шифротекст.
        var message = new BigInteger(42);
        var cipher1 = _rsa.Encrypt(message);
        var cipher2 = _rsa.Encrypt(message);
        Assert.That(cipher1, Is.EqualTo(cipher2));
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(65537)]
    public void EncryptDecrypt_VariousMessages_ShouldReturnSameMessage(int value)
    {
        var message = new BigInteger(value);
        var cipherText = _rsa.Encrypt(message);
        var decryptedMessage = _rsa.Decrypt(cipherText);
        Assert.That(decryptedMessage, Is.EqualTo(message));
    }
}