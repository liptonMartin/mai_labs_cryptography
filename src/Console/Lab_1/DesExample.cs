using Core.Lab_1;

namespace Console.Lab_1;

public class DesExample
{
    public static async Task Main(string[] args)
    {
        byte[] key = [10, 51, 15, 36, 120, 23, 89, 10];
        var context = new CryptoContext(key, EncryptMode.Ecb, PaddingMode.Zeros, new Des());

        byte[] message = [35, 19, 110, 12, 0, 1, 5, 7];
        var encryptedMessage = await context.EncryptAsync(message);

        System.Console.Write(encryptedMessage);
    }
}