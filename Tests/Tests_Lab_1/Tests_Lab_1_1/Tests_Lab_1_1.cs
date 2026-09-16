using Core.Lab_1;

namespace Tests.Tests_Lab_1.Tests_Lab_1_1;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestMsb0()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [1, 5, 3, 6, 3, 0, 4, 7];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_00111101];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestMsb1()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [3, 6, 4, 8, 1, 5, 2, 3];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb1);

        byte[] expected = [0b_10111001];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestLsb0()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [0, 7, 1, 6, 2, 5, 3, 4];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromLsb0);

        byte[] expected = [0b_10100111];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestLsb1()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [4, 1, 5, 6, 7, 8, 2, 1];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromLsb1);

        byte[] expected = [0b_11101110];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestReverseBitsMsb0()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [7, 6, 5, 4, 3, 2, 1, 0];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_11001101];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestIdentityMsb0()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [0, 1, 2, 3, 4, 5, 6, 7];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        Assert.That(actual, Is.EqualTo(array));
    }

    [Test]
    public void TestIdentityLsb0()
    {
        byte[] array = [0b_10110011];
        byte[] pBlock = [0, 1, 2, 3, 4, 5, 6, 7];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromLsb0);

        Assert.That(actual, Is.EqualTo(array));
    }

    [Test]
    public void TestTwoBytesMsb0()
    {
        byte[] array = [0b_10110011, 0b_01101001];

        byte[] pBlock = [0, 8, 1, 9, 2, 10, 3, 11];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_1001_1110, 0b_0000_0000];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestSingleSetBitMsb0()
    {
        byte[] array = [0b_00000001];

        byte[] pBlock = [7, 6, 5, 4, 3, 2, 1, 0];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_10000000];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestRepeatedBitsMsb0()
    {
        byte[] array = [0b_10000000];

        byte[] pBlock = [0, 0, 0, 0, 1, 1, 1, 1];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_11110000];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestSelectPartOfBitsMsb0()
    {
        byte[] array = [0b_10110011];

        byte[] pBlock = [0, 2, 4, 6];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb0);

        byte[] expected = [0b_11010000];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestTwoBytesMsb1()
    {
        byte[] array = [0b_10110011, 0b_01101001];

        byte[] pBlock = [1, 9, 2, 10, 3, 11, 4, 12];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromMsb1);

        byte[] expected = [0b_1001_1110, 0b_0000_0000];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestTwoBytesLsb0()
    {
        byte[] array = [0b_10110011, 0b_01101001];

        byte[] pBlock = [0, 8, 1, 9, 2, 10, 3, 11];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromLsb0);

        byte[] expected = [0b_0000_0000, 0b_0100_1011];

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestTwoBytesLsb1()
    {
        byte[] array = [0b_10110011, 0b_01101001];

        byte[] pBlock = [1, 9, 2, 10, 3, 11, 4, 12];

        var actual = PBlock.Permutate(array, pBlock, IndexBitsRule.FromLsb1);

        byte[] expected = [0b_0000_0000, 0b_0100_1011];

        Assert.That(actual, Is.EqualTo(expected));
    }
}