using Core.Lab_1;

namespace Console.Lab_1;

public class DesExample
{
    public static async Task Main(string[] args)
    {
        byte[] key = [1, 35, 69, 103, 137, 171, 205, 239];
        var context = new CryptoContext<Des>(key, EncryptMode.Ecb, PaddingMode.Zeros);

        byte[] message = [1, 35, 69, 103, 137, 171, 205, 239];
        var encryptedMessage = await context.EncryptAsync(message);

        foreach (var b in encryptedMessage)
        {
            System.Console.Write("[");
            System.Console.Write(b);
            System.Console.Write("], ");
        }

        System.Console.WriteLine();
        
        var decryptedMessage = await context.DecryptAsync(encryptedMessage);
        
        foreach (var b in decryptedMessage)
        {
            System.Console.Write("[");
            System.Console.Write(b);
            System.Console.Write("], ");
        }
        
    }
}