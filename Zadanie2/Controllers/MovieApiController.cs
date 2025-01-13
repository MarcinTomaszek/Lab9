using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Movies;

namespace WebApplication1.Controllers;


    [ApiController]
    [Route("/api/companies")]
    public class MovieApiController : ControllerBase
    {
        private readonly MoviesDbContext _context;

        public MovieApiController(MoviesDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiltered(string fragment)
        {
             return  Ok(
                    _context.ProductionCompanies
                        .Where(c=>c.CompanyName!=null&&c.CompanyName.Contains(fragment))
                        .AsNoTracking()
                        .AsAsyncEnumerable()
                );
        }
    }
