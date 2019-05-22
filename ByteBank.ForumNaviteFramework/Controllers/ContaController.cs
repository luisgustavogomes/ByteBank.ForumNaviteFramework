using ByteBank.ForumNaviteFramework.Models;
using ByteBank.ForumNaviteFramework.Models.ViewModels;
using ByteBank.ForumNaviteFramework.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
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

        private SignInManager<UsuarioAplicacao, string> _signInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                    _signInManager = HttpContext.GetOwinContext().GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
            }
        }

        public ActionResult Registrar() => View();

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
                    AdicicionaErros(resultado);
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult RegistrarPorAutenticacaoExterna(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("RegistrarPorAutenticacaoExternaCallback")
                },
                provider);

            return new HttpUnauthorizedResult();
        }

        public async Task<ActionResult> RegistrarPorAutenticacaoExternaCallback()
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();

            var usuarioExiste = await UserManager.FindByEmailAsync(loginInfo.Email);
            if (usuarioExiste != null)
                return View("Error");

            var usuario = new UsuarioAplicacao
            {
                Email = loginInfo.Email,
                UserName = loginInfo.Email,
                NomeCompleto = loginInfo.ExternalIdentity.FindFirstValue(loginInfo.ExternalIdentity.NameClaimType)
            };

            var resultado = await UserManager.CreateAsync(usuario);

            if (resultado.Succeeded)
            {
                var resultadoAddLoginInfo = await UserManager.AddLoginAsync(usuario.Id, loginInfo.Login);
                if (resultadoAddLoginInfo.Succeeded)
                    return RedirectToAction("Index", "Home");
            }

            return View("Error");
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
                "Fórum Bytebank - Confirmação de E-mail",
                $"Bem vindo ao fórum do Bytebank, clique aqui {linkDeCallback} para confirmar seu e-mail!");
        }

        private void AdicicionaErros(IdentityResult resultado)
        {
            foreach (var item in resultado.Errors)
                ModelState.AddModelError("", item);
        }

        public async Task<ActionResult> Login() => View();

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(model.Email);


                if (usuario == null)
                    return SenhaOuUsuarioInvalidos();

                var signInResultado = await SignInManager.PasswordSignInAsync(
                                                usuario.UserName,
                                                model.Senha,
                                                isPersistent: model.ContinuarLogado,
                                                shouldLockout: true);
                switch (signInResultado)
                {
                    case SignInStatus.Success:
                        if (!usuario.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("AguardandoConfirmacao");
                        }
                        return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        var senhaCorreta = await UserManager.CheckPasswordAsync(usuario, model.Senha);
                        if (senhaCorreta)
                        {
                            var tempo = await UserManager.GetLockoutEndDateAsync(usuario.Id);
                            ModelState.AddModelError("", $"A Conta está bloqueada! {HelpDate.GetDateTimeZoneBr(tempo.UtcDateTime)}");
                        }
                        else
                            return SenhaOuUsuarioInvalidos();
                        break;
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("VerificacaoDoisFatores", "Conta");
                    default:
                        return SenhaOuUsuarioInvalidos();
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult LoginPorAutenticacaoExterna(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("LoginPorAutenticacaoExternaCallback")
                },
                provider);
            return new HttpUnauthorizedResult();
        }

        public async Task<ActionResult> LoginPorAutenticacaoExternaCallback()
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();
            var signInResultado = await SignInManager.ExternalSignInAsync(loginInfo, true);

            if (signInResultado == SignInStatus.Success)
                return RedirectToAction("Index", "Home");
            return View("Error");
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "Credenciais inválidas");
            return View("Login");
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "home");
        }

        public ActionResult EsqueciSenha() => View();

        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(model.Email);

                if (usuario != null)
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);
                    var linkDeCallback = Url.Action("ConfirmacaoAlteracaoSenha",
                        "Conta",
                        new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);
                    await UserManager.SendEmailAsync(usuario.Id,
                        "Fórum Bytebank - Alteração de senha",
                        $"Clique aqui {linkDeCallback} para alterar sua senha!");
                }
                return View("EmailAlteracaoSenhaEnviado");
            }
            return View();
        }

        public ActionResult ConfirmacaoAlteracaoSenha(string usuarioId, string token)
        {
            var model = new ContaConfirmacaoAlteracaoSenhaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ContaConfirmacaoAlteracaoSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resultadoAlteracaoSenha = await UserManager.ResetPasswordAsync(model.UsuarioId,
                    model.Token, model.NovaSenha);

                if (resultadoAlteracaoSenha.Succeeded)
                    return RedirectToAction("Index", "Home");

                AdicicionaErros(resultadoAlteracaoSenha);
            }
            return View();
        }

        public async Task<ActionResult> MinhaConta()
        {
            var usuarioId = HttpContext.User.Identity.GetUserId();
            var usuario = await UserManager.FindByIdAsync(usuarioId);
            var modelo = new ContaMinhaContaViewModel
            {
                NomeCompleto = usuario.NomeCompleto,
                NumeroDeCelular = usuario.PhoneNumber,
                HabilitarAutenticacaoDeDoisFatores = usuario.TwoFactorEnabled,
                NumeroDeCelularConfirmado = usuario.PhoneNumberConfirmed
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<ActionResult> MinhaConta(ContaMinhaContaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
                usuario.NomeCompleto = model.NomeCompleto;
                usuario.PhoneNumber = model.NumeroDeCelular;

                if (!usuario.PhoneNumberConfirmed)
                    await EnviaSmsDeConfirmacaoAsync(usuario);
                else
                    usuario.TwoFactorEnabled = model.HabilitarAutenticacaoDeDoisFatores;

                var resultadoUpdate = await UserManager.UpdateAsync(usuario);

                if (resultadoUpdate.Succeeded)
                    RedirectToAction("Index", "Home");
                AdicicionaErros(resultadoUpdate);

            }
            return View();
        }

        private async Task EnviaSmsDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateChangePhoneNumberTokenAsync(usuario.Id, usuario.PhoneNumber);
            await UserManager.SendSmsAsync(usuario.Id, $"Token de confirmação: {token}");
        }

        public ActionResult VerificacaoCodigoCelular() => View();

        [HttpPost]
        public async Task<ActionResult> VerificacaoCodigoCelular(string token)
        {
            var usuario = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var resultado = await UserManager.ChangePhoneNumberAsync(usuario.Id, usuario.PhoneNumber, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");

            AdicicionaErros(resultado);
            return View();
        }

        public async Task<ActionResult> VerificacaoDoisFatores()
        {
            var resultado = await SignInManager.SendTwoFactorCodeAsync("SMS");
            if (resultado)
                return View();
            return View("Error");
        }

        [HttpPost]
        public async Task<ActionResult> VerificacaoDoisFatores(ContaVerificacaoDoisFatoresViewModel model)
        {
            var resultado = await SignInManager.TwoFactorSignInAsync("SMS", 
                                                                     model.Token, 
                                                                     isPersistent: model.ContinuarLogado, 
                                                                     rememberBrowser: model.LembrarDesteComputador);
            if (resultado == SignInStatus.Success)
                return RedirectToAction("Index", "Home");
            return View("Error");
        }

        [HttpPost]
        public ActionResult EsquecerNavegador()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> DeslogarDeTodosOsLocais()
        {
            var usuarioId = HttpContext.User.Identity.GetUserId();
            await UserManager.UpdateSecurityStampAsync(usuarioId);

            return RedirectToAction("Index", "Home");
        }

    }

}