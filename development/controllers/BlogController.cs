using System;
using System.Web;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

using Models;
using Common;
using Managment;
using Models.AdminPanel;

namespace Controllers
{
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        public BlogController(Context context)
        {
            var config = Program.serverConfiguration();
            this.context = context;
            this.uploader = new AwsUploader(log);
            this.awsPath = config.GetValue<string>("aws_path");
        }
        public string awsPath;
        public Context context;
        public AwsUploader uploader;
        public Logger log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        [HttpPost]
        [Authorize]
        [ActionName("Create")]
        public ActionResult<dynamic> Create(IFormFile file, IFormCollection post)
        {
            BlogCache cache;

            cache = JsonConvert.DeserializeObject<BlogCache>(post["data"]);

            BlogPost blogPost = new BlogPost() {
                postImageUrl = uploader.SaveFile(file, "yodev-blog-post"),
                postTitle = HttpUtility.UrlDecode(cache.post_title),
                postBody = HttpUtility.UrlDecode(cache.post_body),
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                deleted = false
            };
            context.BlogPosts.Add(blogPost);
            context.SaveChanges();
            return new { success = true, data = new {
                post_id = blogPost.postId,
                post_image_url = awsPath + blogPost.postImageUrl,  
                post_title = blogPost.postTitle,
                post_body = blogPost.postBody,
                created_at = blogPost.createdAt
            }};
        }
        [HttpPost]
        [Authorize]
        [ActionName("Update")]
        public ActionResult<dynamic> Update(IFormCollection post, IFormFile file = null)
        {
            BlogCache cache;
            BlogPost blogPost; string message = string.Empty;

            cache = JsonConvert.DeserializeObject<BlogCache>(post["data"]);

            if ((blogPost = context.BlogPosts.Where(b => b.postId == cache.post_id && !b.deleted).FirstOrDefault()) != null) {
                if (file != null)
                    blogPost.postImageUrl = uploader.SaveFile(file, "yodev-blog-post");
                if (cache.post_title != null)
                    blogPost.postTitle = HttpUtility.UrlDecode(cache.post_title);
                if (cache.post_body != null)
                    blogPost.postBody = HttpUtility.UrlDecode(cache.post_body);
                context.BlogPosts.Update(blogPost);
                context.SaveChanges();
                return new { success = true, data = new {
                    post_id = blogPost.postId,
                    post_image_url = awsPath + blogPost.postImageUrl,  
                    post_title = blogPost.postTitle,
                    post_body = blogPost.postBody,
                    created_at = blogPost.createdAt
                }};
            }
            else
                message = "Can't define blog post by id, -> " + cache.post_id;
            return Return500Error(message);
        }
        [HttpPost]
        [Authorize]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete(BlogCache cache)
        {
            BlogPost post; string message = string.Empty;

            if ((post = context.BlogPosts.Where(p => p.postId == cache.post_id && !p.deleted).FirstOrDefault()) != null) {
                post.deleted = true;
                context.BlogPosts.Update(post);
                context.SaveChanges();
                return new { success = true };
            }
            else
                message = "Can't define blog post by id, -> " + cache.post_id;
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Short")]
        public ActionResult<dynamic> Short([FromQuery] int since = 0, [FromQuery] int count = 30)
        {
            return new { success = true, data = context.BlogPosts.Where(p => !p.deleted)
                .Select(x => new {
                    post_id = x.postId,
                    post_image_url = x.postId,
                    post_title = x.postTitle
                } )
                .Skip(since * count).Take(count).ToArray() };
        }
        [HttpGet("{id}")]
        [ActionName("Fully")]
        public ActionResult<dynamic> Fully(int id)
        {
            return new { success = true, data = context.BlogPosts.Where(p => !p.deleted
                && p.postId == id)
                .Select(x => new {
                    post_id = x.postId,
                    post_image_url = x.postId,
                    post_title = x.postTitle,
                    post_body = x.postBody
                } ).FirstOrDefault() };
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
                Response.StatusCode = 500;
            
            log.Warning(message + " IP -> " + HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new { success = false, message = message };
        }
    }
    public struct BlogCache
    {
        public int post_id;
        public string post_body;
        public string post_title;
    }
}