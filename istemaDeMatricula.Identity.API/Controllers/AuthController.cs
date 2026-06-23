using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Identity.API.DTOS;
using SistemaDeMatricula.Identity.API.Interface;
using SistemaDeMatricula.Identity.API.utilitario;

namespace SistemaDeMatricula.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, ITokenService tokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario == null)
        {
            return Unauthorized(new { mensagem = "Usuário ou senha incorretos!" });
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, lockoutOnFailure: false);
        if (!resultado.Succeeded)
        {
            return Unauthorized(new { census = "Usuário ou senha incorretos!" });
        }

        var roles = await _userManager.GetRolesAsync(usuario);

        var tokenGerado = await _tokenService.GenerateToken(usuario, roles);

        return Ok(new LoginResponse(usuario.Email!, tokenGerado, roles));
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
    {
        var novoUsuario = new IdentityUser { UserName = request.Email, Email = request.Email };

        var resultado = await _userManager.CreateAsync(novoUsuario, request.Password);
        if (!resultado.Succeeded) return BadRequest(resultado.Errors);

        var roleDefinida = string.IsNullOrWhiteSpace(request.Role) ? RolesUsuarios.Aluno : request.Role;

        var roleExiste = await _roleManager.RoleExistsAsync(roleDefinida);
        if (!roleExiste)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleDefinida));
        }

        var vinculoResultado = await _userManager.AddToRoleAsync(novoUsuario, roleDefinida);
        if (!vinculoResultado.Succeeded)
        {
            return BadRequest(new { mensagem = "Usuário criado, mas falhou ao vincular o cargo.", erros = vinculoResultado.Errors });
        }

        return Ok(new { mensagem = $"Usuário criado com sucesso e associado ao cargo: {roleDefinida}!" });
    }
}