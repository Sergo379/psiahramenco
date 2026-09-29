using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExamTicketGenerator.Tests;

public sealed class ReliabilityTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "tickets-tests-" + Guid.NewGuid().ToString("N"));
    public ReliabilityTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);
    private string FilePath(string name) => Path.Combine(_directory, name);

    [Fact]
    public void MalformedHeadingDoesNotAddQuestionsToPreviousIncompleteTicket()
    {
        WriteWord("Билет 1", "1. один", "Билет неверный", "1. чужой", "2. чужой", "3. чужой", "Билет 3", "1. а", "2. б", "3. в");
        var warnings = new List<string>();
        var ticket = Assert.Single(TicketsReader.Read(FilePath("tickets.docx"), warnings.Add));
        Assert.Equal(3, ticket.Number);
        Assert.Contains(warnings, warning => warning.Contains("Нераспознанный"));
    }

    [Fact]
    public void DuplicateAndOverflowHeadersAreLoggedAndSkipped()
    {
        WriteWord("Билет 5", "1. а", "2. б", "3. в", "Билет 5", "1. г", "2. д", "3. е", "Билет 999999999999999999999", "1. а", "2. б", "3. в");
        var warnings = new List<string>();
        Assert.Equal(5, Assert.Single(TicketsReader.Read(FilePath("tickets.docx"), warnings.Add)).Number);
        Assert.Equal(2, warnings.Count(w => w.Contains("заголовок")));
    }

    [Fact]
    public void ReadsWordsAutomaticNumberedParagraphs()
    {
        using (var doc = WordprocessingDocument.Create(FilePath("tickets.docx"), WordprocessingDocumentType.Document))
        {
            var part = doc.AddMainDocumentPart();
            part.Document = new Document(new Body(new Paragraph(new Run(new Text("Билет 1"))),
                Numbered("Первый"), Numbered("Второй"), Numbered("Третий")));
            part.Document.Save();
        }
        Assert.Equal("Второй", Assert.Single(TicketsReader.Read(FilePath("tickets.docx"))).Questions[1]);
    }

    [Fact]
    public void EmptyGroupsAreRetainedAndNamesAreTrimmed()
    {
        using (var workbook = new XLWorkbook())
        {
            workbook.AddWorksheet("Пустая");
            var sheet = workbook.AddWorksheet("Группа");
            sheet.Cell(2, 1).Value = " Иванов "; sheet.Cell(2, 2).Value = " Иван ";
            sheet.Cell(3, 1).Value = "Неполная строка";
            workbook.SaveAs(FilePath("students.xlsx"));
        }
        var groups = StudentsReader.Read(FilePath("students.xlsx"));
        Assert.Empty(groups[0].Students);
        Assert.Equal(new Student("Иванов", "Иван"), Assert.Single(groups[1].Students));
    }

    [Fact]
    public void JournalCreatedBeforeFirstGenerationAndOriginalCellsArePreserved()
    {
        var journal = new ExcelJournal(FilePath("results.xlsx"));
        journal.EnsureCreated();
        using (var workbook = new XLWorkbook(FilePath("results.xlsx")))
            Assert.Equal(1, workbook.Worksheet(1).LastRowUsed()!.RowNumber());
        journal.Append(new JournalEntry("A", "Иванов", "Иван", 4, new DateTime(2026, 9, 29, 12, 0, 0), false));
        journal.Append(new JournalEntry("A", "Иванов", "Иван", 6, DateTime.Now, true));
        Assert.Equal(4, journal.FindFirst("A", "Иванов", "Иван")!.TicketNumber);
        Assert.Null(journal.FindFirst("B", "Иванов", "Иван"));
        using var saved = new XLWorkbook(FilePath("results.xlsx"));
        Assert.Equal("нет", saved.Worksheet(1).Cell(2, 6).GetString());
        Assert.Equal(4, saved.Worksheet(1).Cell(2, 4).GetValue<int>());
        Assert.Equal(new DateTime(2026, 9, 29, 12, 0, 0), saved.Worksheet(1).Cell(2, 5).GetDateTime());
    }

    [Fact]
    public void LockedJournalLeavesOriginalBytesIntactAndCanBeRetried()
    {
        var path = FilePath("results.xlsx");
        var journal = new ExcelJournal(path);
        journal.EnsureCreated();
        var before = File.ReadAllBytes(path);
        var entry = new JournalEntry("A", "B", "C", 1, DateTime.Now, false);
        using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Assert.Throws<JournalFileAccessException>(() => journal.Append(entry));
        Assert.Equal(before, File.ReadAllBytes(path));
        journal.Append(entry);
        Assert.NotNull(journal.FindFirst("A", "B", "C"));
    }

    [Fact]
    public void RemovedTicketDoesNotChangeStudentsAssignment()
    {
        var journal = new ExcelJournal(FilePath("results.xlsx"));
        journal.Append(new JournalEntry("A", "B", "C", 7, DateTime.Now, false));
        var service = new TicketService([new Ticket(2, ["a", "b", "c"])]);
        Assert.Equal(7, service.ChooseTicketNumber("A", new Student("B", "C"), journal, out var repeat));
        Assert.True(repeat);
        Assert.Throws<InvalidOperationException>(() => service.GetTicket(7));
    }

    [Fact]
    public void RandomChoiceUsesActualValidTicketNumbersAfterSkipping()
    {
        var service = new TicketService([new Ticket(2, ["a", "b", "c"]), new Ticket(9, ["a", "b", "c"])]);
        var journal = new ExcelJournal(FilePath("results.xlsx"));
        for (var i = 0; i < 100; i++)
        {
            Assert.Contains(service.ChooseTicketNumber("A", new Student("B", "C"), journal, out var repeat), new[] { 2, 9 });
            Assert.False(repeat);
        }
    }

    [Fact]
    public void InvalidJournalFormatIsRejectedWithoutOverwriting()
    {
        var path = FilePath("results.xlsx");
        using (var book = new XLWorkbook()) { book.AddWorksheet("Old").Cell(1, 1).Value = "Old header"; book.SaveAs(path); }
        var before = File.ReadAllBytes(path);
        Assert.Throws<InvalidDataException>(() => new ExcelJournal(path).Append(new JournalEntry("A", "B", "C", 1, DateTime.Now, false)));
        Assert.Equal(before, File.ReadAllBytes(path));
    }
    private static Paragraph Numbered(string text) => new(new ParagraphProperties(new NumberingProperties(new NumberingLevelReference { Val = 0 }, new NumberingId { Val = 1 })), new Run(new Text(text)));
    private void WriteWord(params string[] paragraphs)
    {
        using var doc = WordprocessingDocument.Create(FilePath("tickets.docx"), WordprocessingDocumentType.Document);
        var part = doc.AddMainDocumentPart();
        part.Document = new Document(new Body(paragraphs.Select(text => new Paragraph(new Run(new Text(text))))));
        part.Document.Save();
    }
}
