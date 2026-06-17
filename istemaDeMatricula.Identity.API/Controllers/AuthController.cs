using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SistemaDeMatricula.Identity.API.Interface;

namespace SistemaDeMatricula.Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        // 🎯 Injeção de dependência tripla: gerenciadores do Identity + nosso serviço de token
        public AuthController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ITokenService tokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Procuramos se o usuário existe pelo e-mail
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null)
            {
                return Unauthorized(new { mensagem = "Usuário ou senha incorretos!" });
            }

            // 2. O Identity checa se a senha bate de forma segura (sem dar lockout por enquanto)
            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, request.Password, lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                return Unauthorized(new { mensagem = "Usuário ou senha incorretos!" });
            }

            // 3. Se a senha está certa, buscamos os cargos (Roles) dele no banco
            var roles = await _userManager.GetRolesAsync(usuario);

            // 4. CHAMA A FÁBRICA! Geramos o token usando a nossa lógica sênior
            var tokenGerado = _tokenService.GenerateToken(usuario, roles);

            // 5. Retorna o crachá brilhando para o cliente!
            return Ok(new { token = tokenGerado });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegisterRequest request)
        {
            var novoUsuario = new IdentityUser { UserName = request.Email, Email = request.Email };

            // 🎯 A senha vem dinâmica do Postman/Swagger e o Identity calcula o Hash na hora!
            var resultado = await _userManager.CreateAsync(novoUsuario, request.Password);

            if (!resultado.Succeeded) return BadRequest(resultado.Errors);

            return Ok(new { mensagem = "Usuário criado com sucesso!" });
        }
    }
}