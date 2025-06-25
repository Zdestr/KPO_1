using System.Text.Json;
using HSE.Finance.Core.Domain;
using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Services;

public class JsonExportVisitor : IVisitor
{
    private readonly List<BankAccount> _accounts = [];
    private readonly List<Category> _categories = [];
    private readonly List<Operation> _operations = [];

    public void Visit(BankAccount account) => _accounts.Add(account);
    public void Visit(Category category) => _categories.Add(category);
    public void Visit(Operation operation) => _operations.Add(operation);

    public string GetResult()
    {
        var data = new { Accounts = _accounts, Categories = _categories, Operations = _operations };
        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true });
    }
}

public record ImportDataDto(List<BankAccount> Accounts, List<Category> Categories, List<Operation> Operations);

public abstract class BaseDataImporter
{
    protected readonly IRepository<BankAccount> AccountRepo;
    protected readonly IRepository<Category> CategoryRepo;
    protected readonly IRepository<Operation> OperationRepo;

    protected BaseDataImporter(IRepository<BankAccount> accountRepo, IRepository<Category> categoryRepo, IRepository<Operation> operationRepo)
    {
        AccountRepo = accountRepo;
        CategoryRepo = categoryRepo;
        OperationRepo = operationRepo;
    }

    public void Import(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("Файл импорта не найден.", filePath);
        var fileContent = File.ReadAllText(filePath);
        var data = ParseData(fileContent);

        foreach(var item in AccountRepo.GetAll()) AccountRepo.Delete(item.Id);
        foreach(var item in CategoryRepo.GetAll()) CategoryRepo.Delete(item.Id);
        foreach(var item in OperationRepo.GetAll()) OperationRepo.Delete(item.Id);
        
        data.Accounts.ForEach(AccountRepo.Add);
        data.Categories.ForEach(CategoryRepo.Add);
        data.Operations.ForEach(OperationRepo.Add);
        Console.WriteLine("Данные успешно импортированы.");
    }
    
    protected abstract ImportDataDto ParseData(string content);
}

public class JsonDataImporter : BaseDataImporter
{
    public JsonDataImporter(IRepository<BankAccount> accountRepo, IRepository<Category> categoryRepo, IRepository<Operation> operationRepo)
        : base(accountRepo, categoryRepo, operationRepo) { }

    protected override ImportDataDto ParseData(string content)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ImportDataDto>(content, options) 
            ?? throw new InvalidDataException("Не удалось распарсить JSON.");
    }
}
