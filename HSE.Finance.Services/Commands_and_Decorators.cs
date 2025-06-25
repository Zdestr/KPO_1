using System.Diagnostics;
using HSE.Finance.Core.Domain;
using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Services;

public class TimingCommandDecorator(ICommand wrappedCommand) : ICommand
{
    public void Execute()
    {
        var stopwatch = Stopwatch.StartNew();
        wrappedCommand.Execute();
        stopwatch.Stop();
        Console.WriteLine($"[INFO]: Выполнение '{wrappedCommand.GetType().Name}' заняло {stopwatch.ElapsedMilliseconds} мс.");
    }
}

public class CreateAccountCommand(IFinanceFacade facade, string name, decimal balance) : ICommand
{
    public void Execute()
    {
        var account = facade.CreateAccount(name, balance);
        Console.WriteLine($"Создан счет: '{account.Name}' (ID: {account.Id}) с балансом {account.Balance:C}");
    }
}

public class CreateCategoryCommand(IFinanceFacade facade, string name, CategoryType type) : ICommand
{
    public void Execute()
    {
        var category = facade.CreateCategory(name, type);
        Console.WriteLine($"Создана категория: '{category.Name}' (Тип: {category.Type})");
    }
}

public class AddIncomeCommand(IFinanceFacade facade, Guid accountId, Guid catId, decimal amount, DateTime date, string desc) : ICommand
{
    public void Execute()
    {
        var op = facade.AddIncome(accountId, catId, amount, date, desc);
        var acc = facade.GetAccountById(op.BankAccountId);
        Console.WriteLine($"Добавлен доход '{op.Description}' на сумму {op.Amount:C}. Новый баланс счета '{acc?.Name}': {acc?.Balance:C}");
    }
}

public class AddExpenseCommand(IFinanceFacade facade, Guid accountId, Guid catId, decimal amount, DateTime date, string desc) : ICommand
{
    public void Execute()
    {
        var op = facade.AddExpense(accountId, catId, amount, date, desc);
        var acc = facade.GetAccountById(op.BankAccountId);
        Console.WriteLine($"Добавлен расход '{op.Description}' на сумму {op.Amount:C}. Новый баланс счета '{acc?.Name}': {acc?.Balance:C}");
    }
}
