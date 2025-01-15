using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Movies;

namespace WebApplication1.Controllers
{
    public class MovieController : Controller
    {
        private readonly MoviesDbContext _context;

        public MovieController(MoviesDbContext context)
        {
            _context = context;
        }

        // GET: MovieControler
        public async Task<IActionResult> Index(int page =1 , int size =20)
        {
            var query = 
                from movie in _context.Movies
                join movieCompany in _context.MovieCompanies on movie.MovieId equals movieCompany.MovieId
                join productionCompany in _context.ProductionCompanies on movieCompany.CompanyId equals productionCompany.CompanyId into productionGroup
                from productionCompany in productionGroup.DefaultIfEmpty() // LEFT JOIN
                group new { movie, productionCompany } by new { productionCompany.CompanyId, productionCompany.CompanyName } into grouped // Group by both CompanyId and CompanyName
                select new MovieView()
                {
                    company_id = grouped.Key.CompanyId, 
                    company_name = grouped.Key.CompanyName,
                    movies_count = grouped.Count(g => g.movie != null),
                    company_budget = grouped.Sum(g => (long)g.movie.Budget) 
                };
            var resultList = query.ToList();
            
            return View(resultList);
        }
        
        public async Task<IActionResult> VideoList(int? company_id, string? company_name)
        {
            var query = _context.Movies
                .Join(
                    _context.MovieCompanies,
                    movie => movie.MovieId, 
                    movieCompany => movieCompany.MovieId, 
                    (movie, movieCompany) => new { Movie = movie, MovieCompany = movieCompany } 
                )
                .Where(joined => joined.MovieCompany.CompanyId == company_id) 
                .Select(joined => new VideoListView
                {
                    title = joined.Movie.Title,
                    popularity = joined.Movie.Popularity,
                    revenue = joined.Movie.Revenue,
                    runtime = joined.Movie.Runtime,
                    votes_avg = joined.Movie.VoteAverage,
                    votes_count = joined.Movie.VoteCount,
                    comp_name = company_name,
                    movie_id = joined.Movie.MovieId
                    
                });
            var resultList = query.ToList();
            
            return View(resultList);
        }
        
        public async Task<IActionResult> Keywords(int? movie_id)
        {
            
            
            return View();
        }

        // GET: MovieControler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: MovieControler/Create
        public IActionResult Create()
        {
            return View();
        }
        
        

        // POST: MovieControler/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,Title,Budget,Homepage,Overview,Popularity,ReleaseDate,Revenue,Runtime,MovieStatus,Tagline,VoteAverage,VoteCount")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // GET: MovieControler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: MovieControler/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,Title,Budget,Homepage,Overview,Popularity,ReleaseDate,Revenue,Runtime,MovieStatus,Tagline,VoteAverage,VoteCount")] Movie movie)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: MovieControler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: MovieControler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }
    }
}
