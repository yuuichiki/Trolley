using Trolley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.Interfaces
{
    interface IItemsSoldReportData
    {
        bool IsDailyReport();
        DateTime GetDate();
        List<ReportItemSold> GetItemsSold();
        int GetTotalItemsSold();
        string GetTotalIncomeWithCurrency();
        string GetTotalProfitWithCurrency();
        string GetTotalCashProfitWithCurrency();
        string GetTotalQRCodeProfitWithCurrency();
        List<ItemTypeMoneyInfo> GetItemTypeMoneyInfo();
        string GetTotalCashIncomeWithCurrency();
        string GetTotalQRCodeIncomeWithCurrency();
        int GetTotalNumCashSales();
        int GetTotalNumQRCodeSales();
    }
}
