using System.Text;
using Console.Common;
using Core.Lab_2_RSA;

namespace Console.Lab2Rsa;

public static class RsaExamples
{
    private const uint DefaultBitLength = 24 * 8;
    private const double DefaultProbability = 0.99999;

    private const PrimaryTestType DefaultPrimaryTestType =
        PrimaryTestType.MellerRabin;

    private const string DefaultMessage = "Hello, RSA!";

    public async static Task RsaMessage()
    {
        var primaryTestType = OutputHelper.ReadEnum(
            "Input primary test type",
            DefaultPrimaryTestType);

        var probability = OutputHelper.ReadDouble(
            "Input probability",
            DefaultProbability);

        var bitLength = OutputHelper.ReadUInt(
            "Input bit length",
            DefaultBitLength);

        var message = OutputHelper.ReadString(
            "Input message",
            DefaultMessage);

        var rsa = new Rsa(
            primaryTestType,
            probability,
            bitLength);

        var messageBytes = Encoding.UTF8.GetBytes(message);

        var encryptedMessage = rsa.Encrypt(messageBytes);
        var decryptedMessage = rsa.Decrypt(encryptedMessage);

        OutputHelper.OutputByteArray(
            messageBytes,
            "Message");

        OutputHelper.OutputByteArray(
            encryptedMessage,
            "Encrypted message");

        OutputHelper.OutputByteArray(
            decryptedMessage,
            "Decrypted message");

        System.Console.WriteLine(
            $"Decrypted text: " +
            $"{Encoding.UTF8.GetString(decryptedMessage)}");
    }
}