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

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await ObtenerTicketAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!PuedeVer(ticket))
        {
            return Forbid();
        }

        var tecnicos = await _userManager.GetUsersInRoleAsync("Tecnico");
        var modelo = new TicketDetailsViewModel
        {
            Ticket = ticket,
            Tecnicos = tecnicos
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new SelectListItem(u.NombreCompleto, u.Id, u.Id == ticket.TecnicoId))
                .ToList()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AsignarTecnico(int id, string? tecnicoId)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(tecnicoId))
        {
            ticket.TecnicoId = null;
        }
        else
        {
            var tecnico = await _userManager.FindByIdAsync(tecnicoId);
            if (tecnico == null || !await _userManager.IsInRoleAsync(tecnico, "Tecnico"))
            {
                return BadRequest();
            }

            ticket.TecnicoId = tecnicoId;
            if (ticket.Estado == EstadoTicket.Abierto)
            {
                ticket.Estado = EstadoTicket.EnProceso;
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Tecnico")]
    public async Task<IActionResult> TomarTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (ticket.TecnicoId == null)
        {
            ticket.TecnicoId = _userManager.GetUserId(User);
            if (ticket.Estado == EstadoTicket.Abierto)
            {
                ticket.Estado = EstadoTicket.EnProceso;
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Tecnico")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoTicket estado)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (!User.IsInRole("Administrador") && ticket.TecnicoId != userId)
        {
            return Forbid();
        }

        if (!Enum.IsDefined(estado))
        {
            return BadRequest();
        }

        ticket.Estado = estado;
        ticket.FechaCierre = (estado == EstadoTicket.Resuelto || estado == EstadoTicket.Cerrado)
            ? DateTime.UtcNow
            : null;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarComentario(int id, string? texto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!PuedeVer(ticket))
        {
            return Forbid();
        }

        texto = (texto ?? string.Empty).Trim();
        if (texto.Length == 0 || texto.Length > 1000)
        {
            TempData["Error"] = "El comentario debe tener entre 1 y 1000 caracteres.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.Comentarios.Add(new Comentario
        {
            TicketId = id,
            AutorId = _userManager.GetUserId(User)!,
            Texto = texto,
            Fecha = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private bool PuedeVer(Ticket ticket)
    {
        var userId = _userManager.GetUserId(User);

        if (User.IsInRole("Administrador"))
        {
            return true;
        }

        if (User.IsInRole("Tecnico"))
        {
            return ticket.TecnicoId == userId || ticket.TecnicoId == null;
        }

        return ticket.CreadorId == userId;
    }

    private Task<Ticket?> ObtenerTicketAsync(int id)
    {
        return _context.Tickets
            .Include(t => t.Categoria)
            .Include(t => t.Creador)
            .Include(t => t.Tecnico)
            .Include(t => t.Comentarios).ThenInclude(c => c.Autor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    private async Task CargarCategoriasAsync()
    {
        var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
    }
}
