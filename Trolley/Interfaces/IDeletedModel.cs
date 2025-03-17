using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.Interfaces
{
    interface IDeletedModel<T>
    {
        void ModelWasDeleted(T model);
    }
}
