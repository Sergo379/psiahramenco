using System.IO;
using ClosedXML.Excel;

namespace ExamTicketGenerator;

public sealed class JournalFileAccessException(string message, Exception inner) : Exception(message, inner);

public sealed class ExcelJournal(string filePath)
{
    private static readonly string[] Headers = ["Группа", "Фамилия", "Имя", "Номер билета", "Дата и время", "Повтор"];

    public void EnsureCreated()
    {
        if (File.Exists(filePath)) return;
        using var workbook = new XLWorkbook();
        CreateSheet(workbook);
        Save(workbook);
    }

    public JournalEntry? FindFirst(string group, string lastName, string firstName)
    {
        if (!File.Exists(filePath)) return null;
        using var workbook = new XLWorkbook(filePath);
        var sheet = GetSheet(workbook);
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
        {
            if (sheet.Cell(row, 1).GetString() == group && sheet.Cell(row, 2).GetString() == lastName && sheet.Cell(row, 3).GetString() == firstName)
                return new JournalEntry(group, lastName, firstName, sheet.Cell(row, 4).GetValue<int>(),
                    sheet.Cell(row, 5).GetDateTime(), sheet.Cell(row, 6).GetString() == "да");
        }
        return null;
    }

    public void Append(JournalEntry entry)
    {
        try
        {
            using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.Any() ? GetSheet(workbook) : CreateSheet(workbook);
            var row = (sheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
            sheet.Cell(row, 1).Value = entry.Group;
            sheet.Cell(row, 2).Value = entry.LastName;
            sheet.Cell(row, 3).Value = entry.FirstName;
            sheet.Cell(row, 4).Value = entry.TicketNumber;
            sheet.Cell(row, 5).Value = entry.DateTime;
            sheet.Cell(row, 5).Style.DateFormat.Format = "dd.MM.yyyy HH:mm:ss";
            sheet.Cell(row, 6).Value = entry.IsRepeat ? "да" : "нет";
            Save(workbook);
        }
        catch (IOException ex) { throw new JournalFileAccessException("Журнал занят или недоступен для записи.", ex); }
        catch (UnauthorizedAccessException ex) { throw new JournalFileAccessException("Нет доступа к папке журнала.", ex); }
    }

    private static IXLWorksheet GetSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidDataException("Журнал не содержит листов.");
        for (var column = 1; column <= Headers.Length; column++)
            if (sheet.Cell(1, column).GetString() != Headers[column - 1])
                throw new InvalidDataException("Неверный формат results.xlsx. Требуются колонки: " + string.Join(", ", Headers));
        return sheet;
    }

    private static IXLWorksheet CreateSheet(XLWorkbook workbook)
    {
        var sheet = workbook.AddWorksheet("Результаты");
        for (var col = 0; col < Headers.Length; col++) sheet.Cell(1, col + 1).Value = Headers[col];
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E5C39D");
        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, 3).Width = 24;
        sheet.Column(4).Width = 18;
        sheet.Column(5).Width = 24;
        sheet.Column(6).Width = 14;
        return sheet;
    }

    private void Save(XLWorkbook workbook)
    {
        var temporary = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp.xlsx";
        try
        {
            // Verify an exclusive write is possible before preparing the replacement.
            if (File.Exists(filePath))
                using (var access = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            workbook.SaveAs(temporary);
            // Replace only after the new workbook is fully serialized; never truncate the original.
            File.Move(temporary, filePath, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
