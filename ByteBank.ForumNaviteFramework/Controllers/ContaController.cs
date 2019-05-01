using ByteBank.ForumNaviteFramework.Models;
using ByteBank.ForumNaviteFramework.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.ForumNaviteFramework.Controllers
{
    public class ContaController : Controller
    {

        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                    _userManager = HttpContext.GetOwinContext().GetUserManager<UserManager<UsuarioAplicacao>>();
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }


        public ActionResult Registrar()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel model)
        {
            if (ModelState.IsValid)
            {

                var novoUsuario = new UsuarioAplicacao
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    NomeCompleto = model.NomeCompleto
                };

                var usuario = await UserManager.FindByEmailAsync(model.Email);
                var usuarioJaExiste = usuario != null;

                if (usuarioJaExiste)
                    return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, model.Senha);

                if (resultado.Succeeded)
                {
                    await EnviaEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    AdicicionaErros(resultado);
                }
            }
            return View(model);
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Erro");

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
            
        }


        private async Task EnviaEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);
            var linkDeCallback = Url.Action("ConfirmacaoEmail", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);
            await UserManager.SendEmailAsync(usuario.Id,
                "Fórum Bytebank -Confirmação de E-mail",
                $"Bem vindo ao fórum do Bytebank, clique aqui {linkDeCallback} para confirmar seu e-mail!");
        }

        /// <summary>
        /// Método que verifica se o usuário já está criado
        /// </summary>
        /// <param name="resultado"></param>
        private void AdicicionaErros(IdentityResult resultado)
        {
            foreach (var item in resultado.Errors)
                ModelState.AddModelError("", item);
        }
    }
}