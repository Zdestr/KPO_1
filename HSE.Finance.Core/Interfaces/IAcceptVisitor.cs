namespace HSE.Finance.Core.Interfaces;

public interface IAcceptVisitor
{
    void Accept(IVisitor visitor);
}
