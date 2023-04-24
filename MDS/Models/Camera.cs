using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MDS.Models
{
    public class Camera
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele camerei este obligatoriu")]
        public string Nume{ get; set; }
        
        [Required(ErrorMessage = "Capacitatea camerei este obligatorie")]
        public int Capacitate { get; set; }

        [Required(ErrorMessage = "Descrierea camerei este obligatorie")]
        public string Descriere { get; set; }

        [Required(ErrorMessage = "Pretul camerei este obligatoriu")]
        public float PretNoapte{ get; set; }

        public int HotelId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Hotel? Hotel { get; set; }

        public virtual ICollection<Rezervare> ListaRezervari { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Hoteluri { get; set; }

        [NotMapped]
        public bool Disponibila { get; set; }

        
    }
}
