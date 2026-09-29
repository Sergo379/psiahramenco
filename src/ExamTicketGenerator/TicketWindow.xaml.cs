using System.Windows;
using System.Windows.Input;
namespace ExamTicketGenerator;

public partial class TicketWindow : Window
{
    public TicketWindow(Ticket ticket, Student student, bool repeat)
    {
        InitializeComponent();
        NumberText.Text = $"Ваш билет № {ticket.Number}";
        StudentText.Text = student.DisplayName;
        RepeatText.Text = repeat ? "Повторная выдача · номер из первой записи" : "Первая выдача · удачи на экзамене!";
        QuestionsList.ItemsSource = ticket.Questions.Select((text, index) => new { Index = $"{index + 1:00}", Text = text });
    }
    private void KeyPressed(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { e.Handled = true; Close(); }
    }
    private void CloseClicked(object sender, RoutedEventArgs e) => Close();
}
