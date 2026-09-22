using Core.Lab_1;

namespace Console.Lab_1;

public static class DesExamples
{
    public static async Task BaseDes()
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