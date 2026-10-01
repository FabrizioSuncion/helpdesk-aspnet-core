using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
