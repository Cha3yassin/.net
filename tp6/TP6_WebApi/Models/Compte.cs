using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TP6_WebApi.Models
{
    public class Compte
    {
        public int Id { get; set; }
        [Required] public int NuméroCompte { get; set; }
        [DataType(DataType.Date)] public DateTime DateCréation { get; set; }
        [Range(0, double.MaxValue)] public double Solde { get; set; }
        [Required] public int ClientID { get; set; }          // clé étrangère
        [Required] public string TypeCompte { get; set; }
        [ValidateNever]
        public Client? Client { get; set; }
        [JsonIgnore, ValidateNever]
        public ICollection<Operation>? Operation { get; set; }
    }
}
