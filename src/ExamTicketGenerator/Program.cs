namespace ExamTicketGenerator;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var journalPath = Path.Combine(
            Environment.CurrentDirectory,
            "journal.xlsx"
        );

        var journal = new ExcelJournal(journalPath);

        Console.WriteLine("Генератор экзаменационных билетов");
        Console.WriteLine("Для выхода нажмите ESC.");
        Console.WriteLine();

        while (true)
        {
            var lastName = ConsoleInput.ReadRequired("Last name: ");

            if (lastName is null)
            {
                break;
            }

            var firstName = ConsoleInput.ReadRequired("First name: ");

            if (firstName is null)
            {
                break;
            }

            var ticketNumber = TicketService.GenerateTicketNumber();

            Console.WriteLine($"Билет № {ticketNumber}.");

            journal.Append(
                lastName,
                firstName,
                ticketNumber,
                DateTime.Now
            );

            Console.WriteLine("Запись сохранена в journal.xlsx.");
            Console.WriteLine();
        }

        Console.WriteLine("Работа завершена.");
    }
}