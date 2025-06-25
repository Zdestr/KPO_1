using HSE.Finance.Core.Domain;
using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Services;

public class DomainFactory
{
    private readonly IRepository<BankAccount> _accountRepo;
    private readonly IRepository<Category> _categoryRepo;

    public DomainFactory(IRepository<BankAccount> accountRepo, IRepository<Category> categoryRepo)
    {
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
    }

    public BankAccount CreateBankAccount(string name, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя счета не может быть пустым.", nameof(name));
        if (initialBalance < 0)
            throw new ArgumentException("Начальный баланс не может быть отрицательным.", nameof(initialBalance));
        
        return new BankAccount { Id = Guid.NewGuid(), Name = name, Balance = initialBalance };
    }

    public Category CreateCategory(string name, CategoryType type)
    {
         if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя категории не может быть пустым.", nameof(name));

        return new Category { Id = Guid.NewGuid(), Name = name, Type = type };
    }

    public Operation CreateOperation(Guid accountId, Guid categoryId, OperationType type, decimal amount, DateTime date, string description)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма операции должна быть положительной.", nameof(amount));

        var account = _accountRepo.GetById(accountId) ?? throw new KeyNotFoundException("Счет не найден.");
        var category = _categoryRepo.GetById(categoryId) ?? throw new KeyNotFoundException("Категория не найдена.");

        if ((int)category.Type != (int)type)
            throw new InvalidOperationException("Тип операции не совпадает с типом категории.");

        // Атомарное обновление баланса счета
        if (type == OperationType.Expense)
        {
            if (account.Balance < amount) throw new InvalidOperationException("Недостаточно средств на счете.");
            account.Balance -= amount;
        }
        else
        {
            account.Balance += amount;
        }
        _accountRepo.Update(account);

        return new Operation
        {
            Id = Guid.NewGuid(),
            BankAccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Amount = amount,
            Date = date,
            Description = description
        };
    }
}
