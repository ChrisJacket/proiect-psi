using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
         
    [Route("api/[controller]")]
    [ApiController]
    public class SimulatorController : ControllerBase
    {
        private static List<ProcessStatusEvent> stamps = null;

        public SimulatorController()
        {
            if(stamps==null)
            {
                stamps = new List<ProcessStatusEvent>();
            }
        }
        
        // GET: api/Simulator
        [HttpGet]
        public IEnumerable<ProcessStatusEvent> Get()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return stamps;
        }

        // GET: api/Simulator/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Simulator    
        [HttpPost()]
        public void Post([FromBody] ProcessStatusEvent value)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            stamps.Add(value);
        }

        // PUT: api/Simulator/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }    
}
