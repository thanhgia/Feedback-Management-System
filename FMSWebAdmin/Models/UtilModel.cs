using FMSWebAdmin.Models.Dataset;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models
{
    public class UtilModel
    {
        private readonly IConfiguration _config;
        private UtilConstant uConstant = new UtilConstant();
        private readonly Random _random = new Random();

        public UtilModel() {}
        public UtilModel(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(string email)
        {
            string keySecret = _config.GetValue<string>("Jwt:SecretKey");
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var myClaim = new List<Claim>();
            myClaim.Add(new Claim("Email", email));
            // myClaim.Add(new Claim(ClaimTypes.Role, "Customer"));
            // create token
            var token = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Jwt:ValidIssuer"),
                audience: _config.GetValue<string>("Jwt:ValidAudience"),
                expires: DateTime.Now.AddHours(1),
                signingCredentials: signingCredentials,
                claims: myClaim
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandlder = new JwtSecurityTokenHandler();
            var validParamaterToken = new TokenValidationParameters();
            validParamaterToken.ValidateLifetime = false;
            validParamaterToken.ValidateIssuer = true;
            validParamaterToken.ValidateAudience = true;
            validParamaterToken.ValidIssuer = _config.GetValue<string>("Jwt:ValidIssuer");
            validParamaterToken.ValidAudience = _config.GetValue<string>("Jwt:ValidAudience");
            validParamaterToken.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:SecretKey")));
            SecurityToken validToken;
            try
            {
                tokenHandlder.ValidateToken(token, validParamaterToken, out validToken);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string RandomString(int size)
        {
            var builder = new StringBuilder(size);

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                var @char = chars[random.Next(chars.Length)];
                builder.Append(@char);
            }

            return builder.ToString();
        }

        public async Task<bool> SendPushNoti(string[] deviceToken, string title, string body, object data)
        {
            bool sent = false;
            var messageInformation = new FMSMessage()
            {
                notification = new FMSNoti()
                {
                    title = title,
                    text = body
                },
                data = data,
                registration_ids = deviceToken
            };
            //Object to JSON STRUCTURE => using Newtonsoft.Json;
            string jsonMessage = JsonConvert.SerializeObject(messageInformation);

            var request = new HttpRequestMessage(HttpMethod.Post, uConstant.FIREBASE_URL);
            request.Headers.TryAddWithoutValidation("Authorization", "Key =" + uConstant.FIREBASE_KEY);
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                result = await client.SendAsync(request);
                sent = sent && result.IsSuccessStatusCode;
            }
            return sent;
        }
    }
}
