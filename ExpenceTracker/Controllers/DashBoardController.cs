using ExpenceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenceTracker.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashBoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            //последние 30/7 experiment дней
            DateTime StartDate = DateTime.Today.AddDays(-30);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();
            //весь доход
            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Доход")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");
            //весь расход
            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Расход")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");
            //Баланс
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-Ru");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            //ViewBag.Balance = String.Format(culture, "{0:C0}", Balance);
            ViewBag.Balance = Balance.ToString("C0");

            //dougnut chart -  трата в категории
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Расход")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();
            //Spline Chart Доходы и Расходы
            //Доходы
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Доход")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MM-yy"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();
            //Расходы
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Расход")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MM-yy"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            // Конструктор\Комбинирование расходов и доходов 
            string[] LastMonth = Enumerable.Range(0, 30)
                .Select(i => StartDate.AddDays(i).ToString("dd-MM-yy"))
                .ToArray();

            ViewBag.SplineChartData = from day in LastMonth
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into dayExpenseJoined
                                      from expense in dayExpenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };

            //последние 7 транзакций
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)//упорядочить в порядке убывания 
                .OrderByDescending(j => j.Date)
                .Take(7)
                .ToListAsync();
            return View();
        }
    }
    public class SplineChartData
    {
        public string day;
        public int week;
        public int income;
        public int expense;
    }
}
