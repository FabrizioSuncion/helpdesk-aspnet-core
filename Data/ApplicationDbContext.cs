using HelpDesk.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ticket>(e =>
        {
            e.HasOne(t => t.Creador).WithMany().HasForeignKey(t => t.CreadorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Tecnico).WithMany().HasForeignKey(t => t.TecnicoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Categoria).WithMany(c => c.Tickets).HasForeignKey(t => t.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Comentario>(e =>
        {
            e.HasOne(c => c.Ticket).WithMany(t => t.Comentarios).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Autor).WithMany().HasForeignKey(c => c.AutorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Hardware" },
            new Categoria { Id = 2, Nombre = "Software" },
            new Categoria { Id = 3, Nombre = "Red" },
            new Categoria { Id = 4, Nombre = "Accesos" }
        );
    }
}
