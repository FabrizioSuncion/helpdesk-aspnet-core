using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    public Prioridad Prioridad { get; set; } = Prioridad.Media;
    public EstadoTicket Estado { get; set; } = EstadoTicket.Abierto;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string CreadorId { get; set; } = string.Empty;
    public ApplicationUser? Creador { get; set; }

    public string? TecnicoId { get; set; }
    public ApplicationUser? Tecnico { get; set; }

    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
