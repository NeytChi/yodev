using System;
using Serilog;
using System.Linq;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Managment;
using Models.AdminPanel;

namespace Controllers
{
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        public Admins admins = new Admins();

        [HttpPost]
        [ActionName("SignIn")]
        public ActionResult<dynamic> SignIn(AdminCache cache)
        {
            string message = string.Empty;
            string authToken = admins.AuthToken(cache, ref message);
            if (!string.IsNullOrEmpty(authToken)) {
                return new { success = true, data = new { auth_token = authToken }};
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(AdminCache cache)
        {
            string message = null;
            if (admins.RecoveryPassword(cache.admin_email, ref message)) {
                return new { success = true, 
                    message = "Every thing is fine. Check your email to get recovery code." };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(AdminCache cache)
        {
            string message = null;
            if (admins.ChangePassword(cache, ref message)) {
                return new { success = true, message = "Your password was changed." };
            }
            return Return500Error(message);
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
                Response.StatusCode = 500;
            
            admins.log.Warning(message + " IP -> " + HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new { success = false, message = message };
        }
    }
}