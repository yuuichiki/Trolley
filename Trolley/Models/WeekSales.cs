﻿using Trolley.Helpers;
using Trolley.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.Models
{
    class WeekSales : IItemsSoldReportData
    {
        public List<DaySales> AllDaySales { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalCashIncome { get; set; }
        public decimal TotalQRCodeIncome { get; set; }
        public int TotalNumCashSales { get; set; }
        public int TotalNumQRCodeSales { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalCashProfit { get; set; }
        public decimal TotalQRCodeProfit { get; set; }
        public Currency Currency { get; set; }
        public int TotalItemsSold { get; set; }

        public List<ReportItemSold> AllItemsSold { get; private set; }

        public List<ItemTypeMoneyInfo> ItemTypeMoneyBreakdown { get; set; }
        public Dictionary<int, ItemTypeMoneyInfo> ItemTypeIDToMoneyInfo { get; private set; }

        public WeekSales()
        {
            ItemTypeMoneyBreakdown = new List<ItemTypeMoneyInfo>();
            ItemTypeIDToMoneyInfo = new Dictionary<int, ItemTypeMoneyInfo>();
            AllDaySales = new List<DaySales>();
            AllItemsSold = new List<ReportItemSold>();
            TotalNumCashSales = 0;
            TotalNumQRCodeSales = 0;
        }

        public string TotalIncomeWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalIncome, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalIncome);
            }
        }

        public string TotalCashIncomeWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalCashIncome, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalCashIncome);
            }
        }

        public string TotalQRCodeIncomeWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalQRCodeIncome, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalQRCodeIncome);
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalProfit, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalProfit);
            }
        }

        public string TotalCashProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalCashProfit, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalCashProfit);
            }
        }

        public string TotalQRCodeProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalQRCodeProfit, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalQRCodeProfit);
            }
        }

        public static WeekSales GenerateDataForWeek(DateTime date, int userID = -1)
        {
            WeekSales weekSales = new WeekSales();
            weekSales.Date = date;
            var allItemsSoldReports = new List<ReportItemSold>();

            var currencies = Currency.LoadCurrencies();
            foreach (Currency currency in currencies)
            {
                if (currency.IsDefaultCurrency)
                {
                    weekSales.Currency = currency;
                    break;
                }
            }

            for (int i = 0; i < 7; i++) // get all sales for the week
            {
                DaySales sales = DaySales.GenerateDataForSingleDay(date.AddDays(i), userID);
                weekSales.AllDaySales.Add(sales);
                weekSales.TotalIncome += Utilities.ConvertAmount(sales.TotalIncome, sales.Currency, weekSales.Currency);
                weekSales.TotalCashIncome += Utilities.ConvertAmount(sales.TotalCashIncome, sales.Currency, weekSales.Currency);
                weekSales.TotalQRCodeIncome += Utilities.ConvertAmount(sales.TotalQRCodeIncome, sales.Currency, weekSales.Currency);
                weekSales.TotalNumCashSales += sales.TotalNumCashSales;
                weekSales.TotalNumQRCodeSales += sales.TotalNumQRCodeSales;
                weekSales.TotalProfit += Utilities.ConvertAmount(sales.TotalProfit, sales.Currency, weekSales.Currency);
                weekSales.TotalCashProfit += Utilities.ConvertAmount(sales.TotalCashProfit, sales.Currency, weekSales.Currency);
                weekSales.TotalQRCodeProfit += Utilities.ConvertAmount(sales.TotalQRCodeProfit, sales.Currency, weekSales.Currency);
                weekSales.TotalItemsSold += sales.TotalItemsSold;
                allItemsSoldReports.AddRange(sales.ItemsSold);
                // must add up item type category incomes & profits now
                foreach (ItemTypeMoneyInfo moneyInfo in sales.ItemTypeMoneyBreakdown)
                {
                    // if we don't have info on that item type already, create it
                    if (!weekSales.ItemTypeIDToMoneyInfo.ContainsKey(moneyInfo.Type.ID))
                    {
                        var createdMoneyInfo = new ItemTypeMoneyInfo(moneyInfo.Type);
                        createdMoneyInfo.Currency = weekSales.Currency;
                        weekSales.ItemTypeIDToMoneyInfo[moneyInfo.Type.ID] = createdMoneyInfo;
                        weekSales.ItemTypeMoneyBreakdown.Add(createdMoneyInfo);
                    }
                    var moneyInfoToAdjust = weekSales.ItemTypeIDToMoneyInfo[moneyInfo.Type.ID];
                    moneyInfoToAdjust.TotalItemsSold += moneyInfo.TotalItemsSold;
                    // need to add in the income and profit
                    moneyInfoToAdjust.TotalIncome += Utilities.ConvertAmount(moneyInfo.TotalIncome, sales.Currency, weekSales.Currency);
                    moneyInfoToAdjust.TotalQRCodeIncome += Utilities.ConvertAmount(moneyInfo.TotalQRCodeIncome, sales.Currency, weekSales.Currency);
                    moneyInfoToAdjust.TotalCashIncome += Utilities.ConvertAmount(moneyInfo.TotalCashIncome, sales.Currency, weekSales.Currency);
                    moneyInfoToAdjust.TotalProfit += Utilities.ConvertAmount(moneyInfo.TotalProfit, sales.Currency, weekSales.Currency);
                }
            }
            // now we need to set up the AllItemsSold array
            var itemIDToReportSold = new Dictionary<int, ReportItemSold>();
            foreach (ReportItemSold singleItemSoldReport in allItemsSoldReports)
            {
                if (!itemIDToReportSold.ContainsKey(singleItemSoldReport.InventoryItemID))
                {
                    itemIDToReportSold[singleItemSoldReport.InventoryItemID] = singleItemSoldReport;
                    weekSales.AllItemsSold.Add(singleItemSoldReport);
                }
                else
                {
                    ReportItemSold allItemsSoldData = itemIDToReportSold[singleItemSoldReport.InventoryItemID];
                    allItemsSoldData.QuantityPurchased += singleItemSoldReport.QuantityPurchased;
                    // TODO: do we need to add qr/cash totals? need to check actual report output here
                    // it's not used for actual report output I think but eventually we should probably add those calculations in
                    allItemsSoldData.TotalCost +=
                        Utilities.ConvertAmount(singleItemSoldReport.QuantityPurchased * singleItemSoldReport.CostPerItem,
                        singleItemSoldReport.CostCurrency, allItemsSoldData.CostCurrency);
                    allItemsSoldData.TotalProfit +=
                        Utilities.ConvertAmount(singleItemSoldReport.QuantityPurchased * singleItemSoldReport.ProfitPerItem,
                        singleItemSoldReport.ProfitCurrency, allItemsSoldData.ProfitCurrency);
                }
            }
            // sort final arrays for nice display
            weekSales.AllItemsSold.Sort((left, right) => left.Name.ToLower().CompareTo(right.Name.ToLower()));
            weekSales.ItemTypeMoneyBreakdown.Sort((left, right) => left.Type.Name.ToLower().CompareTo(right.Type.Name.ToLower()));
            return weekSales;
        }

        #region IItemsSoldReportData

        public DateTime GetDate()
        {
            return Date;
        }

        public List<ReportItemSold> GetItemsSold()
        {
            return AllItemsSold;
        }

        public string GetTotalIncomeWithCurrency()
        {
            return TotalIncomeWithCurrency;
        }

        public int GetTotalItemsSold()
        {
            return TotalItemsSold;
        }

        public string GetTotalProfitWithCurrency()
        {
            return TotalProfitWithCurrency;
        }

        public bool IsDailyReport()
        {
            return false;
        }

        public List<ItemTypeMoneyInfo> GetItemTypeMoneyInfo()
        {
            return ItemTypeMoneyBreakdown;
        }

        public string GetTotalCashIncomeWithCurrency() => TotalCashIncomeWithCurrency;
        public string GetTotalQRCodeIncomeWithCurrency() => TotalQRCodeIncomeWithCurrency;
        public int GetTotalNumCashSales() => TotalNumCashSales;
        public int GetTotalNumQRCodeSales() => TotalNumQRCodeSales;
        public string GetTotalCashProfitWithCurrency() => TotalCashProfitWithCurrency;
        public string GetTotalQRCodeProfitWithCurrency() => TotalQRCodeProfitWithCurrency;

        #endregion
    }
}
