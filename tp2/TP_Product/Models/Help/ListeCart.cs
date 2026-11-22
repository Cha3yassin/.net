using System.Text.Json;
using TP_Product.Models.Repositories;

namespace TP_Product.Models.Help
{
    // Une ligne du panier telle qu'elle est enregistrée dans la session
    public class LignePanier
    {
        public int ProductId { get; set; }
        public int Quantite { get; set; }
    }

    public class ListeCart
    {
        // Clé utilisée pour enregistrer le panier dans la session
        private const string SessionKey = "Panier";

        public List<Item> Items { get; private set; }

        public ListeCart()
        {
            Items = new List<Item>();
        }

        // Charger le panier de l'utilisateur depuis SA session
        // (chaque visiteur a son propre panier)
        public static ListeCart FromSession(ISession session, IProductRepository productRepository)
        {
            ListeCart cart = new ListeCart();
            string json = session.GetString(SessionKey);
            if (json != null)
            {
                var lignes = JsonSerializer.Deserialize<List<LignePanier>>(json);
                foreach (var ligne in lignes)
                {
                    // On recharge le produit depuis la base (prix et stock à jour)
                    Product p = productRepository.GetById(ligne.ProductId);
                    if (p != null)
                    {
                        Item item = new Item(p);
                        item.quantite = ligne.Quantite;
                        cart.Items.Add(item);
                    }
                }
            }
            return cart;
        }

        // Enregistrer le panier dans la session
        public void SaveToSession(ISession session)
        {
            var lignes = Items.Select(i => new LignePanier
            {
                ProductId = i.Prod.ProductId,
                Quantite = i.quantite
            }).ToList();
            session.SetString(SessionKey, JsonSerializer.Serialize(lignes));
        }

        // Nombre d'articles dans le panier (pour le badge de la navbar)
        public static int CountInSession(ISession session)
        {
            string json = session.GetString(SessionKey);
            if (json == null)
                return 0;
            var lignes = JsonSerializer.Deserialize<List<LignePanier>>(json);
            return lignes.Sum(l => l.Quantite);
        }

        public void AddItem(Product prod)
        {
            Boolean iswhat = false;
            // Create a new item to add to the cart
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    a.quantite++;
                    iswhat = true;
                    return;
                }
            }
            if (iswhat == false)
            {
                Item newItem = new Item(prod);
                newItem.quantite = 1;
                Items.Add(newItem);
            }
        }
        public void SetLessOneItem(Product prod)
        {
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    if (a.quantite <= 1)
                    {
                        RemoveItem(a.Prod);
                        return;
                    }
                    else
                    {
                        a.quantite--;
                        return;
                    }
                }
            }
        }
        public void SetItemQuantity(Product prod, int quantity)
        {
            if (quantity == 0)
            {
                RemoveItem(prod);
                return;
            }
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    a.quantite = quantity;
                    return;
                }
            }
        }
        public void RemoveItem(Product prod)
        {
            Item t = null;
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    t = a;
                }
            }
            if (t != null)
            {
                Items.Remove(t);
            }
        }
        public void Clear()
        {
            Items.Clear();
        }
        public float GetSubTotal()
        {
            float subTotal = 0;
            foreach (Item i in Items)
                subTotal += i.TotalPrice;
            return (float)subTotal;
        }
    }
}
