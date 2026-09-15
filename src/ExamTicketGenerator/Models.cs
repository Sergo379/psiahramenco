namespace ExamTicketGenerator;
public sealed record Student(string LastName, string FirstName) { public string DisplayName => $"{LastName} {FirstName}"; }
public sealed record Ticket(int Number, IReadOnlyList<string> Questions);
public sealed record Group(string Name, IReadOnlyList<Student> Students);
public sealed record JournalEntry(string Group, string LastName, string FirstName, int TicketNumber, DateTime DateTime, bool IsRepeat);
