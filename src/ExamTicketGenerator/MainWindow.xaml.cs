using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ExamTicketGenerator;

public partial class MainWindow : Window
{
    private readonly string _studentsPath = Path.Combine(AppContext.BaseDirectory, "students.xlsx");
    private readonly string _ticketsPath = Path.Combine(AppContext.BaseDirectory, "tickets.docx");
    private readonly ExcelJournal _journal;
    private IReadOnlyList<Group> _groups = [];
    private TicketService? _ticketService;

    public MainWindow()
    {
        InitializeComponent();
        _journal = new ExcelJournal(Path.Combine(AppContext.BaseDirectory, "results.xlsx"));
        Loaded += (_, _) => LoadSources();
    }

    private void LoadSources()
    {
        var missing = new List<string>();
        if (!File.Exists(_studentsPath)) missing.Add("students.xlsx");
        if (!File.Exists(_ticketsPath)) missing.Add("tickets.docx");
        if (missing.Count > 0) { ShowStatus($"Не найдены: {string.Join(", ", missing)}", true); MessageBox.Show($"Поместите рядом с приложением файлы: {string.Join(", ", missing)}.", "Не хватает входных данных", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        try
        {
            _groups = StudentsReader.Read(_studentsPath);
            var logPath = Path.Combine(AppContext.BaseDirectory, "tickets-parser.log");
            var warnings = new List<string>();
            var tickets = TicketsReader.Read(_ticketsPath, warnings.Add);
            File.WriteAllLines(logPath, warnings);
            if (tickets.Count == 0) { ShowStatus("В tickets.docx нет корректных билетов.", true); return; }
            _ticketService = new TicketService(tickets);
            GroupCombo.ItemsSource = _groups;
            ShowStatus($"Загружено групп: {_groups.Count} · билетов: {tickets.Count}", false);
        }
        catch (Exception ex) { ShowStatus("Не удалось прочитать входные файлы.", true); MessageBox.Show(ex.Message, "Ошибка чтения", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void GroupChanged(object sender, SelectionChangedEventArgs e)
    {
        StudentCombo.ItemsSource = (GroupCombo.SelectedItem as Group)?.Students ?? [];
        StudentCombo.SelectedIndex = -1; GenerateButton.IsEnabled = false;
    }

    private void StudentChanged(object sender, SelectionChangedEventArgs e) => GenerateButton.IsEnabled = GroupCombo.SelectedItem is Group && StudentCombo.SelectedItem is Student && _ticketService is not null;

    private void GenerateClicked(object sender, RoutedEventArgs e)
    {
        if (GroupCombo.SelectedItem is not Group group || StudentCombo.SelectedItem is not Student student || _ticketService is null) return;
        var number = _ticketService.ChooseTicketNumber(group.Name, student, _journal, out var repeat);
        var entry = new JournalEntry(group.Name, student.LastName, student.FirstName, number, DateTime.Now, repeat);
        while (true)
        {
            try { _journal.Append(entry); break; }
            catch (JournalFileAccessException ex)
            {
                var answer = MessageBox.Show($"{ex.Message}\n\nЗакройте results.xlsx и нажмите «Повторить».", "Файл занят", MessageBoxButton.RetryCancel, MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Retry) return;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error); return; }
        }
        new TicketWindow(_ticketService.GetTicket(number), student, repeat) { Owner = this }.ShowDialog();
        StatusText.Text = repeat ? "Повтор сохранён в журнал · можно выбрать следующего студента" : "Билет сохранён в журнал · можно выбрать следующего студента";
    }

    private void ShowStatus(string text, bool error) { StatusText.Text = text; StatusText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(error ? "#FCA5A5" : "#94A3B8")); }
}
