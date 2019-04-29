using ByteBank.ForumNaviteFramework.Models;
using ByteBank.ForumNaviteFramework.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
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
                {
                    _userManager = HttpContext.GetOwinContext().GetUserManager<UserManager<UsuarioAplicacao>>();
                }
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

                await UserManager.CreateAsync(novoUsuario, model.Senha);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }


    }
}