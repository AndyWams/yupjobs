using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace VoiceChatServer.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        static ConcurrentDictionary<string, (List<RoomMember>, MemoryStream)> Rooms = new ConcurrentDictionary<string, (List<RoomMember>, MemoryStream)>();

        [HttpPost("create/{id}")]
        public ActionResult<string> Create(string id)
        {
            //Rooms[id] = new List<RoomMember>();
            return id;
        }

        [HttpPost("join/{id}/{username}")]
        public ActionResult<string> Join(string id, string username)
        {
            string roomUrl = $"https://voice.yupjobs.net/room/{id}";

            return roomUrl;
        }

        [HttpPost("post")]
        public ActionResult<string> Post(string username, [FromForm] byte[] bytes)
        {

        }

        [HttpPost("get/{id}/{username}")]
        public ActionResult<string> Get(string id, string username)
        {

        }
    }

    public class RoomMember
    {
        public HostString Host { get; set; }
        public string Username { get; set; }
    }
}
