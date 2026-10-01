using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Comentario
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string AutorId { get; set; } = string.Empty;
    public ApplicationUser? Autor { get; set; }

    [Required, StringLength(1000)]
    public string Texto { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
