using Common;
using Serilog;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Route("api/[controller]/[action]/")]
    [ApiController]
    public class EmailController : ControllerBase
    {	
        public Mailer mailer = new Mailer(new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger());
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }
        [HttpPost]
        [ActionName("ContactUs")]
        public ActionResult<dynamic> ContactUs(ContactUsCache cache)
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(cache.message_email)) {
                mailer.ContactUsReceiver(cache);
                return new { success = true };
            }
            else    
                message = "Email message can't be empty.";
            return Return500Error(message);
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
                Response.StatusCode = 500;
            
            mailer.log.Warning(message + " IP -> " + HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new { success = false, message = message };
        }
    }
    public struct ContactUsCache
    {
        public string message_name;
        public string message_email;
        public string message_company_name;
        public string message_budget;
        public string message_description;
    }
}
