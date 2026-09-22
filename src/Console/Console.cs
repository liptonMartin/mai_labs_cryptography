using Console.Lab_1;

namespace Console;

public static class Console
{
    private record LabAction(string Description, Func<Task> Run);

    private static readonly Dictionary<string, (string Title, Dictionary<string, LabAction> Actions)> Labs = new()
    {
        ["1"] = ("Lab 1. DES", new()
        {
            ["1"] = new LabAction(
                "Шифрование/расшифровка примера",
                () => DesExamples.DesEcbZeros()
            ),
            ["2"] = new LabAction(
                "Шифрование/расшифровка примера из файла",
                () => DesExamples.DesEcbZerosFromFile()
                ),
        }),
    };

    public static async Task Main(string[] args)
    {
        while (true)
        {
            PrintLabsMenu();
            var labChoice = ReadChoice(Labs.Keys);

            if (labChoice is null) return;

            var (title, actions) = Labs[labChoice];

            while (true)
            {
                PrintActionsMenu(title, actions);
                var actionChoice = ReadChoice(actions.Keys);

                if (actionChoice is null) break;

                System.Console.WriteLine();
                System.Console.WriteLine(new string('─', 60));

                try
                {
                    await actions[actionChoice].Run();
                }
                catch (Exception ex)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine($"Ошибка: {ex.Message}");
                    System.Console.ResetColor();
                }
                
                System.Console.WriteLine();
                System.Console.WriteLine(new string('─', 60));
                System.Console.WriteLine();
                System.Console.WriteLine("Нажмите Enter, чтобы продолжить...");
                System.Console.ReadLine();
            }
        }
    }

    private static void PrintLabsMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("Выберите номер лабы:");
        foreach (var (key, value) in Labs)
            System.Console.WriteLine($"  {key}. {value.Title}");
        System.Console.WriteLine("  0. Выход");
        System.Console.Write("> ");
    }

    private static void PrintActionsMenu(string labTitle, Dictionary<string, LabAction> actions)
    {
        System.Console.Clear();
        System.Console.WriteLine($"Лаба: {labTitle}");
        System.Console.WriteLine("Что вывести:");
        foreach (var (key, value) in actions)
            System.Console.WriteLine($"  {key}. {value.Description}");
        System.Console.WriteLine("  0. Назад");
        System.Console.Write("> ");
    }

    private static string? ReadChoice(IEnumerable<string> validKeys)
    {
        var valid = validKeys.ToHashSet();

        while (true)
        {
            var input = System.Console.ReadLine()?.Trim();

            if (input == "0") return null;
            if (input is not null && valid.Contains(input)) return input;

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write("Некорректный ввод, попробуйте ещё раз: ");
            System.Console.ResetColor();
        }
    }
}