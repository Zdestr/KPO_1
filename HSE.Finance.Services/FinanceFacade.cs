using HSE.Finance.Core.Domain;
using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Services;

public class FinanceFacade : IFinanceFacade
{
    private readonly IRepository<BankAccount> _accountRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Operation> _operationRepo;
    private readonly DomainFactory _factory;

    public FinanceFacade(IRepository<BankAccount> accountRepo, IRepository<Category> categoryRepo, IRepository<Operation> operationRepo, DomainFactory factory)
    {
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
        _operationRepo = operationRepo;
        _factory = factory;
    }

    public BankAccount CreateAccount(string name, decimal initialBalance)
    {
        var account = _factory.CreateBankAccount(name, initialBalance);
        _accountRepo.Add(account);
        return account;
    }
    
    public Category CreateCategory(string name, CategoryType type)
    {
        var category = _factory.CreateCategory(name, type);
        _categoryRepo.Add(category);
        return category;
    }

    public Operation AddIncome(Guid accountId, Guid categoryId, decimal amount, DateTime date, string description)
    {
        var operation = _factory.CreateOperation(accountId, categoryId, OperationType.Income, amount, date, description);
        _operationRepo.Add(operation);
        return operation;
    }

    public Operation AddExpense(Guid accountId, Guid categoryId, decimal amount, DateTime date, string description)
    {
        var operation = _factory.CreateOperation(accountId, categoryId, OperationType.Expense, amount, date, description);
        _operationRepo.Add(operation);
        return operation;
    }
    
    public IEnumerable<BankAccount> GetAllAccounts() => _accountRepo.GetAll();
    public IEnumerable<Category> GetAllCategories() => _categoryRepo.GetAll();
    public IEnumerable<Operation> GetAllOperations() => _operationRepo.GetAll();
    public BankAccount? GetAccountById(Guid id) => _accountRepo.GetById(id);
    public Category? GetCategoryById(Guid id) => _categoryRepo.GetById(id);
    
    public string GetAnalyticsReport()
    {
        var operations = _operationRepo.GetAll();
        var totalIncome = operations.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
        var totalExpense = operations.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
        return $"----- Аналитика -----\nВсего доходов: {totalIncome:C}\nВсего расходов: {totalExpense:C}\nРазница: {(totalIncome - totalExpense):C}\n--------------------";
    }

    public string ExportDataToJson()
    {
        var visitor = new JsonExportVisitor();
        foreach (var item in _accountRepo.GetAll()) item.Accept(visitor);
        foreach (var item in _categoryRepo.GetAll()) item.Accept(visitor);
        foreach (var item in _operationRepo.GetAll()) item.Accept(visitor);
        
        var json = visitor.GetResult();
        var filePath = "export.json";
        File.WriteAllText(filePath, json);
        return $"Данные экспортированы в файл: {Path.GetFullPath(filePath)}";
    }

    public void ImportDataFromJson(string filePath)
    {
        var importer = new JsonDataImporter(_accountRepo, _categoryRepo, _operationRepo);
        importer.Import(filePath);
    }
}
