using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace BatchTests.Controllers {
    public class ValuesController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/Values/accepted")]
        public IHttpActionResult Accepted()
        {
            return StatusCode(HttpStatusCode.Accepted);
        }

        [HttpGet]
        [Route("api/Values/bad")]
        public IHttpActionResult Bad()
        {
            return BadRequest();
        }

        [HttpGet]
        [Route("api/Values/error")]
        public IHttpActionResult Error()
        {
            return InternalServerError();
        }

        [HttpGet]
        [Route("api/Values/redirect")]
        public IHttpActionResult Redirect()
        {
            return Redirect("https://google.com/");
        }
    }
}
