using ByteBank.ForumNaviteFramework.App_Start.Identity;
using ByteBank.ForumNaviteFramework.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;

[assembly: OwinStartup(typeof(ByteBank.ForumNaviteFramework.Startup))]
namespace ByteBank.ForumNaviteFramework
{
    public class Startup
    {

        public void Configuration(IAppBuilder builder)
        {
            builder.CreatePerOwinContext<DbContext>(() => new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"));
            builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var dbContext = contextoOwin.Get<DbContext>();
                    return new UserStore<UsuarioAplicacao>(dbContext);
                });

            builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var userStore = contextoOwin.Get<IUserStore<UsuarioAplicacao>>();
                    var userManager = new UserManager<UsuarioAplicacao>(userStore);

                    var userValidator = new UserValidator<UsuarioAplicacao>(userManager)
                    {
                        RequireUniqueEmail = true
                    };

                    userManager.UserValidator = userValidator;

                    userManager.PasswordValidator = new SenhaValidador()
                    {
                        TamanhoRequerido = 6,
                        ObrigatorioCaracteresEspeciais = true,
                        ObrigatorioDigitos = true,
                        ObrigatorioLowerCase = true,
                        ObrigatorioUpperCase = true
                    };

                    userManager.EmailService = new EmailServico();

                    return userManager;
                });
        }
    }
}