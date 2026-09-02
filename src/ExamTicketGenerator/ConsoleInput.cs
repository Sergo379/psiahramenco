namespace ExamTicketGenerator;

public static class ConsoleInput
{
    public static string? ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            var buffer = new List<char>();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return null;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();

                    var value = new string(buffer.ToArray()).Trim();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }

                    Console.WriteLine(
                        "Поле не может быть пустым. Повторите ввод."
                    );

                    break;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Count > 0)
                    {
                        buffer.RemoveAt(buffer.Count - 1);
                        Console.Write("\b \b");
                    }

                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                {
                    buffer.Add(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }
    }
}