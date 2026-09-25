using System.Text;
using Console.Common;
using Core.Lab_1;

namespace Console.Lab_1;

public static class DealExamples
{
    private const int BlockSize = 16;

    private static readonly byte[] DefaultKey =
    [
        1, 35, 69, 103, 137, 171, 205, 239, 1, 35, 69, 103, 137, 171, 205, 239
    ];

    private static readonly byte[] DefaultInitializationVector =
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ];

    private const string DefaultMessage = "Hello, DEAL 128!";

    private const EncryptMode DefaultEncryptMode = EncryptMode.Ecb;
    private const PaddingMode DefaultPaddingMode = PaddingMode.Zeros;

    public static async Task DealMessage()
    {
        var key = OutputHelper.ReadBytes(
            BlockSize,
            DefaultKey,
            "Input DEAL key");

        var message = OutputHelper.ReadString(
            "Input message",
            DefaultMessage);

        var encryptMode = OutputHelper.ReadEnum(
            "Input encryption mode",
            DefaultEncryptMode);

        var paddingMode = OutputHelper.ReadEnum(
            "Input padding mode",
            DefaultPaddingMode);

        byte[]? initializationVector = null;

        if (encryptMode != EncryptMode.Ecb)
        {
            initializationVector = OutputHelper.ReadBytes(
                BlockSize,
                DefaultInitializationVector,
                "Input initialization vector");
        }

        var context = new CryptoContext<Deal128>(
            key,
            encryptMode,
            paddingMode,
            initializationVector);

        var messageBytes = Encoding.UTF8.GetBytes(message);

        var encryptedMessage =
            await context.EncryptAsync(messageBytes);

        var decryptedMessage =
            await context.DecryptAsync(encryptedMessage);

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

    public static async Task DealFiles()
    {
        var key = OutputHelper.ReadBytes(
            BlockSize,
            DefaultKey,
            "Input DES key");

        var encryptMode = OutputHelper.ReadEnum(
            "Input encryption mode",
            DefaultEncryptMode);

        var paddingMode = OutputHelper.ReadEnum(
            "Input padding mode",
            DefaultPaddingMode);

        byte[]? initializationVector = null;

        if (encryptMode != EncryptMode.Ecb)
        {
            initializationVector = OutputHelper.ReadBytes(
                BlockSize,
                DefaultInitializationVector,
                "Input initialization vector");
        }

        var context = new CryptoContext<Deal128>(
            key,
            encryptMode,
            paddingMode,
            initializationVector);

        var inputFilePath = OutputHelper.ReadFilePath(
            "Input your file:");

        var outputEncryptedFilePath =
            OutputHelper.ReadFilePath(
                "Input your output encrypted message file:");

        var outputDecryptedFilePath =
            OutputHelper.ReadFilePath(
                "Input your output decrypted message file:");

        await context.EncryptAsync(
            inputFilePath,
            outputEncryptedFilePath);

        await context.DecryptAsync(
            outputEncryptedFilePath,
            outputDecryptedFilePath);

        System.Console.WriteLine("Done!");
    }
}