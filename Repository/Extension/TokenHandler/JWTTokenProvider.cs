using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Repository.Data.Entities;
using Repository.DTO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomFunctions.TokenHandler
{
    public sealed class JWTTokenProvider(IConfiguration config)
    {
        /*
         * tl;dr on how jwt token works
         * each token is a long string of these json properties groups (separated by a dot .) encrypted in Base64:
         *  + The header (encoded base64, can be decoded into plain text):
         *      - jwt signing algorithm: hs256 or rs256 for example
         *      - token type: usually jwt maybe
         *  + The payload (encoded base64, can be decoded into plain text):
         *      - token claims: sub (uid), email, custom claims like role and username for example
         *      - other data to send with the token
         *  + The signature (the more complex part):
         *      - created with the defined signing algorithm
         *      - using the datas and the secret key to create a unique token
         *      - the secret key can be user-defined, or an arbitrary/encrypted/encoded/generated random string
         * how the token is actually verified server-side:
         *  + server generate a test signature using the data received in the token
         *  + if the test signature matches the received signature, it verified that the header and payload has not been modified
         *  + plus if an attacker were to tamper the data, they do not have knowledge of the secret key to sign the token
         *  + essentially ensuring a nonreplicable unique token
         */

        //One way of generating a token
        /// <summary>
        /// .Include() Role in the User object sent to this
        /// </summary>
        public (string token, DateTime expire) GenerateAccessToken(User user)
        {
            if(user == null)
            {
                throw new ArgumentException("User is null");
            }
            var expire = DateTime.UtcNow.AddMinutes(config.GetValue<int>("JWT:ExpirationTimeMinutes"));
            //get secret authkey in the config
            string secretKey = config["JWT:Secret"];
            //encode the secret key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            //define the signing algorithm and generation of the signature
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            //define the payload
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.Username),
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                        //new Claim("RoleName", user.Role.Name)
                        //new Claim("email_verified",)
                    ]),
                Expires = expire,
                SigningCredentials = credentials,
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"]
            };

            //JsonWebTokenHandler: less popular but faster and more lightweight
            //this can allow the handler to create a string token
            //JwtSecurityTokenHandler: more popular and secure but heavier with extra overhead
            //this requires the token to be serialized (WriteToken) to return as a string
            //JsonWebTokenHandler returns raw strings and ClaimsIdentity,
            //while JwtSecurityTokenHandler returns rich JwtSecurityToken objects.
            #region
            /*
            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
            */
            #endregion
            var handler = new JsonWebTokenHandler();

            var token = handler.CreateToken(tokenDescriptor);

            return (token, expire);
        }

        //Implement Refresh TOken here
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public RefreshTokensResponse RefreshTokenAsync(User user)
        {
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            return new RefreshTokensResponse
            {
                AccessToken = newAccessToken.token,
                RefreshToken = newRefreshToken
            };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, //allow expired tokens to be parsed

                ValidIssuer = config["JWT:Issuer"],
                ValidAudience = config["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null; //reject token if algorithm is tampered
                }

                return principal;
            }
            catch
            {
                return null; //invalid token (bad signature, etc.)
            }
        }
    }
}
