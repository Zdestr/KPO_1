using HSE.Finance.Core.Domain;

namespace HSE.Finance.Core.Interfaces;

public interface IFinanceFacade
{
    BankAccount CreateAccount(string name, decimal initialBalance);
    Category CreateCategory(string name, CategoryType type);
    Operation AddIncome(Guid accountId, Guid categoryId, decimal amount, DateTime date, string description);
    Operation AddExpense(Guid accountId, Guid categoryId, decimal amount, DateTime date, string description);

    IEnumerable<BankAccount> GetAllAccounts();
    IEnumerable<Category> GetAllCategories();
    IEnumerable<Operation> GetAllOperations();
    BankAccount? GetAccountById(Guid id);
    Category? GetCategoryById(Guid id);

    string GetAnalyticsReport();
    string ExportDataToJson();
    void ImportDataFromJson(string filePath);
}
