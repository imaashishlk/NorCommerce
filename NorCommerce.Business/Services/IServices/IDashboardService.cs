using NorCommerce.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IDashboardService
    {
        Task<DashboardVM> GetDashboardAsync();
    }
}
