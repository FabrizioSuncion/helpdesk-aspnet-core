using HelpDesk.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var query = _context.Tickets
            .Include(t => t.Categoria)
            .Include(t => t.Creador)
            .Include(t => t.Tecnico)
            .AsQueryable();

        if (User.IsInRole("Administrador"))
        {
            // El administrador ve todos los tickets.
        }
        else if (User.IsInRole("Tecnico"))
        {
            query = query.Where(t => t.TecnicoId == userId || t.TecnicoId == null);
        }
        else
        {
            query = query.Where(t => t.CreadorId == userId);
        }

        var tickets = await query.OrderByDescending(t => t.FechaCreacion).ToListAsync();
        return View(tickets);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new TicketCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync();
            return View(modelo);
        }

        var ticket = new Ticket
        {
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion,
            Prioridad = modelo.Prioridad,
            CategoriaId = modelo.CategoriaId,
            CreadorId = _userManager.GetUserId(User)!,
            Estado = EstadoTicket.Abierto,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync()
    {
        var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
    }
}
