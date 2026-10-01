using HelpDesk.Models;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Data;

public static class DbSeeder
{
    public static readonly string[] Roles = { "Administrador", "Tecnico", "Usuario" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var rol in Roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        await CrearUsuarioAsync(userManager, "admin@helpdesk.com", "Administrador Demo", "Admin123", "Administrador");
        await CrearUsuarioAsync(userManager, "tecnico@helpdesk.com", "Tecnico Demo", "Tecnico123", "Tecnico");
        await CrearUsuarioAsync(userManager, "usuario@helpdesk.com", "Usuario Demo", "Usuario123", "Usuario");
    }

    private static async Task CrearUsuarioAsync(
        UserManager<ApplicationUser> userManager, string email, string nombre, string password, string rol)
    {
        if (await userManager.FindByEmailAsync(email) != null)
        {
            return;
        }

        var usuario = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NombreCompleto = nombre,
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(usuario, password);
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(usuario, rol);
        }
    }
}
