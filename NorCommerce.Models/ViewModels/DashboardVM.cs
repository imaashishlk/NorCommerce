using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class DashboardVM
    {
        public decimal RevenueTotal{ get; set; }
        public int OrdersTotal{ get; set; }
        public int ProductsTotal{ get; set; }
        public int UsersTotal { get; set; }

        // Chart 1 + 2
        public List<string> MonthLabels { get; set; } = new();
        public List<int> MonthlyOrders { get; set; } = new();
        public List<double> MonthlyRevenue { get; set; } = new();

        // Chart 3
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();

        // Chart 4
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();
    }
}
