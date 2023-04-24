using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDS.Models
{
    public class Rezervare
    {
        [Key]
        public int Id { get; set; }

        public string ListaClienti { get; set; } 
        [Required(ErrorMessage = "Data de check in este obligatorie")]
        public DateTime CheckIn { get; set; }

        [Required(ErrorMessage = "Data de check out este obligatorie")]

        public DateTime CheckOut { get; set; }

        public float Suma { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public String ? UserId { get; set; }
        public int? CameraId { get; set;}

        

        [NotMapped]
        public IEnumerable<SelectListItem>? Camera { get; set; }

    }
}
