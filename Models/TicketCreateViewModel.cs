using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models;

public class TicketCreateViewModel
{
    [Required(ErrorMessage = "El titulo es obligatorio")]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatoria")]
    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    public Prioridad Prioridad { get; set; } = Prioridad.Media;

    [Required(ErrorMessage = "Seleccione una categoria")]
    public int CategoriaId { get; set; }
}
