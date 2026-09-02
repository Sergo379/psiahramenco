namespace ExamTicketGenerator.Tests;

public class TicketServiceTests
{
    [Fact]
    public void GenerateTicketNumber_ReturnsNumberFrom1To20()
    {
        for (int i = 0; i < 10000; i++)
        {
            int ticketNumber = TicketService.GenerateTicketNumber();

            Assert.InRange(ticketNumber, 1, 20);
        }
    }
}