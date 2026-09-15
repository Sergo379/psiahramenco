using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExamTicketGenerator;

namespace ExamTicketGenerator.Tests;

public class CoreTests
{
    [Fact]
    public void StudentsReader_ReadsSheetsAndSkipsEmptyRows()
    {
        var path = TempFile("students.xlsx");
        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.AddWorksheet("ИС-21"); sheet.Cell("A1").Value = "Фамилия"; sheet.Cell("B1").Value = "Имя"; sheet.Cell("A2").Value = "Иванов"; sheet.Cell("B2").Value = "Иван"; workbook.SaveAs(path);
        }
        var group = Assert.Single(StudentsReader.Read(path));
        Assert.Equal("Иванов Иван", Assert.Single(group.Students).DisplayName);
        File.Delete(path);
    }

    [Fact]
    public void TicketsReader_ParsesThreeQuestionsAndSkipsInvalidTicket()
    {
        var path = TempFile("tickets.docx");
        using (var doc = WordprocessingDocument.Create(path, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart(); main.Document = new Document(new Body(new Paragraph(new Text("Билет 1")), new Paragraph(new Text("1. Первый")), new Paragraph(new Text("2. Второй")), new Paragraph(new Text("3. Третий")), new Paragraph(new Text("Билет 2")), new Paragraph(new Text("1. Только один")))); main.Document.Save();
        }
        var warnings = new List<string>(); var tickets = TicketsReader.Read(path, warnings.Add);
        var ticket = Assert.Single(tickets); Assert.Equal("Третий", ticket.Questions[2]); Assert.Contains(warnings, item => item.Contains("Билет № 2"));
        File.Delete(path);
    }

    [Fact]
    public void Journal_RepeatUsesFirstTicketAndAppendsHistory()
    {
        var path = TempFile("results.xlsx"); var journal = new ExcelJournal(path); var first = new Student("Иванов", "Иван");
        journal.Append(new JournalEntry("ИС-21", first.LastName, first.FirstName, 7, DateTime.Now, false));
        var service = new TicketService([new Ticket(1, ["a", "b", "c"]), new Ticket(7, ["a", "b", "c"])]);
        var number = service.ChooseTicketNumber("ИС-21", first, journal, out var repeat);
        journal.Append(new JournalEntry("ИС-21", first.LastName, first.FirstName, number, DateTime.Now, repeat));
        Assert.True(repeat); Assert.Equal(7, number);
        using var workbook = new XLWorkbook(path); Assert.Equal(3, workbook.Worksheet(1).LastRowUsed()!.RowNumber()); Assert.Equal("да", workbook.Worksheet(1).Cell(3, 6).GetString());
        File.Delete(path);
    }

    private static string TempFile(string name) => Path.Combine(Path.GetTempPath(), $"ticket-{Guid.NewGuid():N}-{name}");
}
