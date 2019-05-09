﻿using ByteBank.ForumNaviteFramework.App_Start.Identity;
using ByteBank.ForumNaviteFramework.Models;
using ByteBank.ForumNaviteFramework.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Configuration;
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
                    var dataProtectionProvider = opcoes.DataProtectionProvider;
                    var dataProtectionProviderCreated = opcoes.DataProtectionProvider.Create("ByteBank.ForumNaviteFramework");

                    userManager.UserTokenProvider = new DataProtectorTokenProvider<UsuarioAplicacao>(dataProtectionProviderCreated);

                    userManager.MaxFailedAccessAttemptsBeforeLockout = 3;
                    userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(2);
                    userManager.UserLockoutEnabledByDefault = true;

                    

                    return userManager;
                });

            builder.CreatePerOwinContext<SignInManager<UsuarioAplicacao, string>>(
                (opcoes, contextoOwin) =>
                {
                    
                    var userManager = contextoOwin.Get<UserManager<UsuarioAplicacao>>();
                    var signInManager = new SignInManager<UsuarioAplicacao, string>(userManager, contextoOwin.Authentication);
                    return signInManager;
                });

            builder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });

            builder.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            builder.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["google:client_id"],
                ClientSecret = ConfigurationManager.AppSettings["google:client_secret"],
                Caption = "Google"
            });

            using (var dbContext = new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"))
            {
                CriarRoles(dbContext);
                CriarAdministrador(dbContext);
            }
        }

        private void CriarAdministrador(IdentityDbContext<UsuarioAplicacao> dbContext)
        {
            using (var userStore = new UserStore<UsuarioAplicacao>(dbContext))
            using (var userManager = new UserManager<UsuarioAplicacao>(userStore))
            {
                var administradorEmail = ConfigurationManager.AppSettings["admin:email"];
                var administrador = userManager.FindByEmail(administradorEmail);

                if (administrador != null)
                    return;

                administrador = new UsuarioAplicacao
                {
                    Email = administradorEmail,
                    UserName = ConfigurationManager.AppSettings["admin:user_name"],
                    EmailConfirmed = true
                };
                

                userManager.Create(administrador, ConfigurationManager.AppSettings["admin:senha"]);

                userManager.AddToRole(administrador.Id, RolesNomes.ADMINISTRADOR);
            }
        }

        private void CriarRoles(IdentityDbContext<UsuarioAplicacao> dbContext)
        {
            using (var roleStore = new RoleStore<IdentityRole>(dbContext))
            using (var roleManager = new RoleManager<IdentityRole>(roleStore))
            {
                if (!roleManager.RoleExists(RolesNomes.ADMINISTRADOR))
                roleManager.Create(new IdentityRole(RolesNomes.ADMINISTRADOR));
                if (!roleManager.RoleExists(RolesNomes.MODERADOR))
                    roleManager.Create(new IdentityRole(RolesNomes.MODERADOR));
            }

            
        }
    }
}