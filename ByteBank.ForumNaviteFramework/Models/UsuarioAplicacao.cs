﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ByteBank.ForumNaviteFramework.Models
{
    public class UsuarioAplicacao : IdentityUser
    {
        public string NomeCompleto { get; set; }
    }
}