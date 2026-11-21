using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TP6_WebApi.Models
{
    public class Operation
    {
        public int Id { get; set; }
        [Required, RegularExpression("Retrait|Versement|Virement")]
        public string TypeOperation { get; set; }
        [Range(0.01, double.MaxValue)] public double Montant { get; set; }
        public DateTime DateOperation { get; set; }
        [Required] public int CompteID { get; set; }           // clé étrangère
        [JsonIgnore, ValidateNever]
        public Compte? Compte { get; set; }
    }
}
