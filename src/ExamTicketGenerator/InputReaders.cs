using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExamTicketGenerator;

public static class StudentsReader
{
    public static IReadOnlyList<Group> Read(string path)
    {
        using var workbook = new XLWorkbook(path);
        var groups = new List<Group>();
        foreach (var sheet in workbook.Worksheets)
        {
            var students = new List<Student>();
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            for (var row = 2; row <= lastRow; row++)
            {
                var lastName = sheet.Cell(row, 1).GetString().Trim();
                var firstName = sheet.Cell(row, 2).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(firstName)) students.Add(new Student(lastName, firstName));
            }
            groups.Add(new Group(sheet.Name, students));
        }
        return groups;
    }
}

public static class TicketsReader
{
    private static readonly Regex Header = new(@"^\s*Билет\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Question = new(@"^\s*(\d+)[.)]\s*(.+?)\s*$", RegexOptions.Compiled);

    public static IReadOnlyList<Ticket> Read(string path, Action<string>? log = null)
    {
        var parsed = new List<(int Number, List<string> Questions)>();
        using var document = WordprocessingDocument.Open(path, false);
        foreach (var paragraph in document.MainDocumentPart?.Document.Body?.Elements<Paragraph>() ?? [])
        {
            var text = paragraph.InnerText.Trim();
            var header = Header.Match(text);
            if (header.Success) { parsed.Add((int.Parse(header.Groups[1].Value), [])); continue; }
            var question = Question.Match(text);
            if (question.Success && parsed.Count > 0) parsed[^1].Questions.Add(question.Groups[2].Value);
            else if (!string.IsNullOrWhiteSpace(text)) log?.Invoke($"Пропущен нераспознанный абзац: {text}");
        }
        var result = new List<Ticket>();
        foreach (var item in parsed)
        {
            if (item.Number <= 0 || item.Questions.Count < 3) { log?.Invoke($"Билет № {item.Number} пропущен: найдено вопросов {item.Questions.Count}, требуется минимум 3."); continue; }
            result.Add(new Ticket(item.Number, item.Questions.Take(3).ToArray()));
        }
        return result;
    }
}
