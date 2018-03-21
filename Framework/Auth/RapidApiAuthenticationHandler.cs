﻿using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Framework.Auth
{
    public class RapidApiAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IConfiguration _configuration { get; set; }

        public RapidApiAuthenticationHandler(
            IConfiguration configuration,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        public const string RapidApiSecretHeaderName = "X-Mashape-Proxy-Secret";
        public const string RapidApiUsernameHeaderName = "X-Mashape-User";
        private const string RapidApiConfigSetting = "RapidApiSecret";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            var providedSecret = Request.Headers[RapidApiSecretHeaderName];
            if (string.IsNullOrEmpty(providedSecret))
            {
                return AuthenticateResult.NoResult();
            }

            if (!await Task.Run(() => IsCorrectSecret(providedSecret)))
            {
                return AuthenticateResult.NoResult();
            }

            var username = Request.Headers[RapidApiUsernameHeaderName];
            if (string.IsNullOrEmpty(providedSecret))
            {
                return AuthenticateResult.NoResult();
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, ClaimsIssuer) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            var ticket = new AuthenticationTicket(principal, null, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        protected bool IsCorrectSecret(string providedSecret) 
        {
            return !string.IsNullOrEmpty(providedSecret) && providedSecret.Equals(GetCorrectRapidApiSecret());
        }

        private string GetCorrectRapidApiSecret()
        {
            return _configuration[RapidApiConfigSetting];
        }
    }
}