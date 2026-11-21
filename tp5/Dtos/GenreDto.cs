using System.ComponentModel.DataAnnotations;

namespace MoviesApi.Dtos
{
    public class GenreDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
