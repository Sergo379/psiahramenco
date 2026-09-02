using ClosedXML.Excel;

namespace ExamTicketGenerator;

public sealed class ExcelJournal
{
    private readonly string _filePath;

    public ExcelJournal(string filePath)
    {
        _filePath = filePath;
    }

    public void Append(
        string lastName,
        string firstName,
        int ticketNumber,
        DateTime dateTime)
    {
        while (true)
        {
            try
            {
                using var workbook = File.Exists(_filePath)
                    ? new XLWorkbook(_filePath)
                    : CreateWorkbook();

                var worksheet = workbook.Worksheet("Journal");

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                var newRow = lastRow + 1;

                worksheet.Cell(newRow, 1).Value = lastName;
                worksheet.Cell(newRow, 2).Value = firstName;
                worksheet.Cell(newRow, 3).Value = ticketNumber;
                worksheet.Cell(newRow, 4).Value = dateTime;

                worksheet.Cell(newRow, 4)
                    .Style.DateFormat.Format = "dd.MM.yyyy HH:mm:ss";

                worksheet.Columns(1, 4).AdjustToContents();

                workbook.SaveAs(_filePath);

                return;
            }
            catch (IOException)
            {
                ShowFileError();
            }
            catch (UnauthorizedAccessException)
            {
                ShowFileError();
            }
        }
    }

    private static XLWorkbook CreateWorkbook()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Journal");

        worksheet.Cell(1, 1).Value = "Last name";
        worksheet.Cell(1, 2).Value = "First name";
        worksheet.Cell(1, 3).Value = "Номер билета";
        worksheet.Cell(1, 4).Value = "Дата и время";

        worksheet.Range(1, 1, 1, 4).Style.Font.Bold = true;

        return workbook;
    }

    private static void ShowFileError()
    {
        Console.WriteLine();
        Console.WriteLine("Не удалось записать данные в journal.xlsx.");
        Console.WriteLine("Возможно, файл открыт в Excel.");
        Console.WriteLine("Закройте journal.xlsx и нажмите Enter для повторной попытки.");

        Console.ReadLine();
    }
}