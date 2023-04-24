using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDS.Models
{
    public class ApplicationUser : IdentityUser
    {

        public virtual ICollection<Hotel>? ListaHoteluri { get; set; }

        public virtual ICollection<Rezervare>? ListaRezervari { get; set; }

        public virtual ICollection<Camera>? ListaCamere { get; set; }

        public virtual ICollection<Review>? ListaReviews { get; set; }

        public virtual ICollection<Tara>? ListaTari { get; set; }


        public string? Nume{ get; set; }

        public string? Prenume { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
