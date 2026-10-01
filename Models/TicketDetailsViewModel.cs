using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpDesk.Models;

public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; } = null!;
    public List<SelectListItem> Tecnicos { get; set; } = new();
}
