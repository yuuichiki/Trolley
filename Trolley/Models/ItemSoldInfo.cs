﻿using Trolley.Enums;
using Trolley.Helpers;
using Trolley.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.Models
{
    public class ItemSoldInfo : ChangeNotifier, IItemSoldInfo
    {
        private int _quantity;

        public ItemSoldInfo()
        {
            _quantity = 1;
            PurchaseMethod = PurchaseMethod.Cash;
        }

        public int ID { get; set; }
        public DateTime DateTimeSold { get; set; }

        public int QuantitySold // defaults to 1
        {
            get => _quantity;
            set
            {
                _quantity = value;
                if (value > MaxQuantity)
                {
                    value = MaxQuantity >= 0 ? MaxQuantity : 0;
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(TotalCost));
                NotifyPropertyChanged(nameof(TotalCostWithCurrency));
                NotifyPropertyChanged(nameof(TotalProfit));
                NotifyPropertyChanged(nameof(TotalProfitWithCurrency));
                PurchaseInfoChanged?.QuantityChanged(this, value);
            }
        }

        public IPurchaseInfoChanged PurchaseInfoChanged { get; set; }

        public int InventoryItemID { get; set; }
        public int SoldByUserID { get; set; }
        public string SoldByUserName { get; set; }

        // we remember the cost of things in case it changes over time so we have the original info :)
        /// <summary>
        /// just the cost for 1 item -- to get total, multiply by QuantitySold
        /// </summary>
        public decimal Cost { get; set; }
        public Currency CostCurrency { get; set; }
        public decimal Paid { get; set; }
        public Currency PaidCurrency { get; set; }
        public decimal Change { get; set; }
        public Currency ChangeCurrency { get; set; }
        /// <summary>
        /// just the profit for 1 item -- to get total, multiply by QuantitySold
        /// </summary>
        public decimal ProfitPerItem { get; set; }
        public Currency ProfitPerItemCurrency { get; set; }
        public ItemType ItemType { get; set; }

        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int MaxQuantity { get; set; }
        public PurchaseMethod PurchaseMethod { get; set; }
        public string PurchaseMethodString
        {
            get => PurchaseMethod.ToHumanReadableString();
        }

        public string FriendlyTime
        {
            get { return DateTimeSold.ToLongTimeString(); }
        }

        public string FriendlyDateTime
        {
            get { return DateTimeSold.ToString(Utilities.DateTimeToFriendlyFullDateTimeStringFormat()); }
        }

        public string CostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", Cost, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", Cost);
            }
        }

        public decimal TotalCost
        {
            get => QuantitySold * Cost;
        }

        public string TotalCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", Cost * QuantitySold, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", Cost * QuantitySold);
            }
        }

        public decimal TotalProfit
        {
            get => QuantitySold * ProfitPerItem;
        }

        public string ProfitWithCurrency
        {
            get
            {
                if (ProfitPerItemCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", ProfitPerItem, ProfitPerItemCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", ProfitPerItem);
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (ProfitPerItemCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", ProfitPerItem * QuantitySold, ProfitPerItemCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", ProfitPerItem * QuantitySold);
            }
        }

        private static List<ItemSoldInfo> LoadInfo(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<ItemSoldInfo>();
            var currencies = Currency.GetKeyValueCurrencyList();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "" +
                        "SELECT isi.ID, DateTimeSold, QuantitySold, isi.Cost, isi.CostCurrencyID, isi.ProfitPerItem, isi.ProfitPerItemCurrencyID, " +
                        "       isi.InventoryItemID, isi.SoldByUserID, i.Name, i.Description, isi.Paid, isi.PaidCurrencyID, isi.Change, isi.ChangeCurrencyID, " +
                        "       isi.PurchaseMethod, " + 
                        "       it.ID AS ItemTypeID, it.Name AS ItemTypeName, it.Description AS ItemTypeDescription," +
                        "       it.IsDefault AS ItemTypeIsDefault, u.Name AS UserName " +
                        "FROM ItemsSoldInfo isi JOIN InventoryItems i ON isi.InventoryItemID = i.ID " +
                        "   LEFT JOIN ItemTypes it ON i.ItemTypeID = it.ID " +
                        "   JOIN Users u ON isi.SoldByUserID = u.ID " +
                        (string.IsNullOrWhiteSpace(whereClause) ? "" : whereClause + " ") +
                        "ORDER BY lower(i.Name), isi.DateTimeSold";

                    command.CommandText = query;
                    if (!string.IsNullOrEmpty(whereClause) && whereParams != null)
                    {
                        foreach (Tuple<string, string> keyValuePair in whereParams)
                        {
                            command.Parameters.AddWithValue(keyValuePair.Item1, keyValuePair.Item2);
                        }
                    }
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ItemSoldInfo();
                            item.ID = dbHelper.ReadInt(reader, "ID");
                            item.ItemName = dbHelper.ReadString(reader, "Name");
                            item.ItemDescription = dbHelper.ReadString(reader, "Description");
                            item.ItemType = new ItemType(
                                dbHelper.ReadInt(reader, "ItemTypeID"),
                                dbHelper.ReadString(reader, "ItemTypeName"),
                                dbHelper.ReadString(reader, "ItemTypeDescription"),
                                dbHelper.ReadBool(reader, "ItemTypeIsDefault"));
                            item.InventoryItemID = dbHelper.ReadInt(reader, "InventoryItemID");
                            item.SoldByUserID = dbHelper.ReadInt(reader, "SoldByUserID");
                            item.SoldByUserName = dbHelper.ReadString(reader, "UserName");
                            string dateTimeSold = dbHelper.ReadString(reader, "DateTimeSold");
                            item.DateTimeSold = Convert.ToDateTime(dateTimeSold); // DateTime.ParseExact(dateTimeSold, 
                                //Utilities.DateTimeToDateOnlyStringFormat(), System.Globalization.CultureInfo.InvariantCulture);
                            item.QuantitySold = dbHelper.ReadInt(reader, "QuantitySold");
                            item.Cost = dbHelper.ReadDecimal(reader, "Cost");
                            var costCurrencyID = dbHelper.ReadInt(reader, "CostCurrencyID");
                            item.CostCurrency = currencies.ContainsKey(costCurrencyID) ? currencies[costCurrencyID] : null;
                            item.ProfitPerItem = dbHelper.ReadDecimal(reader, "ProfitPerItem");
                            var profitCurrencyID = dbHelper.ReadInt(reader, "ProfitPerItemCurrencyID");
                            item.ProfitPerItemCurrency = currencies.ContainsKey(profitCurrencyID) ? currencies[profitCurrencyID] : null;

                            item.Paid = dbHelper.ReadDecimal(reader, "Paid");
                            var paidCurrencyID = dbHelper.ReadInt(reader, "PaidCurrencyID");
                            item.PaidCurrency = currencies.ContainsKey(paidCurrencyID) ? currencies[paidCurrencyID] : null;

                            item.Change = dbHelper.ReadDecimal(reader, "Change");
                            var changeCurrencyID = dbHelper.ReadInt(reader, "ChangeCurrencyID");
                            item.ChangeCurrency = currencies.ContainsKey(changeCurrencyID) ? currencies[changeCurrencyID] : null;

                            var didParsePurchaseMethod = Enum.TryParse(dbHelper.ReadInt(reader, "PurchaseMethod").ToString(),
                                true, out PurchaseMethod resultingPurchaseMethod);
                            item.PurchaseMethod = didParsePurchaseMethod ? resultingPurchaseMethod : PurchaseMethod.Cash;

                            items.Add(item);
                        }
                        reader.Close();
                    }

                    conn.Close();
                }
            }
            return items;
        }

        public static List<ItemSoldInfo> LoadInfoForDate(DateTime date, int userID = -1)
        {
            string whereClause = "WHERE DateTimeSold LIKE '" + date.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + "%' ";
            if (userID != -1)
            {
                whereClause += " AND isi.SoldByUserID = " + userID + " ";
            }
            return LoadInfo(whereClause);
        }

        public static List<ItemSoldInfo> LoadInfoForDateAndItem(DateTime date, int inventoryItemID, int userID = -1)
        {
            string whereClause = "WHERE DateTimeSold LIKE '" + date.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + "%' AND InventoryItemID = @itemID";
            if (userID != -1)
            {
                whereClause += " AND isi.SoldByUserID = " + userID + " ";
            }
            return LoadInfo(whereClause, new List<Tuple<string, string>>() { new Tuple<string, string>("@itemID", inventoryItemID.ToString()) });
        }

        public static List<ItemSoldInfo> LoadInfoForDateAndItemUntilDate(DateTime startDate, DateTime endDate, int inventoryItemID, int userID = -1)
        {
            if (endDate != null && startDate.Date != endDate.Date && endDate > startDate)
            {
                string whereClause = "WHERE DateTimeSold BETWEEN '" + startDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' AND '" +
                        endDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' AND InventoryItemID = @itemID";
                if (userID != -1)
                {
                    whereClause += " AND isi.SoldByUserID = " + userID + " ";
                }
                return LoadInfo(whereClause, new List<Tuple<string, string>>() { new Tuple<string, string>("@itemID", inventoryItemID.ToString()) });
            }
            return LoadInfoForDateAndItem(startDate, inventoryItemID, userID);
        }

        public static List<int> LoadItemIDsSoldBetweenDateAndItemUntilDate(DateTime startDate, DateTime endDate, bool ignoreTime = false)
        {
            List<ItemSoldInfo> infoList = null;
            if (endDate != null && startDate.Date != endDate.Date && endDate > startDate)
            {
                if (!ignoreTime)
                {
                    string whereClause = "WHERE DateTimeSold BETWEEN '" + startDate.ToString(Utilities.DateTimeToStringFormat()) + "' AND '" +
                            endDate.ToString(Utilities.DateTimeToStringFormat()) + "'";
                    infoList = LoadInfo(whereClause, new List<Tuple<string, string>>() { });
                }
                else
                {
                    string whereClause = "WHERE DateTimeSold BETWEEN '" + startDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' AND '" +
                            endDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00'";
                    infoList = LoadInfo(whereClause, new List<Tuple<string, string>>() { });
                }
            }
            else
            {
                infoList = LoadInfoForDate(startDate);
            }
            var output = new List<int>();
            foreach (ItemSoldInfo info in infoList)
            {
                output.Add(info.InventoryItemID);
            }
            return output;
        }

        public void CreateNewSoldInfo()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO ItemsSoldInfo (DateTimeSold, QuantitySold, Cost, CostCurrencyID, " +
                        "Paid, PaidCurrencyID, Change, ChangeCurrencyID, ProfitPerItem, ProfitPerItemCurrencyID, InventoryItemID, SoldByUserID," +
                        "PurchaseMethod) " +
                        " VALUES (@dateTime, @quantity, @cost, @costCurrency, @paid, @paidCurrency, @change, @changeCurrency, @profit, " +
                        "@profitCurrency, @inventoryID, @userID, @purchaseMethod) ";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@dateTime", DateTimeSold.ToString(Utilities.DateTimeToStringFormat()));
                    command.Parameters.AddWithValue("@quantity", QuantitySold);
                    command.Parameters.AddWithValue("@cost", Cost);
                    command.Parameters.AddWithValue("@costCurrency", CostCurrency?.ID);
                    command.Parameters.AddWithValue("@paid", Paid);
                    command.Parameters.AddWithValue("@paidCurrency", PaidCurrency?.ID);
                    command.Parameters.AddWithValue("@change", Change);
                    command.Parameters.AddWithValue("@changeCurrency", ChangeCurrency?.ID);
                    command.Parameters.AddWithValue("@profit", ProfitPerItem);
                    command.Parameters.AddWithValue("@profitCurrency", ProfitPerItemCurrency?.ID);
                    command.Parameters.AddWithValue("@inventoryID", InventoryItemID);
                    command.Parameters.AddWithValue("@userID", SoldByUserID);
                    command.Parameters.AddWithValue("@purchaseMethod", PurchaseMethod);
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                    conn.Close();
                }
            }
        }

        public void SaveUpdates()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "UPDATE ItemsSoldInfo SET DateTimeSold = @dateTime, QuantitySold = @quantity, Cost = @cost, " +
                        "CostCurrencyID = @costCurrency, Paid = @paid, PaidCurrencyID = @paidCurrency, Change = @change, " +
                        "ChangeCurrencyID = @changeCurrency, ProfitPerItem = @profit, ProfitPerItemCurrencyID = @profitCurrency, " +
                        "InventoryItemID = @inventoryID, SoldByUserID = @userID, PurchaseMethod = @purchaseMethod " +
                        " WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@dateTime", DateTimeSold.ToString(Utilities.DateTimeToStringFormat()));
                    command.Parameters.AddWithValue("@quantity", QuantitySold);
                    command.Parameters.AddWithValue("@cost", Cost);
                    command.Parameters.AddWithValue("@costCurrency", CostCurrency?.ID);
                    command.Parameters.AddWithValue("@paid", Paid);
                    command.Parameters.AddWithValue("@paidCurrency", PaidCurrency?.ID);
                    command.Parameters.AddWithValue("@change", Change);
                    command.Parameters.AddWithValue("@changeCurrency", ChangeCurrency?.ID);
                    command.Parameters.AddWithValue("@profit", ProfitPerItem);
                    command.Parameters.AddWithValue("@profitCurrency", ProfitPerItemCurrency?.ID);
                    command.Parameters.AddWithValue("@inventoryID", InventoryItemID);
                    command.Parameters.AddWithValue("@userID", SoldByUserID);
                    command.Parameters.AddWithValue("@purchaseMethod", PurchaseMethod);
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void Delete()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "DELETE FROM ItemsSoldInfo WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
