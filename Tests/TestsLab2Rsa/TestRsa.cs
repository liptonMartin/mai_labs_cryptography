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
}