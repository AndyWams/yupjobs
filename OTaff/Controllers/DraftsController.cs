using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OTaff.Lib.Extensions;
using ServerLib;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTaff.Controllers
{
    [Route("api/drafts")]
    [ApiController]
    public class DraftsController : ControllerBase
    {
        [HttpPost("jpsave")]
        public ActionResult<string> SaveDraft([FromForm] string token, [FromForm] string jpost)
        {
            var jwt = token.ToObject<Jwt>();
            if (!jwt.Verify()) return Unauthorized();
            var post = jpost.ToObject<JobPost>();
            post.UserId = jwt.UserId;

            var cnt = Db.DraftJobPosts.CountDocuments(x => x.UserId == jwt.UserId);
            if (cnt > 1)
            {
                Db.DraftJobPosts.DeleteMany(x => x.UserId == jwt.UserId);
                cnt = 0;
            }
            if (cnt is 0)
                Db.DraftJobPosts.InsertOne(post);
            else
            {
                var id = post.Id;
                //post.Id = null;
                Db.DraftJobPosts.UpdateOne(x=>x.Id==post.UserId, new UpdateDefinitionBuilder<JobPost>()
                    .Set(x=>x.Description, post.Description)
                    .Set(x=>x.Title, post.Title)
                    .Set(x=>x.Tags, post.Tags)
                    .Set(x=>x.WorkDuration, post.WorkDuration));
            }

            return post.Id;
        }

        [HttpPost("jpget")]
        public ActionResult<JobPost> Get([FromForm] string token)
        {
            var jwt = token.ToObject<Jwt>();
            if (!jwt.Verify()) return Unauthorized();
            var post = Db.DraftJobPosts.First(x => x.UserId == jwt.UserId) ?? new JobPost();
            return post;
        }
    }
}
