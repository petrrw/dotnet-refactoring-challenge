namespace RefactoringChallenge.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsVip { get; set; }
    public DateTime RegistrationDate { get; set; }
}
