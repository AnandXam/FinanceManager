using SQLite;
using FinanceManager.Models;

namespace FinanceManager.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "financemanager.db3");
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<Transaction>();
        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<UserProfile>();

        return _database;
    }

    // ─── User Operations ───────────────────────────────────────────

    public async Task<UserProfile?> GetUserAsync(string userId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<UserProfile>().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task SaveUserAsync(UserProfile user)
    {
        var db = await GetDatabaseAsync();
        var existing = await db.Table<UserProfile>().FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existing is not null)
            await db.UpdateAsync(user);
        else
            await db.InsertAsync(user);
    }

    // ─── Transaction Operations ────────────────────────────────────

    public async Task<List<Transaction>> GetTransactionsAsync(string userId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(string userId, DateTime start, DateTime end)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>()
            .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByTypeAsync(string userId, TransactionType type)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>()
            .Where(t => t.UserId == userId && t.Type == type)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetRecentTransactionsAsync(string userId, int count = 10)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> SaveTransactionAsync(Transaction transaction)
    {
        var db = await GetDatabaseAsync();
        if (transaction.Id != 0)
        {
            await db.UpdateAsync(transaction);
            return transaction.Id;
        }
        else
        {
            await db.InsertAsync(transaction);
            return transaction.Id;
        }
    }

    public async Task DeleteTransactionAsync(Transaction transaction)
    {
        var db = await GetDatabaseAsync();
        await db.DeleteAsync(transaction);
    }

    // ─── Category Operations ───────────────────────────────────────

    public async Task<List<Category>> GetCategoriesAsync(string userId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Category>()
            .Where(c => c.UserId == userId || c.IsDefault)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesByTypeAsync(string userId, TransactionType type)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Category>()
            .Where(c => (c.UserId == userId || c.IsDefault) && c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<int> SaveCategoryAsync(Category category)
    {
        var db = await GetDatabaseAsync();
        if (category.Id != 0)
        {
            await db.UpdateAsync(category);
            return category.Id;
        }
        else
        {
            await db.InsertAsync(category);
            return category.Id;
        }
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        var db = await GetDatabaseAsync();
        await db.DeleteAsync(category);
    }

    public async Task<bool> HasCategoriesAsync(string userId)
    {
        var db = await GetDatabaseAsync();
        var count = await db.Table<Category>()
            .Where(c => c.UserId == userId || c.IsDefault)
            .CountAsync();
        return count > 0;
    }

    // ─── Aggregation Queries ───────────────────────────────────────

    public async Task<decimal> GetTotalIncomeAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var transactions = await GetTransactionsAsync(userId);
        var query = transactions.Where(t => t.Type == TransactionType.Income);

        if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue) query = query.Where(t => t.Date <= to.Value);

        return query.Sum(t => t.Amount);
    }

    public async Task<decimal> GetTotalExpensesAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var transactions = await GetTransactionsAsync(userId);
        var query = transactions.Where(t => t.Type == TransactionType.Expense);

        if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue) query = query.Where(t => t.Date <= to.Value);

        return query.Sum(t => t.Amount);
    }

    public async Task<List<MonthlySummary>> GetMonthlySummariesAsync(string userId, int months = 6)
    {
        var transactions = await GetTransactionsAsync(userId);
        var startDate = DateTime.Now.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1);

        var summaries = new List<MonthlySummary>();

        for (int i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthTransactions = transactions
                .Where(t => t.Date >= monthStart && t.Date <= monthEnd)
                .ToList();

            summaries.Add(new MonthlySummary
            {
                Month = monthStart.Month,
                Year = monthStart.Year,
                TotalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            });
        }

        return summaries;
    }
}
