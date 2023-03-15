using System.Security.Claims;

namespace UrlShortener.Utils;

public class ContextUserId
{
    public static int? FromClaims(ClaimsPrincipal claims)
    {
        var usrClaim = claims.Claims.FirstOrDefault(c => c.Type == "usr")?.Value;
        if (string.IsNullOrEmpty(usrClaim))
        {
            return null;
        }

        return int.Parse(usrClaim);
    }
}