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

    private static void OutputByteArray(byte[] array)
    {
        System.Console.WriteLine(string.Join(", ", array.Select(b => $"[{b}]")));
    }

    private static void OutputByteArray(byte[] array, string nameArray)
    {
        System.Console.Write($"{nameArray}: ");
        OutputByteArray(array);
    }
}