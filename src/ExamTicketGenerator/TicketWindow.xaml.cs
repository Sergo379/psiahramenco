using System.Windows;
using System.Windows.Input;
namespace ExamTicketGenerator;
public partial class TicketWindow : Window
{
    public TicketWindow(Ticket ticket, Student student, bool repeat)
    {
        InitializeComponent(); NumberText.Text = $"Билет № {ticket.Number}"; RepeatText.Text = repeat ? $"Повторная выдача · {student.DisplayName}" : student.DisplayName; QuestionsList.ItemsSource = ticket.Questions.Select((question, index) => $"{index + 1}. {question}");
    }
    private void KeyPressed(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) { e.Handled = true; Close(); } }
}
