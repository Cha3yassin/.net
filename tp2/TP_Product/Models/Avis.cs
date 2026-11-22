using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TP_Product.Models
{
    // Avis d'un client sur un produit (note de 1 à 5 + commentaire)
    public class Avis
    {
        public int AvisId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Lien avec l'utilisateur dans Identity
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Required]
        [Range(1, 5)]
        public int Note { get; set; }

        [StringLength(500)]
        public string Commentaire { get; set; }

        public DateTime DateAvis { get; set; }
    }
}
