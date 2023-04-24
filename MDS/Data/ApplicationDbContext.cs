using MDS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MDS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tara> ListaTari { get; set; }
        public DbSet<Hotel> ListaHoteluri { get; set; }
        public DbSet<Camera> ListaCamere { get; set; }

        public DbSet<Rezervare> ListaRezervari { get; set; }

        public DbSet<Review> ListaReviews { get; set; }

    }
}