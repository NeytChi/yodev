using System;
using System.Web;
using Serilog;
using System.Linq;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Common;
using Managment;
using Models;
using Models.AdminPanel;


namespace Controllers
{
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class LendingController : ControllerBase
    {
        public LendingController(Context context)
        {
            this.context = context;
        }
        public Context context;
        public Logger log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        [HttpPost]
        [Authorize]
        [ActionName("Create")]
        public ActionResult<dynamic> Create(LendCache cache)
        {
            LendPiece lend = new LendPiece() {
                lendBody = HttpUtility.UrlDecode(cache.lend_body),
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                deleted = false
            };
            context.LendPieces.Add(lend);
            context.SaveChanges();
            log.Information("Create new lend piece, id -> " + lend.lendId);
            return new { success = true, data = new {
                lend_id = lend.lendId,
                lend_body = lend.lendBody,
                created_at = lend.createdAt
            }};
        }
        [HttpPost]
        [Authorize]
        [ActionName("Update")]
        public ActionResult<dynamic> Update(LendCache cache)
        {
            LendPiece lend; string message = string.Empty;

            if ((lend = context.LendPieces.Where(l => l.lendId == cache.lend_id && !l.deleted).FirstOrDefault()) != null) {
                lend.lendBody = HttpUtility.UrlDecode(cache.lend_body);
                context.LendPieces.Update(lend);
                context.SaveChanges();
                log.Information("Update lend piece, id -> " + lend.lendId);
                return new { success = true, data = new {
                    lend_id = lend.lendId,
                    lend_body = lend.lendBody,
                    created_at = lend.createdAt
                }};
            }
            else
                message = "Can't define lending piece by id, -> " + cache.lend_id;
            return Return500Error(message);
        }
        [HttpPost]
        [Authorize]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete(LendCache cache)
        {
            LendPiece lend; string message = string.Empty;

            if ((lend = context.LendPieces.Where(l => l.lendId == cache.lend_id && !l.deleted).FirstOrDefault()) != null) {
                lend.deleted = true;
                context.LendPieces.Update(lend);
                context.SaveChanges();
                log.Information("Delete lend piece, id -> " + lend.lendId);
                return new { success = true };
            }
            else
                message = "Can't define lending piece by id, -> " + cache.lend_id;
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("All")]
        public ActionResult<dynamic> All([FromQuery] int since = 0, [FromQuery] int count = 30)
        {
            log.Information("Get all lend pieces");
            return new { success = true, data = context.LendPieces.Where(l => !l.deleted)
                .Select(x => new {
                    lend_id = x.lendId,
                    lend_body = x.lendBody,
                    created_at = x.createdAt
                })
                .Skip(since * count).Take(count).ToArray() };
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
                Response.StatusCode = 500;
            
            log.Warning(message + " IP -> " + HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new { success = false, message = message };
        }
    }
    public struct LendCache
    {
        public int lend_id;
        public string lend_body;
    }
}