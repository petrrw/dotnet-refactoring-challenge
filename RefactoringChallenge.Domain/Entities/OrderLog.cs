namespace RefactoringChallenge.Domain.Entities;

public class OrderLog
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime LogDate { get; set; }
    public string Message { get; set; } = null!;
}