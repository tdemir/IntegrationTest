using Integration.Common;
using Integration.Backend;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; }

    private int activeSaveItemCount = 0;


    public ItemIntegrationService(ItemOperationBackend itemOperationBackend)
    {
        this.ItemIntegrationBackend = itemOperationBackend;
    }


    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        Interlocked.Increment(ref activeSaveItemCount);

        Item item;
        lock (ItemIntegrationBackend)
        {
            // Check the backend to see if the content is already saved.
            if (IsItemContentExist(itemContent))
            {
                Interlocked.Decrement(ref activeSaveItemCount);
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }
            item = ItemIntegrationBackend.SaveItem(itemContent);
        }

        Interlocked.Decrement(ref activeSaveItemCount);
        return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
    }

    private bool IsItemContentExist(string itemContent)
    {
        return ItemIntegrationBackend.FindItemsWithContent(itemContent).Any();
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }

    public bool HasActiveSave()
    {
        return activeSaveItemCount != 0;
    }
}