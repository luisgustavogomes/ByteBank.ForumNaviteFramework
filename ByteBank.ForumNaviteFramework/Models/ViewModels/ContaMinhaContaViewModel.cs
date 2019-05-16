using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ByteBank.ForumNaviteFramework.Models.ViewModels
{
    public class ContaMinhaContaViewModel
    {
        [Required]
        [Display(Name ="Nome Completo")]
        public string NomeCompleto { get; set; }

        [Required]
        [Display(Name = "Número de celular")]
        public string NumeroDeCelular { get; set; }

        [Display(Name ="Habilitar autententicação de dois fatores")]
        public bool HabilitarAutenticacaoDeDoisFatores { get; set; }

        public bool NumeroDeCelularConfirmado { get; set; }
    }
}