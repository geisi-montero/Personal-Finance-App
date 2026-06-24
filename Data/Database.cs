using Microsoft.Data.Sqlite;
using FinanzasApp.Models;
using System.IO;

namespace FinanzasApp.Data
{
    public static class DatabaseInitializer
    {
        private static string DbPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FinanzasApp", "finanzas.db");

        public static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Icon TEXT DEFAULT '📦',
                    Type INTEGER NOT NULL,
                    Color TEXT DEFAULT '#6366F1'
                );

                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Type INTEGER NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    Notes TEXT DEFAULT '',
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                );

                CREATE TABLE IF NOT EXISTS Budgets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryId INTEGER NOT NULL,
                    LimitAmount REAL NOT NULL,
                    Month INTEGER NOT NULL,
                    Year INTEGER NOT NULL,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                );
            ";
            cmd.ExecuteNonQuery();

            // Seed default categories if empty
            cmd.CommandText = "SELECT COUNT(*) FROM Categories";
            var count = (long)(cmd.ExecuteScalar() ?? 0L);
            if (count == 0)
            {
                SeedCategories(conn);
            }
        }

        private static void SeedCategories(SqliteConnection conn)
        {
            var categories = new[]
            {
                ("Salario", "💼", 0, "#10B981"),
                ("Freelance", "💻", 0, "#6366F1"),
                ("Inversiones", "📈", 0, "#3B82F6"),
                ("Otros Ingresos", "💰", 0, "#8B5CF6"),
                ("Alimentación", "🍔", 1, "#EF4444"),
                ("Transporte", "🚗", 1, "#F59E0B"),
                ("Entretenimiento", "🎬", 1, "#EC4899"),
                ("Salud", "🏥", 1, "#14B8A6"),
                ("Educación", "📚", 1, "#8B5CF6"),
                ("Hogar", "🏠", 1, "#F97316"),
                ("Ropa", "👗", 1, "#A855F7"),
                ("Tecnología", "📱", 1, "#3B82F6"),
                ("Servicios", "⚡", 1, "#6B7280"),
                ("Otros Gastos", "📦", 1, "#9CA3AF"),
            };

            using var cmd = conn.CreateCommand();
            foreach (var (name, icon, type, color) in categories)
            {
                cmd.CommandText = "INSERT INTO Categories (Name, Icon, Type, Color) VALUES (@n, @i, @t, @c)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@i", icon);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@c", color);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class TransactionRepository
    {
        private string ConnStr => DatabaseInitializer.ConnectionString;

        public List<Transaction> GetAll(DateTime? from = null, DateTime? to = null, int? categoryId = null, TransactionType? type = null)
        {
            var list = new List<Transaction>();
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.Id, t.Title, t.Amount, t.Type, t.CategoryId, t.Date, t.Notes,
                       c.Name as CategoryName, c.Icon as CategoryIcon
                FROM Transactions t
                JOIN Categories c ON t.CategoryId = c.Id
                WHERE 1=1
            ";
            if (from.HasValue) { cmd.CommandText += " AND t.Date >= @from"; cmd.Parameters.AddWithValue("@from", from.Value.ToString("yyyy-MM-dd")); }
            if (to.HasValue) { cmd.CommandText += " AND t.Date <= @to"; cmd.Parameters.AddWithValue("@to", to.Value.ToString("yyyy-MM-dd")); }
            if (categoryId.HasValue) { cmd.CommandText += " AND t.CategoryId = @cat"; cmd.Parameters.AddWithValue("@cat", categoryId.Value); }
            if (type.HasValue) { cmd.CommandText += " AND t.Type = @type"; cmd.Parameters.AddWithValue("@type", (int)type.Value); }
            cmd.CommandText += " ORDER BY t.Date DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Transaction
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Amount = (decimal)reader.GetDouble(2),
                    Type = (TransactionType)reader.GetInt32(3),
                    CategoryId = reader.GetInt32(4),
                    Date = DateTime.Parse(reader.GetString(5)),
                    Notes = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    CategoryName = reader.GetString(7),
                    CategoryIcon = reader.GetString(8)
                });
            }
            return list;
        }

        public void Add(Transaction t)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Transactions (Title, Amount, Type, CategoryId, Date, Notes) VALUES (@title, @amt, @type, @cat, @date, @notes)";
            cmd.Parameters.AddWithValue("@title", t.Title);
            cmd.Parameters.AddWithValue("@amt", (double)t.Amount);
            cmd.Parameters.AddWithValue("@type", (int)t.Type);
            cmd.Parameters.AddWithValue("@cat", t.CategoryId);
            cmd.Parameters.AddWithValue("@date", t.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@notes", t.Notes ?? "");
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Transactions WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public DashboardStats GetDashboardStats(int month, int year)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();

            var stats = new DashboardStats();
            var cmd = conn.CreateCommand();

            // Income & Expenses for month
            cmd.CommandText = @"
                SELECT Type, SUM(Amount) FROM Transactions
                WHERE strftime('%m', Date) = @month AND strftime('%Y', Date) = @year
                GROUP BY Type";
            cmd.Parameters.AddWithValue("@month", month.ToString("D2"));
            cmd.Parameters.AddWithValue("@year", year.ToString());
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    var t = r.GetInt32(0);
                    var amt = (decimal)r.GetDouble(1);
                    if (t == 0) stats.TotalIncome = amt;
                    else stats.TotalExpenses = amt;
                }
            }

            stats.Balance = stats.TotalIncome - stats.TotalExpenses;
            // Top expense categories
            cmd.CommandText = @"
                SELECT c.Name, c.Icon, c.Color, SUM(t.Amount) as total
                FROM Transactions t JOIN Categories c ON t.CategoryId = c.Id
                WHERE t.Type = 1 AND strftime('%m', t.Date) = @month AND strftime('%Y', t.Date) = @year
                GROUP BY c.Id ORDER BY total DESC LIMIT 5";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@month", month.ToString("D2"));
            cmd.Parameters.AddWithValue("@year", year.ToString());
            var catStats = new List<CategoryStat>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    catStats.Add(new CategoryStat
                    {
                        CategoryName = r.GetString(0),
                        Icon = r.GetString(1),
                        Color = r.GetString(2),
                        Amount = (decimal)r.GetDouble(3)
                    });
            }
            var totalExp = catStats.Sum(c => c.Amount);
            foreach (var c in catStats)
                c.Percentage = totalExp > 0 ? (double)(c.Amount / totalExp * 100) : 0;
            stats.TopExpenseCategories = catStats;

            // Last 6 months
            var monthlyData = new List<MonthlyData>();
            for (int i = 5; i >= 0; i--)
            {
                var d = new DateTime(year, month, 1).AddMonths(-i);
                cmd.CommandText = @"
                    SELECT Type, SUM(Amount) FROM Transactions
                    WHERE strftime('%m', Date) = @m AND strftime('%Y', Date) = @y
                    GROUP BY Type";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@m", d.Month.ToString("D2"));
                cmd.Parameters.AddWithValue("@y", d.Year.ToString());
                var md = new MonthlyData { Month = d.ToString("MMM") };
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var t = r.GetInt32(0);
                    var a = (decimal)r.GetDouble(1);
                    if (t == 0) md.Income = a;
                    else md.Expenses = a;
                }
                monthlyData.Add(md);
            }
            stats.MonthlyData = monthlyData;
            return stats;
        }
    }

    public class CategoryRepository
    {
        private string ConnStr => DatabaseInitializer.ConnectionString;

        public List<Category> GetAll(TransactionType? type = null)
        {
            var list = new List<Category>();
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Icon, Type, Color FROM Categories";
            if (type.HasValue) { cmd.CommandText += " WHERE Type = @type"; cmd.Parameters.AddWithValue("@type", (int)type.Value); }
            cmd.CommandText += " ORDER BY Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1), Icon = reader.GetString(2), Type = (TransactionType)reader.GetInt32(3), Color = reader.GetString(4) });
            return list;
        }

        public void Add(Category c)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Categories (Name, Icon, Type, Color) VALUES (@n, @i, @t, @c)";
            cmd.Parameters.AddWithValue("@n", c.Name);
            cmd.Parameters.AddWithValue("@i", c.Icon);
            cmd.Parameters.AddWithValue("@t", (int)c.Type);
            cmd.Parameters.AddWithValue("@c", c.Color);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Categories WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public class BudgetRepository
    {
        private string ConnStr => DatabaseInitializer.ConnectionString;

        public List<Budget> GetByMonth(int month, int year)
        {
            var list = new List<Budget>();
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT b.Id, b.CategoryId, b.LimitAmount, b.Month, b.Year,
                       c.Name, c.Icon,
                       COALESCE((SELECT SUM(t.Amount) FROM Transactions t
                        WHERE t.CategoryId = b.CategoryId
                        AND strftime('%m', t.Date) = @month
                        AND strftime('%Y', t.Date) = @year
                        AND t.Type = 1), 0) as Spent
                FROM Budgets b
                JOIN Categories c ON b.CategoryId = c.Id
                WHERE b.Month = @m AND b.Year = @y";
            cmd.Parameters.AddWithValue("@month", month.ToString("D2"));
            cmd.Parameters.AddWithValue("@m", month);
            cmd.Parameters.AddWithValue("@y", year);
            cmd.Parameters.AddWithValue("@year", year.ToString());
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var budget = new Budget
                {
                    Id = reader.GetInt32(0),
                    CategoryId = reader.GetInt32(1),
                    LimitAmount = (decimal)reader.GetDouble(2),
                    Month = reader.GetInt32(3),
                    Year = reader.GetInt32(4),
                    CategoryName = reader.GetString(5),
                    CategoryIcon = reader.GetString(6),
                    SpentAmount = (decimal)reader.GetDouble(7)
                };
                budget.RecalcDerived();
                list.Add(budget);
            }
            return list;
        }

        public void Save(Budget b)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Budgets (CategoryId, LimitAmount, Month, Year)
                VALUES (@cat, @limit, @month, @year)
                ON CONFLICT DO UPDATE SET LimitAmount = @limit";
            // For SQLite, check if exists first
            cmd.CommandText = "SELECT Id FROM Budgets WHERE CategoryId=@cat AND Month=@month AND Year=@year";
            cmd.Parameters.AddWithValue("@cat", b.CategoryId);
            cmd.Parameters.AddWithValue("@month", b.Month);
            cmd.Parameters.AddWithValue("@year", b.Year);
            cmd.Parameters.AddWithValue("@limit", (double)b.LimitAmount);
            var existing = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            if (existing != null)
            {
                cmd.CommandText = "UPDATE Budgets SET LimitAmount=@limit WHERE CategoryId=@cat AND Month=@month AND Year=@year";
            }
            else
            {
                cmd.CommandText = "INSERT INTO Budgets (CategoryId, LimitAmount, Month, Year) VALUES (@cat, @limit, @month, @year)";
            }
            cmd.Parameters.AddWithValue("@cat", b.CategoryId);
            cmd.Parameters.AddWithValue("@month", b.Month);
            cmd.Parameters.AddWithValue("@year", b.Year);
            cmd.Parameters.AddWithValue("@limit", (double)b.LimitAmount);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Budgets WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
