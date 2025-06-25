using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Core.Domain;
public enum OperationType { Income, Expense }

public class Operation : IAcceptVisitor
{
    public Guid Id { get; init; }
    public OperationType Type { get; init; }
    public Guid BankAccountId { get; init; }
    public Guid CategoryId { get; init; }
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}
