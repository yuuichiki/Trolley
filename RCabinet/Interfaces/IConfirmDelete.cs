using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCabinet.Interfaces
{
    interface IConfirmDelete<T>
    {
        void ConfirmDelete(T item);
    }
}
