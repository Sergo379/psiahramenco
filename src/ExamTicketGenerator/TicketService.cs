namespace ExamTicketGenerator;

public static class TicketService
{
    public static int GenerateTicketNumber()
    {
        return Random.Shared.Next(1, 21);
    }
}