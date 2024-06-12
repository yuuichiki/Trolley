using RCabinet.Models;

namespace RCabinet.Interfaces
{
    interface ICreatedEditedCurrency
    {
        void CreatedEditedCurrency(Currency currency, bool wasCreated);
    }
}
