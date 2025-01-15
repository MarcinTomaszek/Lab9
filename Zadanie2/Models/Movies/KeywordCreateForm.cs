using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Movies;

public class KeywordCreateForm
{
    [Required]
    [MaxLength(length:50, ErrorMessage = "Keyword must have at most 50 characters")]
    [MinLength(length:2, ErrorMessage = "Keyword must have at least 2 characters")]
    public string Keyword { get; set; }
    
    public int Movie_id { get; set; }
}