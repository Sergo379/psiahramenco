namespace ExamTicketGenerator;
public sealed class TicketService(IReadOnlyList<Ticket> tickets)
{
    public int ChooseTicketNumber(string group, Student student, ExcelJournal journal, out bool repeat)
    {
        var existing = journal.FindFirst(group, student.LastName, student.FirstName);
        repeat = existing is not null;
        return existing?.TicketNumber ?? tickets[Random.Shared.Next(tickets.Count)].Number;
    }
    public Ticket GetTicket(int number) => tickets.First(ticket => ticket.Number == number);
}
