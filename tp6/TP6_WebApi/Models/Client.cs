using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TP6_WebApi.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required, StringLength(50)] public string Nom { get; set; }
        [Required, StringLength(50)] public string Prenom { get; set; }
        [Phone] public string Tel { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        public string Adresse { get; set; }
        public string? Image { get; set; }
        [JsonIgnore]
        public ICollection<Compte>? Compte { get; set; }
    }
}
