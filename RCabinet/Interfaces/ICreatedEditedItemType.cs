using RCabinet.Models;

namespace RCabinet.Interfaces
{
    interface ICreatedEditedItemType
    {
        void CreatedEditedItemType(ItemType itemType, bool wasCreated);
    }
}
