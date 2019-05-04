using ByteBank.ForumNaviteFramework.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.ForumNaviteFramework.Controllers
{
    
    public class TopicoController : Controller
    {
        public TopicoController()
        {
        }

        [Authorize]
        public ActionResult Criar()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Criar(TopicoCriarViewModel model)
        {
            return View();
        }

    }
}