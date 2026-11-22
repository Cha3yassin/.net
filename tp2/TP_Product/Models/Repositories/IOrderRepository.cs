namespace TP_Product.Models.Repositories
{
    public interface IOrderRepository
    {
        Order GetById(int Id);
        void Add(Order o);
        // Commandes d'un seul utilisateur (Mes commandes)
        IList<Order> GetByUserId(string userId);
        // Toutes les commandes (Administration)
        IList<Order> GetAll();
    }
}
