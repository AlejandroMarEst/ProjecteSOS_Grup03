using System.IdentityModel.Tokens.Jwt;
namespace ProjecteSOS_Grup03WebPage.Tools
{
    public static class TokenHelper
    {
        public static bool IsTokenSession(string? token)
        {
            return !string.IsNullOrEmpty(token) && !IsTokenExpired(token);
        }

        /// <summary>
        /// Validar si el token ha expirat o no
        /// Requereix de la instal·lacio de la llibreria System.IdentityModel.Tokens.Jwt
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsTokenExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var expiration = jwt.ValidTo;
            return expiration < DateTime.UtcNow;
        }

        public static IEnumerable<string> GetUserRoles(string? token)
        {
            var userRoles = new List<string>();
            
            if (IsTokenSession(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                userRoles = jwtToken.Claims
                    .Where(c => c.Type == "role" || c.Type == "roles" || c.Type.EndsWith("/role"))
                    .Select(c => c.Value)
                    .ToList();
            }

            return userRoles;
        }
    }
}
