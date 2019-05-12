using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ByteBank.ForumNaviteFramework.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public string Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }


        public UsuarioViewModel() {}
        public UsuarioViewModel(UsuarioAplicacao usuario)
        {
            Id = usuario.Id;
            NomeCompleto = usuario.NomeCompleto;
            UserName = usuario.UserName;
            Email = usuario.Email;
        }
    }
}