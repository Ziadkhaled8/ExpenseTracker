using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DateTime StartDate= DateTime.Today.AddDays(-6);
            DateTime EndDate= DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
               .Include(x => x.Category)
               .Where(y => y.Date >= StartDate && y.Date <= EndDate)
               .ToListAsync();

            int TotalIncome = SelectedTransactions
                .Where(s => s.Category.Type == "Income")
                .Sum(s => s.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("c0");

            int TotalExpense = SelectedTransactions
                .Where(s => s.Category.Type == "Expense")
                .Sum(s => s.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("c0");

            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture, "{0:c0}", Balance);

            //DoughnutChart -Expense Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(s => s.Category.Type == "Expense")
                .GroupBy(s => s.Category.CategoryId)
                .Select(s => new
                {
                    categoryTitleWithIcon = s.First().Category.Icon + " " + s.First().Category.Title,
                    amount = s.Sum(s => s.Amount),
                    formattedAmount = s.Sum(s => s.Amount).ToString("c0"),
                })
                .OrderByDescending(x=>x.amount)
                .ToList();
            //splice chart income vs expense
            //income
            List<SplineChartData> IncomeSummary= SelectedTransactions
                .Where(s => s.Category.Type == "Income")
                .GroupBy(x=>x.Date)
                .Select(x=> new SplineChartData()
                {
                    day=x.First().Date.ToString("dd-MMM"),
                    income=x.Sum(x=>x.Amount)
                })
                .ToList();
            //expense
            List<SplineChartData> ExpenseSummary= SelectedTransactions
                .Where(s => s.Category.Type == "Expense")
                .GroupBy(x=>x.Date)
                .Select(x=> new SplineChartData()
                {
                    day=x.First().Date.ToString("dd-MMM"),
                    expense=x.Sum(x=>x.Amount)
                })
                .ToList();
            //combined
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(y => StartDate.AddDays(y).ToString("dd-MMM"))
                .ToArray();
            ViewBag.SplineChartData = from day in Last7Days
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
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
