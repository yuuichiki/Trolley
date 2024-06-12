using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCabinet.Interfaces
{
    public interface IItemSoldInfo
    {
        string FriendlyDateTime { get; }
        int QuantitySold { get; }
        string TotalCostWithCurrency { get; }
        string TotalProfitWithCurrency { get; }
        string SoldByUserName { get; }
        string PurchaseMethodString { get; }
    }
}
