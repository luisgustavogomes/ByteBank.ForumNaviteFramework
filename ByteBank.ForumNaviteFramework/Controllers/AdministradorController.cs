using ByteBank.ForumNaviteFramework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.ForumNaviteFramework.Controllers
{

    [Authorize(Roles = RolesNomes.ADMINISTRADOR)]
    public class AdministradorController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }
    }
}