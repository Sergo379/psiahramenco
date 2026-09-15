using System;
using System.IO;
using ClosedXML.Excel;
namespace ExamTicketGenerator;
public sealed class JournalFileAccessException(string message, Exception inner) : Exception(message, inner);
public sealed class ExcelJournal(string filePath)
{
    private static readonly string[] Headers = ["Группа", "Фамилия", "Имя", "Номер билета", "Дата и время", "Повтор"];
    public JournalEntry? FindFirst(string group, string lastName, string firstName)
    {
        if (!File.Exists(filePath)) return null;
        using var workbook = new XLWorkbook(filePath); var sheet = workbook.Worksheets.First();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
            if (sheet.Cell(row, 1).GetString() == group && sheet.Cell(row, 2).GetString() == lastName && sheet.Cell(row, 3).GetString() == firstName)
                return new JournalEntry(group, lastName, firstName, sheet.Cell(row, 4).GetValue<int>(), sheet.Cell(row, 5).GetDateTime(), sheet.Cell(row, 6).GetString() == "да");
        return null;
    }
    public void Append(JournalEntry entry)
    {
        try
        {
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault() ?? workbook.AddWorksheet("Результаты");
            if (sheet.Cell(1, 1).IsEmpty()) for (var col = 0; col < Headers.Length; col++) sheet.Cell(1, col + 1).Value = Headers[col];
            var row = (sheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
            sheet.Cell(row, 1).Value = entry.Group; sheet.Cell(row, 2).Value = entry.LastName; sheet.Cell(row, 3).Value = entry.FirstName; sheet.Cell(row, 4).Value = entry.TicketNumber;
            sheet.Cell(row, 5).Value = entry.DateTime; sheet.Cell(row, 5).Style.DateFormat.Format = "dd.MM.yyyy HH:mm:ss"; sheet.Cell(row, 6).Value = entry.IsRepeat ? "да" : "нет";
            sheet.Row(1).Style.Font.Bold = true; sheet.Columns(1, 6).AdjustToContents(); workbook.SaveAs(filePath);
        }
        catch (IOException ex) { throw new JournalFileAccessException("Файл результатов занят или недоступен для записи.", ex); }
        catch (UnauthorizedAccessException ex) { throw new JournalFileAccessException("Нет доступа к файлу результатов.", ex); }
    }
}
