using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

private string GenerateJwtToken(App_Users user)
{
    var claims = new List<Claim>
    {
        new Claim("uid", user.Id.ToString()),

        new Claim("companyId",
            user.company_id.ToString()),

        new Claim(ClaimTypes.Name, user.full_name),

        new Claim(ClaimTypes.Email, user.email)
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            _configuration["JWT:Key"]));

    var creds = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["JWT:Issuer"],
        audience: _configuration["JWT:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler()
        .WriteToken(token);
}