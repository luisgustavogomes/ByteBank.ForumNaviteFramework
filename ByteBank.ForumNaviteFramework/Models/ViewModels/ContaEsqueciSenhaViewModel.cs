﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ByteBank.ForumNaviteFramework.Models.ViewModels
{
    public class ContaEsqueciSenhaViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}