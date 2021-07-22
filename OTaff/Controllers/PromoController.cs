using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OTaff.Lib.Extensions;
using OTaff.Lib.Money;
using ServerLib;
using SharedLib.Models;

namespace OTaff.Controllers
{
    [Route("api/promo")]
    [ApiController]
    public class PromoController : ControllerBase
    {
        public static decimal EurPromoCost = 2.99m;

        [HttpPost("promote")]
        public ActionResult<string> Promote(
            [FromForm] string token,
            [FromForm] string methodId,
            [FromForm] string postId,
            [FromForm] Currency currency = Currency.EUR,
            [FromForm] bool isSearch = false)
        {
            var jwt = token.ToObject<Jwt>();
            if (!jwt.Verify()) return Unauthorized();

            var method = methodId is "WALLET" ? new UserPaymentMethod 
            { 
                Id = "WALLET", 
                MethodId = "WALLET", 
                Type = MethodType.YupWallet 
            } : Db.PaymentMethodsCollection.Find(x => x.Id == methodId && x.UserId == jwt.UserId).FirstOrDefault();
            if (method is null) return NotFound("Please select a valid payment method");
            var post = Db.JobPostsCollection.First(x => x.Id == postId && x.UserId == jwt.UserId);
            if (post is null) return NotFound("Post doesn't exist");
            var desc = $"Promotion for post {post.Id}";
            var charge = ChargeCtl.ChargeMethod(
                    jwt.GetUser(),
                    currency,
                    method,
                    new decimal[] {2.99m, 2.99m, 2.99m },
                    desc,
                    new JobPromoteActionData 
                    { 
                        Description = desc,
                        ActionType = BillAction.Promote,
                        IsSearch = false,
                        PostId = post.Id,
                        Issued = DateTime.UtcNow,
                        UserId = jwt.UserId,
                        Executed = false,
                    });

            return charge.ToJson();
        }

        static decimal ProfilePromoEurCost = 0.99m;
        [HttpPost("promoteprofile")]
        public ActionResult<string> PromoteProfile([FromForm] string token, [FromForm] string postId, [FromForm] string methodId)
        {
            var jwt = token.ToObject<Jwt>();
            if (!jwt.Verify()) return Unauthorized();

            var method = methodId is "WALLET" ? new UserPaymentMethod
            {
                Id = "WALLET",
                MethodId = "WALLET",
                Type = MethodType.YupWallet
            } : Db.PaymentMethodsCollection.Find(x => x.Id == methodId && x.UserId == jwt.UserId).FirstOrDefault();
            var post = Db.JobSearchPostsCollection.First(x => x.Id == postId);
            if (post is null) return NotFound("Post not found");
            var desc = $"Promotion for post: {post.Title}";
            var charge = ChargeCtl.ChargeMethod(
                jwt.GetUser(),
                Currency.EUR,
                method,
                new[] { .99m, .99m, .99m },
                desc,
                new JobPromoteActionData
                {
                    ActionType = BillAction.Promote,
                    Description = desc,
                    IsSearch = true,
                    PostId = post.Id,
                    Executed = false,
                    Issued = DateTime.UtcNow,
                    UserId = jwt.UserId,
                });
            return charge.ToJson();
        }

        static Random Rnd = new Random();
        
        [HttpPost("getjob/{cat}")]
        public ActionResult<string> GetOne(JobCategory cat)
        {
            var posts = Db.JobPostsCollection.Find(x =>
                x.Promoted
                && x.Categories.Contains(cat)
                && x.Active
                && x.PostDate > DateTime.UtcNow - TimeSpan.FromDays(6)).ToList();
            var selected = posts[Rnd.Next(0, posts.Count)];
            return selected.ToJson();
        }

        [HttpPost("getsearch/{cat}")]
        public ActionResult<string> GetSearch(JobCategory cat)
        {
            var posts = Db.JobSearchPostsCollection.Find(x =>
                x.Promoted && x.Categories.Contains(cat) && x.PostDate > DateTime.UtcNow-TimeSpan.FromDays(15)).ToList();



            return default;
        }

    }
}
