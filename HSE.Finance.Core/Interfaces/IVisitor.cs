using HSE.Finance.Core.Domain;

namespace HSE.Finance.Core.Interfaces;

public interface IVisitor
{
    void Visit(BankAccount bankAccount);
    void Visit(Category category);
    void Visit(Operation operation);
}
