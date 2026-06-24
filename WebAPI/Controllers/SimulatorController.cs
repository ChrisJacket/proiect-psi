using DataModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulatorController : ControllerBase
    {
        private static readonly object _lock = new object();
        private static readonly List<PumpSystemStatusEvent> _events = new List<PumpSystemStatusEvent>();
        private const int MaxStoredEvents = 500;

        [HttpGet]
        public IEnumerable<PumpSystemStatusEvent> Get()
        {
            lock (_lock)
            {
                return _events.ToArray();
            }
        }

        [HttpGet("latest")]
        public ActionResult<PumpSystemStatusEvent> GetLatest()
        {
            lock (_lock)
            {
                if (_events.Count == 0) return NotFound();
                return _events[_events.Count - 1];
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] PumpSystemStatusEvent value)
        {
            if (value == null) return BadRequest();
            lock (_lock)
            {
                _events.Add(value);
                if (_events.Count > MaxStoredEvents)
                {
                    _events.RemoveRange(0, _events.Count - MaxStoredEvents);
                }
            }
            return Ok();
        }

        [HttpDelete]
        public IActionResult Clear()
        {
            lock (_lock)
            {
                _events.Clear();
            }
            return NoContent();
        }
    }
}
