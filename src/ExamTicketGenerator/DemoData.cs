using System.IO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExamTicketGenerator;

// Explicit packaging utility. Normal launches never replace the user's sources.
public static class DemoData
{
    public static void Create(string directory)
    {
        var studentsPath = Path.Combine(directory, "students.xlsx");
        if (!File.Exists(studentsPath))
        {
            using var workbook = new XLWorkbook();
            AddGroup(workbook, "ИС-21", [new("Иванов", "Александр"), new("Петрова", "Анна"), new("Смирнов", "Дмитрий"), new("Коваленко", "Мария")]);
            AddGroup(workbook, "ИС-22", [new("Попеску", "Ион"), new("Русу", "Ана"), new("Морозов", "Максим")]);
            AddGroup(workbook, "Пустая группа", []);
            workbook.SaveAs(studentsPath);
        }
        var ticketsPath = Path.Combine(directory, "tickets.docx");
        if (!File.Exists(ticketsPath))
        {
            string[][] questions =
            [
                ["Что такое алгоритм? Перечислите его основные свойства.", "Сравните линейный и двоичный поиск.", "Объясните принцип работы стека и приведите пример."],
                ["Объясните понятия класса и объекта.", "Что такое инкапсуляция и зачем она нужна?", "Приведите пример использования полиморфизма."],
                ["Назовите основные этапы разработки приложения.", "Чем модульные тесты отличаются от интеграционных?", "Как работает система контроля версий Git?"],
                ["Чем массив отличается от связного списка?", "Опишите способы обработки исключений.", "Что такое рекурсия? Приведите пример."],
                ["Что такое реляционная база данных?", "Объясните назначение первичного и внешнего ключа.", "Напишите запрос для выбора данных с условием."],
                ["Какие задачи решает графический интерфейс?", "Объясните механизм обработки событий.", "Как разделить интерфейс и бизнес-логику приложения?"],
                ["Что такое файловый поток?", "Как безопасно сохранить данные в файл?", "Чем форматы XLSX и DOCX отличаются от обычного текста?"],
                ["Объясните принципы асинхронного программирования.", "Чем процесс отличается от потока?", "Для чего нужна синхронизация доступа к данным?"]
            ];
            using var doc = WordprocessingDocument.Create(ticketsPath, WordprocessingDocumentType.Document);
            var part = doc.AddMainDocumentPart();
            var body = new Body();
            for (var i = 0; i < questions.Length; i++)
            {
                body.Append(new Paragraph(new Run(new Text($"Билет {i + 1}"))));
                for (var j = 0; j < 3; j++) body.Append(new Paragraph(new Run(new Text($"{j + 1}. {questions[i][j]}"))));
                body.Append(new Paragraph());
            }
            part.Document = new Document(body);
            part.Document.Save();
        }
    }
    private static void AddGroup(XLWorkbook workbook, string name, Student[] students)
    {
        var sheet = workbook.AddWorksheet(name);
        sheet.Cell(1, 1).Value = "Фамилия"; sheet.Cell(1, 2).Value = "Имя";
        for (var i = 0; i < students.Length; i++) { sheet.Cell(i + 2, 1).Value = students[i].LastName; sheet.Cell(i + 2, 2).Value = students[i].FirstName; }
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Columns(1, 2).Width = 26;
    }
}
