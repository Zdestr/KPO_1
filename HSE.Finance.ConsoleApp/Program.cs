using System;
using System.Globalization;
using HSE.Finance.Core.Domain;
using HSE.Finance.Core.Interfaces;
using HSE.Finance.Services;
using HSE.Finance.Services.Repositories;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IRepository<BankAccount>, InMemoryRepository<BankAccount>>();
services.AddSingleton<IRepository<Category>, InMemoryRepository<Category>>();
services.AddSingleton<IRepository<Operation>, InMemoryRepository<Operation>>();

services.AddTransient<DomainFactory>();
services.AddTransient<IFinanceFacade, FinanceFacade>();

var serviceProvider = services.BuildServiceProvider();
var facade = serviceProvider.GetRequiredService<IFinanceFacade>();

var account1Id = facade.CreateAccount("Карта Сбер", 50000).Id;
var account2Id = facade.CreateAccount("Наличные", 15000).Id;
var catZpId = facade.CreateCategory("Зарплата", CategoryType.Income).Id;
var catCafeId = facade.CreateCategory("Кафе и рестораны", CategoryType.Expense).Id;
var catProductId = facade.CreateCategory("Продукты", CategoryType.Expense).Id;
facade.AddIncome(account1Id, catZpId, 70000, DateTime.Now.AddDays(-10), "Аванс");
facade.AddExpense(account1Id, catProductId, 3500, DateTime.Now.AddDays(-5), "Закупка в Ашане");
facade.AddExpense(account2Id, catCafeId, 1200, DateTime.Now.AddDays(-2), "Ужин в кафе");
Console.Clear();
Console.WriteLine("--- Система Учета Финансов ВШЭ-Банка ---");
Console.WriteLine("Демонстрационные данные были загружены.\n");

while (true)
{
    PrintMenu();
    var choice = Console.ReadLine();
    try
    {
        switch (choice)
        {
            case "1": HandleCreateAccount(); break;
            case "2": HandleCreateCategory(); break;
            case "3": HandleAddOperation(OperationType.Income); break;
            case "4": HandleAddOperation(OperationType.Expense); break;
            case "5": ShowAllAccounts(); break;
            case "6": ShowAllOperations(); break;
            case "7": ShowAnalytics(); break;
            case "8": HandleExport(); break;
            case "9": HandleImport(); break;
            case "0": return;
            default: Console.WriteLine("Неверный ввод, попробуйте еще раз."); break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nОШИБКА: {ex.Message}");
        Console.ResetColor();
    }
    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
    Console.ReadKey();
    Console.Clear();
}

void PrintMenu()
{
    Console.WriteLine("================ Меню ================");
    Console.WriteLine("1. Создать новый счет");
    Console.WriteLine("2. Создать новую категорию");
    Console.WriteLine("3. Добавить доход");
    Console.WriteLine("4. Добавить расход");
    Console.WriteLine("------------------------------------");
    Console.WriteLine("5. Показать все счета и балансы");
    Console.WriteLine("6. Показать все операции");
    Console.WriteLine("7. Показать аналитику");
    Console.WriteLine("------------------------------------");
    Console.WriteLine("8. Экспорт данных в JSON");
    Console.WriteLine("9. Импорт данных из JSON");
    Console.WriteLine("0. Выход");
    Console.Write("Ваш выбор: ");
}

void HandleCreateAccount()
{
    Console.Write("Введите название счета: ");
    var name = Console.ReadLine() ?? "";
    Console.Write("Введите начальный баланс: ");
    var balance = decimal.Parse(Console.ReadLine() ?? "0");

    ICommand cmd = new CreateAccountCommand(facade, name, balance);
    var timedCmd = new TimingCommandDecorator(cmd); 
    timedCmd.Execute();
}

void HandleCreateCategory()
{
    Console.Write("Введите название категории: ");
    var name = Console.ReadLine() ?? "";
    Console.Write("Введите тип (0 - Доход, 1 - Расход): ");
    var type = (CategoryType)int.Parse(Console.ReadLine() ?? "0");
    
    ICommand cmd = new CreateCategoryCommand(facade, name, type);
    var timedCmd = new TimingCommandDecorator(cmd);
    timedCmd.Execute();
}

void HandleAddOperation(OperationType type)
{
    Console.WriteLine($"--- Добавление операции типа: {type} ---");
    var account = SelectAccount();
    if (account == null) return;
    
    var category = SelectCategory(type);
    if (category == null) return;
    
    Console.Write("Введите сумму: ");
    var amount = decimal.Parse(Console.ReadLine() ?? "0");
    Console.Write("Введите описание: ");
    var desc = Console.ReadLine() ?? "";

    ICommand cmd = type == OperationType.Income
        ? new AddIncomeCommand(facade, account.Id, category.Id, amount, DateTime.Now, desc)
        : new AddExpenseCommand(facade, account.Id, category.Id, amount, DateTime.Now, desc);
    
    var timedCmd = new TimingCommandDecorator(cmd);
    timedCmd.Execute();
}

BankAccount? SelectAccount()
{
    Console.WriteLine("Выберите счет:");
    var accounts = facade.GetAllAccounts().ToList();
    for (int i = 0; i < accounts.Count; i++)
        Console.WriteLine($"{i + 1}. {accounts[i].Name} ({accounts[i].Balance:C})");
    
    Console.Write("Номер счета: ");
    if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= accounts.Count)
        return accounts[choice - 1];
    
    Console.WriteLine("Неверный выбор.");
    return null;
}

Category? SelectCategory(OperationType type)
{
    Console.WriteLine($"Выберите категорию (Тип: {type}):");
    var categories = facade.GetAllCategories().Where(c => (int)c.Type == (int)type).ToList();
    for (int i = 0; i < categories.Count; i++)
        Console.WriteLine($"{i + 1}. {categories[i].Name}");

    Console.Write("Номер категории: ");
    if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= categories.Count)
        return categories[choice - 1];

    Console.WriteLine("Неверный выбор.");
    return null;
}


void ShowAllAccounts()
{
    Console.WriteLine("\n--- Все счета ---");
    var accounts = facade.GetAllAccounts();
    foreach (var acc in accounts)
    {
        Console.WriteLine($"ID: {acc.Id} | Название: {acc.Name,-20} | Баланс: {acc.Balance,12:C}");
    }
}

void ShowAllOperations()
{
    Console.WriteLine("\n--- Все операции ---");
    var operations = facade.GetAllOperations().OrderByDescending(o => o.Date);
    foreach (var op in operations)
    {
        var account = facade.GetAccountById(op.BankAccountId);
        var category = facade.GetCategoryById(op.CategoryId);
        var sign = op.Type == OperationType.Income ? "+" : "-";
        Console.ForegroundColor = op.Type == OperationType.Income ? ConsoleColor.Green : ConsoleColor.Yellow;
        Console.WriteLine($"{op.Date:g} | {sign}{op.Amount, -10:C} | Счет: {account?.Name, -15} | Категория: {category?.Name, -20} | {op.Description}");
        Console.ResetColor();
    }
}

void ShowAnalytics()
{
    var report = facade.GetAnalyticsReport();
    Console.WriteLine(report);
}

void HandleExport()
{
    var result = facade.ExportDataToJson();
    Console.WriteLine(result);
}

void HandleImport()
{
    Console.Write("Введите путь к файлу (по умолчанию: export.json): ");
    var path = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(path))
    {
        path = "export.json";
    }
    facade.ImportDataFromJson(path);
}

