using System.Numerics;

namespace Core.Lab_2_RSA;

public class Rsa
{
    private KeyGenerator KeyGenerator;
    private RsaKeys keys;

    public Rsa(
        PrimaryTestType primaryTestType = PrimaryTestType.MellerRabin,
        double probability = 0.99999,
        uint bitLength = 24 * 8)
    {
        KeyGenerator = new KeyGenerator(primaryTestType, probability, bitLength);
        keys = KeyGenerator.GenerateKeys();
    }

    public void RegenerateKeys()
    {
        keys = KeyGenerator.GenerateKeys();
    }

    public byte[] Encrypt(byte[] message)
    {
        var numberMessage = new BigInteger(message);
        var numberCipher = Encrypt(numberMessage);
        return numberCipher.ToByteArray();
    }

    public byte[] Decrypt(byte[] message)
    {
        var numberCipher = new BigInteger(message);
        var numberMessage = Decrypt(numberCipher);
        return numberMessage.ToByteArray();
    }

    public BigInteger Encrypt(BigInteger message) =>
        Math.FastBinaryPowModule(message, keys.PublicKey.Exp, keys.PublicKey.N);

    public BigInteger Decrypt(BigInteger cipherText) =>
        Math.FastBinaryPowModule(cipherText, keys.PrivateKey.Exp, keys.PrivateKey.N);
}