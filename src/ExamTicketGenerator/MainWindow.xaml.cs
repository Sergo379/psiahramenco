using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamTicketGenerator;

public partial class MainWindow : Window
{
    private readonly string _directory;
    private readonly ExcelJournal _journal;
    private TicketService? _ticketService;
    private bool _loading;

    public MainWindow() : this(AppContext.BaseDirectory) { }
    public MainWindow(string directory)
    {
        _directory = directory;
        _journal = new ExcelJournal(Path.Combine(directory, "results.xlsx"));
        InitializeComponent();
        Loaded += (_, _) => LoadSources();
    }

    private void LoadSources()
    {
        _loading = true;
        _ticketService = null;
        GroupCombo.ItemsSource = null;
        StudentCombo.ItemsSource = null;
        GenerateButton.IsEnabled = false;
        StudentCombo.IsEnabled = false;
        GroupsCountText.Text = StudentsCountText.Text = TicketsCountText.Text = "—";
        ReadyText.Text = "Нет данных";
        try
        {
            _journal.EnsureCreated();
            var studentsPath = Path.Combine(_directory, "students.xlsx");
            var ticketsPath = Path.Combine(_directory, "tickets.docx");
            var missing = new[] { studentsPath, ticketsPath }.Where(path => !File.Exists(path)).Select(Path.GetFileName).ToArray();
            if (missing.Length > 0)
            {
                ShowStatus($"Не найдены: {string.Join(", ", missing)}. Добавьте файлы в папку данных и нажмите «Обновить источники».", true);
                return;
            }
            var groups = StudentsReader.Read(studentsPath);
            var warnings = new List<string>();
            var tickets = TicketsReader.Read(ticketsPath, warnings.Add);
            File.AppendAllLines(Path.Combine(_directory, "tickets-parser.log"), warnings.Select(message => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}"));
            if (tickets.Count == 0) { ShowStatus("В Word нет корректных билетов с тремя вопросами. Проверьте файл и обновите источники.", true); return; }
            _ticketService = new TicketService(tickets);
            GroupCombo.ItemsSource = groups;
            GroupsCountText.Text = groups.Count.ToString("00");
            StudentsCountText.Text = groups.Sum(group => group.Students.Count).ToString("00");
            TicketsCountText.Text = tickets.Count.ToString("00");
            ReadyText.Text = "Готово к выдаче";
            SelectionHint.Text = "Начните с выбора группы.";
            ShowStatus(warnings.Count == 0 ? "Источники загружены. Можно начинать." : $"Источники загружены. Замечаний в журнале парсинга: {warnings.Count}.", false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Не удалось загрузить данные: {ex.Message} Закройте файлы в Excel/Word и обновите источники.", true);
        }
        finally { _loading = false; }
    }

    private void GroupChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StudentCombo is null) return;
        var group = GroupCombo.SelectedItem as Group;
        StudentCombo.ItemsSource = group?.Students;
        StudentCombo.SelectedIndex = -1;
        StudentCombo.IsEnabled = group?.Students.Count > 0;
        GenerateButton.IsEnabled = false;
        if (!_loading) SelectionHint.Text = group is null ? "Начните с выбора группы." :
            group.Students.Count == 0 ? "В этой группе нет студентов. Выберите другую группу." : $"В группе студентов: {group.Students.Count}. Выберите участника.";
    }

    private void StudentChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GenerateButton is null) return;
        GenerateButton.IsEnabled = !_loading && GroupCombo.SelectedItem is Group && StudentCombo.SelectedItem is Student && _ticketService is not null;
        if (StudentCombo.SelectedItem is Student student) SelectionHint.Text = $"Всё готово к выдаче: {student.DisplayName}.";
    }

    private void GenerateClicked(object sender, RoutedEventArgs e)
    {
        if (GroupCombo.SelectedItem is not Group group || StudentCombo.SelectedItem is not Student student || _ticketService is null) return;
        GenerateButton.IsEnabled = false;
        try
        {
            while (true)
            {
                try
                {
                    // Both lookup and save are inside the retry boundary.
                    var number = _ticketService.ChooseTicketNumber(group.Name, student, _journal, out var repeat);
                    var ticket = _ticketService.GetTicket(number);
                    _journal.Append(new JournalEntry(group.Name, student.LastName, student.FirstName, number, DateTime.Now, repeat));
                    new TicketWindow(ticket, student, repeat) { Owner = this }.ShowDialog();
                    StudentCombo.SelectedIndex = -1;
                    ShowStatus($"Билет № {number} сохранён · {student.DisplayName}{(repeat ? " · повтор" : "")}. Выберите следующего студента.", false);
                    return;
                }
                catch (Exception ex) when (ex is JournalFileAccessException or IOException or UnauthorizedAccessException)
                {
                    if (MessageBox.Show(this, "Не удалось открыть или сохранить results.xlsx. Закройте файл в Excel и нажмите «Да», чтобы повторить попытку.\n\n" + ex.Message,
                        "Журнал недоступен · повторить?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                }
                catch (Exception ex)
                {
                    ShowStatus($"Выдача не выполнена: {ex.Message}", true);
                    return;
                }
            }
        }
        finally { GenerateButton.IsEnabled = StudentCombo.SelectedItem is Student && _ticketService is not null; }
    }

    private void ReloadClicked(object sender, RoutedEventArgs e) => LoadSources();
    private void FolderClicked(object sender, RoutedEventArgs e) => OpenPath(_directory);
    private void JournalClicked(object sender, RoutedEventArgs e)
    {
        try { _journal.EnsureCreated(); OpenPath(Path.Combine(_directory, "results.xlsx")); }
        catch (Exception ex) { ShowStatus(ex.Message, true); }
    }
    private void OpenPath(string path)
    {
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex) { ShowStatus($"Не удалось открыть: {ex.Message}", true); }
    }
    private void ShowStatus(string text, bool error)
    {
        StatusText.Text = text;
        StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(error ? "#EAA583" : "#A3A697"));
    }
}
