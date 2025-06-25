using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Core.Domain;

public class BankAccount : IAcceptVisitor
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public decimal Balance { get; set; }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}
