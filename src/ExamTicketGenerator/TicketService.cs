namespace ExamTicketGenerator;

public sealed class TicketService
{
    private readonly IReadOnlyList<Ticket> _tickets;
    public TicketService(IReadOnlyList<Ticket> tickets)
    {
        if (tickets.Count == 0) throw new ArgumentException("Нет корректных билетов.", nameof(tickets));
        _tickets = tickets;
    }
    public int ChooseTicketNumber(string group, Student student, ExcelJournal journal, out bool repeat)
    {
        var existing = journal.FindFirst(group, student.LastName, student.FirstName);
        repeat = existing is not null;
        return existing?.TicketNumber ?? _tickets[Random.Shared.Next(_tickets.Count)].Number;
    }
    public Ticket GetTicket(int number) => _tickets.FirstOrDefault(ticket => ticket.Number == number)
        ?? throw new InvalidOperationException($"Билет № {number} из первой записи отсутствует в Word. Восстановите этот билет в tickets.docx и обновите источники.");
}
