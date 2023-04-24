using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace MDS.Models
{
    public class Tara
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele tarii este obligatoriu")]
        public string Nume { get; set; }

        public virtual ICollection<Hotel>? ListaHoteluri { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
