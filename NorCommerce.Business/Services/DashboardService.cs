using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using Microsoft.EntityFrameworkCore;
using NorCommerce.Models.ViewModels;
using NorCommerce.Utility;

namespace NorCommerce.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardVM> GetDashboardAsync()
        {
            var vm = new DashboardVM();

            await FillTotalsAsync(vm);
            await FillMonthlyAsync(vm);
            await FillStatusAsync(vm);
            await FillCategoriesAsync(vm);

            return vm;
        }

        private async Task FillTotalsAsync(DashboardVM vm)
        {
            vm.OrdersTotal = await _context.OrderHeaders.CountAsync();
            vm.ProductsTotal = await _context.Products.CountAsync();
            vm.UsersTotal = await _context.ApplicationUsers.CountAsync();
            vm.RevenueTotal = (decimal)await _context.OrderHeaders
                .Where(o => o.PaymentStatus == StaticDetails.PaymentStatusApproved)
                .SumAsync(o => o.OrderTotal);
        }

        private async Task FillMonthlyAsync(DashboardVM vm)
        {
            var from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);

            var monthly = await _context.OrderHeaders
                .Where(o => o.OrderDate >= from)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Orders = g.Count(),
                    Revenue = g.Where(o => o.PaymentStatus == StaticDetails.PaymentStatusApproved)
                               .Sum(o => o.OrderTotal)
                })
                .ToListAsync();

            for (int i = 0; i < 6; i++)
            {
                var month = from.AddMonths(i);
                var row = monthly.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);

                vm.MonthLabels.Add(month.ToString("MMM"));
                vm.MonthlyOrders.Add(row?.Orders ?? 0);
                vm.MonthlyRevenue.Add(row?.Revenue ?? 0);
            }
        }

        private async Task FillStatusAsync(DashboardVM vm)
        {
            var statuses = await _context.OrderHeaders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.StatusLabels = statuses.Select(s => s.Status ?? "Unknown").ToList();
            vm.StatusCounts = statuses.Select(s => s.Count).ToList();
        }

        private async Task FillCategoriesAsync(DashboardVM vm)
        {
            var categories = await _context.Products
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.CategoryLabels = categories.Select(c => c.Name).ToList();
            vm.CategoryCounts = categories.Select(c => c.Count).ToList();
        }
    }
}
