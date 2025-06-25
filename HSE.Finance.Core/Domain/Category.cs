using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Core.Domain;

public enum CategoryType { Income, Expense }

public class Category : IAcceptVisitor
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public CategoryType Type { get; init; }

    public void Accept(IVisitor visitor) => visitor.Visit(this);
}
