using ByteBank.ForumNaviteFramework.Models;
using ByteBank.ForumNaviteFramework.Models.ViewModels;
using ByteBank.ForumNaviteFramework.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.ForumNaviteFramework.Controllers
{
    [Authorize(Roles = RolesNomes.ADMINISTRADOR)]
    public class UsuarioController : Controller
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

        private RoleManager<IdentityRole> _roleManager;
        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                if (_roleManager == null)
                    _roleManager = HttpContext.GetOwinContext().GetUserManager<RoleManager<IdentityRole>>();
                return _roleManager;
            }
            set
            {
                _roleManager = value;
            }
        }

        public ActionResult Index()
        {
            var usuario = UserManager.Users.ToList().Select(u => new UsuarioViewModel(u));

            return View(usuario);
        }

        public async Task<ActionResult> EditarFuncoes(string id)
        {
            var usuario = await UserManager.FindByIdAsync(id);
            var model = new UsuarioEditarFuncoesViewModel(usuario, RoleManager);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditarFuncoes(UsuarioEditarFuncoesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByIdAsync(model.Id);
                var rolesUsuario = await UserManager.GetRolesAsync(usuario.Id);
                var resultadoRemocao = await UserManager.RemoveFromRolesAsync(model.Id, rolesUsuario.ToArray());


                if (resultadoRemocao.Succeeded)
                {
                    var funcoesSelecionadas = model.Funcoes.Where(f => f.Selecionado).Select(f => f.Nome).ToArray();
                    var resultadoAdicao = await UserManager.AddToRolesAsync(model.Id, funcoesSelecionadas);

                    if (resultadoAdicao.Succeeded)
                        return RedirectToAction("Index", "Usuario");
                }
            }
            return View();
        }
    }
}