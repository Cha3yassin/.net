using System.ComponentModel.DataAnnotations;

namespace TP_Product.ViewModels
{
    // Formulaire "Laisser un avis"
    public class AvisViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Choisissez une note.")]
        [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5.")]
        public int Note { get; set; }

        [StringLength(500, ErrorMessage = "500 caractères maximum.")]
        public string Commentaire { get; set; }
    }
}
