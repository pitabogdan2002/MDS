using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace MDS.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul review-ului  este obligatoriu")]
        public string Titlu { get; set; }

        [Required(ErrorMessage = "Continutul review-ului  este obligatoriu")]
        public string Continut { get; set; }
        public virtual int? Rating { get; set; }

        public int? HotelId { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
