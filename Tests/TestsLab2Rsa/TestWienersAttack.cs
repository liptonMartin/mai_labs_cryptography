using System.Numerics;
using Core.Lab_2_RSA;

namespace Tests.TestsLab2Rsa;

[TestFixture]
public class TestWienersAttack
{
    [Test]
    public void BaseTest()
    {
        var attack = new WienersAttack(new(47318087, 60477719));
        var N = attack.Hack();
        var expected = new BigInteger(60477719);
        Assert.That(N, Is.EqualTo(expected));
    }
}