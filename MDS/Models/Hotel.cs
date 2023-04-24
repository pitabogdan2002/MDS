using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDS.Models
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Numele trebuie sa aiba mai mult de 5 caractere")]
        public string Nume { get; set; }
        public int? Rating { get; set; }


        [Required(ErrorMessage = "Locatia hotelului este obligatorie")]
        public string Locatie { get; set; }

        public string Facilitati { get; set; }


        [Required(ErrorMessage = "Tara este obligatorie")]
        public int? TaraId { get; set; }

        public string? UserId { get; set; }


        public virtual ApplicationUser? User { get; set; }

        public virtual Tara? Tara { get; set; }

        public virtual ICollection<Review>? ListaReviews { get; set; }
        public virtual ICollection<Camera>? ListaCamere { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Tari { get; set; }

    }
}