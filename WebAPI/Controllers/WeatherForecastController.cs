using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Persistencia;
using Dominio;
using System.Linq;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        public readonly CursosOnlineContext context;

        public WeatherForecastController(CursosOnlineContext _context) {
            this.context = _context;
        }

        [HttpGet]
        public IEnumerable<Curso> Get()
        {
            return context.Curso.ToList();
        }
    }
}
