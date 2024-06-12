using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCabinet.Interfaces
{
    interface IDeletedItemSoldInfo
    {
        /// <summary>
        /// returns an updated report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ReportItemSold ItemSoldInfoWasDeleted(IItemSoldInfo model);
    }
}
