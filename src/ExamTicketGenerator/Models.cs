namespace ExamTicketGenerator;
public sealed record Student(string LastName, string FirstName)
{
    public string DisplayName => $"{LastName} {FirstName}";
    public override string ToString() => DisplayName;
}
public sealed record Ticket(int Number, IReadOnlyList<string> Questions);
public sealed record Group(string Name, IReadOnlyList<Student> Students)
{
    public override string ToString() => Name;
}
public sealed record JournalEntry(string Group, string LastName, string FirstName, int TicketNumber, DateTime DateTime, bool IsRepeat);
