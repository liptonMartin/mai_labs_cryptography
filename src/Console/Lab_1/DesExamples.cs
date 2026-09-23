using Core.Lab_1;

namespace Console.Lab_1;

public static class DesExamples
{
    public static async Task DesEcbZeros()
    {
        byte[] key = [1, 35, 69, 103, 137, 171, 205, 239];
        var context = new CryptoContext<Des>(key, EncryptMode.Ecb, PaddingMode.Zeros);

        byte[] message = [1, 35, 69, 103, 137, 171, 205, 239];
        var encryptedMessage = await context.EncryptAsync(message);
        var decryptedMessage = await context.DecryptAsync(encryptedMessage);

        OutputByteArray(message, "Message");
        OutputByteArray(encryptedMessage, "Encrypted message");
        OutputByteArray(decryptedMessage, "Decrypted message");
    }

    public static async Task DesEcbZerosFromFile()
    {
        byte[] key = [1, 35, 69, 103, 137, 171, 205, 239];
        var context = new CryptoContext<Des>(key, EncryptMode.Ecb, PaddingMode.Zeros);

        await HandleFiles(context);
    }

    public static async Task DesChooseModes()
    {
        byte[] key = [1, 35, 69, 103, 137, 171, 205, 239];

        var encryptMode = ReadEnum<EncryptMode>("Input encryption mode");
        var paddingMode = ReadEnum<PaddingMode>("Input padding mode");

        byte[]? initializationVector = null;
        if (encryptMode != EncryptMode.Ecb)
            initializationVector = Read8Bytes("Input ");
        var context = new CryptoContext<Des>(key, encryptMode, paddingMode, initializationVector);
        await HandleFiles(context);
    }

    private static void OutputByteArray(byte[] array)
    {
        System.Console.WriteLine(string.Join(", ", array.Select(b => $"[{b}]")));
    }

    private static void OutputByteArray(byte[] array, string nameArray)
    {
        System.Console.Write($"{nameArray}: ");
        OutputByteArray(array);
    }

    private static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        var values = Enum.GetNames<T>();
        while (true)
        {
            System.Console.WriteLine($"{prompt} ({string.Join(", ", values)}):");
            System.Console.Write("> ");
            var input = System.Console.ReadLine();

            if (Enum.TryParse<T>(input, ignoreCase: true, out var result)
                && Enum.IsDefined(result))
            {
                return result;
            }

            System.Console.WriteLine("Неверный ввод, попробуйте ещё раз.");
        }
    }

    private static byte[] Read8Bytes(string prompt)
    {
        System.Console.WriteLine($"{prompt}:");
        System.Console.Write("> ");
        string[]? parts;
        while (true)
        {
            System.Console.WriteLine("Input 8 bytes (numbers 0-255) with space: ");
            System.Console.Write("> ");
            parts = System.Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts is null)
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("Invalid input, let's try again: ");
                System.Console.ResetColor();
                System.Console.Write("> ");
            }
            else
                break;
        }

        var bytes = new byte[8];
        for (var i = 0; i < 8; i++)
            bytes[i] = byte.Parse(parts[i]);

        return bytes;
    }

    private static async Task HandleFiles(CryptoContext<Des> context)
    {
        System.Console.WriteLine("Input your file:");
        System.Console.Write("> ");

        var inputFilePath = System.Console.ReadLine()?.Trim();
        if (inputFilePath == null)
        {
            System.Console.WriteLine("Failed to read input file");
            return;
        }

        System.Console.WriteLine("Input your output encrypted message file:");
        System.Console.Write("> ");

        var outputEncryptedFilePath = System.Console.ReadLine()?.Trim();
        if (outputEncryptedFilePath == null)
        {
            System.Console.WriteLine("Failed to read output encrypted message file");
            return;
        }

        System.Console.WriteLine("Input your output decrypted message file:");
        System.Console.Write("> ");

        var outputDecryptedFilePath = System.Console.ReadLine()?.Trim();
        if (outputDecryptedFilePath == null)
        {
            System.Console.WriteLine("Failed to read output decrypted message file");
            return;
        }

        await context.EncryptAsync(inputFilePath, outputEncryptedFilePath);
        await context.DecryptAsync(outputEncryptedFilePath, outputDecryptedFilePath);

        System.Console.WriteLine("Done!");
    }
}