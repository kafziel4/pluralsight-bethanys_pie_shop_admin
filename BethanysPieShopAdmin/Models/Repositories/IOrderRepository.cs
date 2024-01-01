namespace BethanysPieShopAdmin.Models.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
        Task<Order?> GetOrderDetailsAsync(int? orderId);
    }
}
