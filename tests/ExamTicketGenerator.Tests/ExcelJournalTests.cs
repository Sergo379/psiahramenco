using ClosedXML.Excel;

namespace ExamTicketGenerator.Tests;

public class ExcelJournalTests
{
    [Fact]
    public void Append_CreatesExcelFileWithCorrectHeaderAndStudent()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString()
        );

        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, "journal.xlsx");

        try
        {
            var journal = new ExcelJournal(filePath);

            journal.Append(
                "Popescu",
                "Ion",
                7,
                new DateTime(2026, 9, 2, 12, 30, 0)
            );

            Assert.True(File.Exists(filePath));

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Journal");

            Assert.Equal(
                "Last name",
                worksheet.Cell(1, 1).GetString()
            );

            Assert.Equal(
                "First name",
                worksheet.Cell(1, 2).GetString()
            );

            Assert.Equal(
                "Номер билета",
                worksheet.Cell(1, 3).GetString()
            );

            Assert.Equal(
                "Дата и время",
                worksheet.Cell(1, 4).GetString()
            );

            Assert.Equal(
                "Popescu",
                worksheet.Cell(2, 1).GetString()
            );

            Assert.Equal(
                "Ion",
                worksheet.Cell(2, 2).GetString()
            );

            Assert.Equal(
                7,
                worksheet.Cell(2, 3).GetValue<int>()
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void Append_AddsNewStudentWithoutOverwritingExistingStudent()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString()
        );

        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, "journal.xlsx");

        try
        {
            var journal = new ExcelJournal(filePath);

            journal.Append(
                "Popescu",
                "Ion",
                7,
                DateTime.Now
            );

            journal.Append(
                "Rusu",
                "Ana",
                15,
                DateTime.Now
            );

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("Journal");

            Assert.Equal(
                "Popescu",
                worksheet.Cell(2, 1).GetString()
            );

            Assert.Equal(
                "Ion",
                worksheet.Cell(2, 2).GetString()
            );

            Assert.Equal(
                7,
                worksheet.Cell(2, 3).GetValue<int>()
            );

            Assert.Equal(
                "Rusu",
                worksheet.Cell(3, 1).GetString()
            );

            Assert.Equal(
                "Ana",
                worksheet.Cell(3, 2).GetString()
            );

            Assert.Equal(
                15,
                worksheet.Cell(3, 3).GetValue<int>()
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}