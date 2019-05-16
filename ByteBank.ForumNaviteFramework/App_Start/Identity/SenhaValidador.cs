using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ByteBank.ForumNaviteFramework.App_Start.Identity
{
    public class SenhaValidador : IIdentityValidator<string>
    {

        public int TamanhoRequerido { get; set; }
        public bool ObrigatorioCaracteresEspeciais { get; set; }
        public bool ObrigatorioLowerCase { get; set; }
        public bool ObrigatorioUpperCase { get; set; }
        public bool ObrigatorioDigitos { get; set; }

#pragma warning disable CS1998 // Este método assíncrono não possui operadores 'await' e será executado de modo síncrono. É recomendável o uso do operador 'await' para aguardar chamadas à API desbloqueadas ou do operador 'await Task.Run(...)' para realizar um trabalho associado à CPU em um thread em segundo plano.
        /// <summary>
        /// Cria um IdentityResult àpartir de uma senha.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ValidateAsync(string item)
#pragma warning restore CS1998 // Este método assíncrono não possui operadores 'await' e será executado de modo síncrono. É recomendável o uso do operador 'await' para aguardar chamadas à API desbloqueadas ou do operador 'await Task.Run(...)' para realizar um trabalho associado à CPU em um thread em segundo plano.
        {
            var erros = new List<string>();

            if (ObrigatorioCaracteresEspeciais && !VerificaCaracteresEspeciais(item))
                erros.Add("A senha deve conter caracteres especiais!");

            if (!VerificaTamanhoRequerido(item))
                erros.Add($"A senha deve conter no mínimo {TamanhoRequerido} caracteres.");

            if (ObrigatorioLowerCase && !VerificaLowercase(item))
                erros.Add($"A senha deve conter no mínimo uma letra minúscula.");

            if (ObrigatorioUpperCase && !VerificaUppercase(item))
                erros.Add($"A senha deve conter no mínimo uma letra maiúscula.");

            if (ObrigatorioDigitos && !VerificaDigito(item))
                erros.Add($"A senha deve conter no mínimo um dígito.");

            if (erros.Any())
                return IdentityResult.Failed(erros.ToArray());
            else
                return IdentityResult.Success;
        }

        private bool VerificaTamanhoRequerido(string senha) => senha?.Length >= TamanhoRequerido;
        private bool VerificaCaracteresEspeciais(string senha) => Regex.IsMatch(senha, @"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");
        private bool VerificaLowercase(string senha) => senha.Any(char.IsLower);
        private bool VerificaUppercase(string senha) => senha.Any(char.IsUpper);
        private bool VerificaDigito(string senha) => senha.Any(char.IsDigit);
    }
}