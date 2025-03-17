using Trolley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.Interfaces
{
    public interface IPurchaseInfoChanged
    {
        void QuantityChanged(ItemSoldInfo info, int quantity);
    }
}
