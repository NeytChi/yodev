using System;
using System.Net;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


using Serilog;
using Serilog.Core;

using Common;
using Models.AdminPanel;

namespace Managment
{
    public class Admins
    {
        private Context context;
        private ProfileCondition profileCondition;
        // private MailF mail;
        public Logger log;
        public Admins(Logger log, Context context)
        {
            this.context = context;
            this.log = log;
            this.profileCondition = new ProfileCondition(log);
        }
        public Admins()
        {
            this.context = new Context(false);
            this.log = new LoggerConfiguration()
                .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            this.profileCondition = new ProfileCondition(log);
        }
        public Admin CreateAdmin(string adminEmail, string adminFullname, string adminPassword, string adminRole, ref string message)
        {
            if (profileCondition.EmailIsTrue(adminEmail, ref message)
                && FullNameIsTrue(adminFullname, ref message)
                && profileCondition.PasswordIsTrue(adminPassword, ref message)) {
                if (GetNonDelete(adminEmail, ref message) == null) {
                    Admin admin = new Admin() {
                        adminEmail = adminEmail,
                        adminFullname = adminFullname,
                        adminPassword = profileCondition.HashPassword(adminPassword),
                        adminRole = adminRole,
                        passwordToken = "",
                        createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        lastLoginAt = 0,
                    };
                    context.Admins.Add(admin);
                    context.SaveChanges();
                    log.Information("Add new admin, id ->" + admin.adminId);
                    return admin;
                }
                else 
                    message = "Admin with this email is exist";
            }            
            return null;
        }
        public Admin CreateAdmin(AdminCache cache, ref string message)
        {
            if (profileCondition.EmailIsTrue(cache.admin_email, ref message) 
                && FullNameIsTrue(cache.admin_fullname, ref message)
                && profileCondition.PasswordIsTrue(cache.admin_password, ref message)) {
                if (GetNonDelete(cache.admin_email, ref message) == null) {
                        cache.admin_fullname = WebUtility.UrlDecode(cache.admin_fullname);
                        Admin admin = AddAdmin(cache);
                        return admin;
                }
                else
                    message = "Admin with this email is exist";
            }            
            return null;
        }
        public Admin AddAdmin(AdminCache cache)
        {
            Admin admin = new Admin() {
                adminEmail = cache.admin_email,
                adminFullname = cache.admin_fullname,
                adminPassword = profileCondition.HashPassword(cache.admin_password),
                adminRole = "default",
                passwordToken = profileCondition.CreateHash(10),
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                lastLoginAt = 0,
                recoveryCode = null,
                deleted = false,
            };
            context.Admins.Add(admin);
            context.SaveChanges();
            log.Information("Add new admin, id -> " + admin.adminId);
            return admin;
        }
        public string AuthToken(AdminCache cache, ref string message)
        {
            Admin admin = GetNonDelete(cache.admin_email, ref message);
            if (admin != null) {
                if (profileCondition.VerifyHashedPassword(admin.adminPassword, cache.admin_password))
                    return Token(admin);
                else
                    message = "Wrong password.";
            }
            return string.Empty;
        }
        public Admin GetNonDelete(int adminId, ref string message)
        {
            Admin admin = context.Admins.Where(a 
                => a.adminId == adminId
                && a.deleted == false).FirstOrDefault();
            if (admin == null)
                message = "Unknow admin id.";
            return admin;
        }
        public Admin GetNonDelete(string adminEmail, ref string message)
        {
            Admin admin = context.Admins.Where(a 
                => a.adminEmail == adminEmail 
                && a.deleted == false).FirstOrDefault();
            if (admin == null)
                message = "Unknow admin email.";
            return admin;
        }
        public Admin GetNonDeleteByToken(string passwordToken, ref string message)
        {
            Admin admin = context.Admins.Where(a 
                => a.passwordToken == passwordToken 
                && a.deleted == false).FirstOrDefault();
            if (admin == null)
                message = "Unknow admin password token.";
            return admin;
        }
        
        private string Token(Admin admin)
        {
            var identity = GetIdentity(admin);
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), 
                SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        private ClaimsIdentity GetIdentity(Admin admin)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, admin.adminId.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, admin.adminEmail),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, admin.adminRole)
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Bearer Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
        public bool RecoveryPassword(string adminEmail, ref string message)
        {
            Admin admin = GetNonDelete(adminEmail, ref message);
            if (admin != null) {
                admin.recoveryCode = profileCondition.CreateCode(6);
                context.Admins.Update(admin);
                context.SaveChanges();
                log.Information("Create code for admin recovery password, id -> " + admin.adminId);
                return true;
            }
            return false;
        }
        public bool ChangePassword(AdminCache cache, ref string message) 
        {
            Admin admin = context.Admins.Where(a => a.recoveryCode == cache.recovery_code).FirstOrDefault();
            if (admin != null) {
                if (cache.admin_password.Equals(cache.confirm_password)) {
                    if (profileCondition.PasswordIsTrue(cache.admin_password, ref message)) {   
                        admin.adminPassword = profileCondition.HashPassword(cache.admin_password);
                        admin.recoveryCode = null;
                        context.Admins.Update(admin);
                        context.SaveChanges();
                        log.Information("Change password for admin, id -> " + admin.adminId);
                        return true;
                    }
                }
                else message = "Passwords aren't equal to each other.";
            }
            else message = "Incorrect code entered";
            return false;
        }
        public bool FullNameIsTrue(string adminFullname, ref string message)
        {
            if (!string.IsNullOrEmpty(adminFullname)) {
                if (adminFullname.Length > 3 && adminFullname.Length < 100)
                    return true;
                else 
                    message = "Admin full name length required more than 3 characters & less that 100.";
            }
            else {
                message = "Admin fullname is null or empty";
            }
            return false;
        }
    }
}