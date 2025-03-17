using Trolley.Models;

namespace Trolley.Interfaces
{
    interface ICreatedEditedCurrency
    {
        void CreatedEditedCurrency(Currency currency, bool wasCreated);
    }
}
