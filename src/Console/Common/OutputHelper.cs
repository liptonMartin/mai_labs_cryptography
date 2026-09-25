namespace Console.Common;

public static class OutputHelper
{
    public static void OutputByteArray(byte[] array)
    {
        System.Console.WriteLine(
            string.Join(", ", array.Select(b => $"[{b}]")));
    }

    public static void OutputByteArray(byte[] array, string nameArray)
    {
        System.Console.Write($"{nameArray}: ");
        OutputByteArray(array);
    }

    public static T ReadEnum<T>(
        string prompt,
        T defaultValue)
        where T : struct, Enum
    {
        var values = Enum.GetNames<T>();

        while (true)
        {
            System.Console.WriteLine(
                $"{prompt} ({string.Join(", ", values)}) " +
                $"[Enter = {defaultValue}]:");

            System.Console.Write("> ");

            var input = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                return defaultValue;

            if (Enum.TryParse<T>(
                    input,
                    ignoreCase: true,
                    out var result)
                && Enum.IsDefined(result))
            {
                return result;
            }

            System.Console.WriteLine(
                "Неверный ввод, попробуйте ещё раз.");
        }
    }

    public static byte[] ReadBytes(
        int count,
        string prompt = "Input bytes")
    {
        while (true)
        {
            System.Console.WriteLine(
                $"{prompt} ({count} bytes, numbers 0-255):");

            System.Console.Write("> ");

            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                System.Console.WriteLine(
                    "Input cannot be empty, let's try again.");

                continue;
            }

            var parts = input.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != count)
            {
                System.Console.WriteLine(
                    $"You must enter exactly {count} bytes.");

                continue;
            }

            var bytes = new byte[count];
            var valid = true;

            for (var i = 0; i < count; i++)
            {
                if (!byte.TryParse(parts[i], out var value))
                {
                    valid = false;
                    break;
                }

                bytes[i] = value;
            }

            if (valid)
                return bytes;

            System.Console.WriteLine(
                "Invalid input. Bytes must be numbers from 0 to 255.");
        }
    }

    public static byte[] ReadBytes(
        int count,
        byte[] defaultValue,
        string prompt = "Input bytes")
    {
        while (true)
        {
            System.Console.WriteLine(
                $"{prompt} ({count} bytes, numbers 0-255) " +
                $"[Enter = {FormatBytes(defaultValue)}]:");

            System.Console.Write("> ");

            var input = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                return defaultValue;

            var parts = input.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != count)
            {
                System.Console.WriteLine(
                    $"You must enter exactly {count} bytes.");

                continue;
            }

            var bytes = new byte[count];
            var valid = true;

            for (var i = 0; i < count; i++)
            {
                if (!byte.TryParse(parts[i], out var value))
                {
                    valid = false;
                    break;
                }

                bytes[i] = value;
            }

            if (valid)
                return bytes;

            System.Console.WriteLine(
                "Invalid input. Bytes must be numbers from 0 to 255.");
        }
    }

    public static string ReadString(
        string prompt,
        string defaultValue)
    {
        System.Console.WriteLine(
            $"{prompt} [Enter = {defaultValue}]:");

        System.Console.Write("> ");

        var input = System.Console.ReadLine();

        return string.IsNullOrEmpty(input)
            ? defaultValue
            : input;
    }

    public static string ReadFilePath(string prompt)
    {
        while (true)
        {
            System.Console.WriteLine(prompt);
            System.Console.Write("> ");

            var input = System.Console.ReadLine()?.Trim();

            if (!string.IsNullOrEmpty(input))
                return input;

            System.Console.WriteLine(
                "Path cannot be empty, let's try again.");
        }
    }

    private static string FormatBytes(byte[] bytes)
    {
        return string.Join(
            " ",
            bytes.Select(b => b.ToString()));
    }
}