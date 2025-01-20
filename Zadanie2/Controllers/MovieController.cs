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
        public static int? movie_idToUpdate;
        public static int? company_id_curr;
        public static string? company_name_curr;
        public int size = 20;

        public MovieController(MoviesDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(int page=1)
        {
            var query = 
                from movie in _context.Movies
                join movieCompany in _context.MovieCompanies on movie.MovieId equals movieCompany.MovieId
                join productionCompany in _context.ProductionCompanies on movieCompany.CompanyId equals productionCompany.CompanyId into productionGroup
                from productionCompany in productionGroup.DefaultIfEmpty() 
                group new { movie, productionCompany } by new { productionCompany.CompanyId, productionCompany.CompanyName } into grouped
                select new MovieViewModelEntity()
                {
                    company_id = grouped.Key.CompanyId, 
                    company_name = grouped.Key.CompanyName,
                    movies_count = grouped.Count(g => g.movie != null),
                    company_budget = grouped.Sum(g => (long)g.movie.Budget) 
                };
            var resultList = await query.OrderByDescending(o=>o.movies_count).Skip((page - 1)*size).Take(size).ToListAsync();
            
            var totalPages = (int)Math.Ceiling(await query.CountAsync() / (double)size);
            
            return View(new MovieViewModel(){MovieModels = resultList, current_page = page, total_pages = totalPages});
        }
        
        public async Task<IActionResult> VideoList(int? company_id, string? company_name, int page=1)
        {
            if (company_id is null)
                company_id = company_id_curr;
            if (company_name is null)
                company_name = company_name_curr;
            company_id_curr = company_id;
            company_name_curr = company_name;
            var query = _context.Movies
                .Join(
                    _context.MovieCompanies,
                    movie => movie.MovieId, 
                    movieCompany => movieCompany.MovieId, 
                    (movie, movieCompany) => new { Movie = movie, MovieCompany = movieCompany } 
                )
                .Where(joined => joined.MovieCompany.CompanyId == company_id) 
                .Select(joined => new VideoListViewModelEntity()
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
            var resultList = await query.OrderByDescending(o=>o.movie_id).Skip((page - 1)*size).Take(size).ToListAsync();
            
            var totalPages = (int)Math.Ceiling(await query.CountAsync() / (double)size);
            
            return View(new VideoListViewModel(){VideoListModels = resultList, current_page = page, total_pages = totalPages} );
        }
        
        public async Task<IActionResult> Keywords(int? movie_id)
        {
            if(movie_id is not null)
                movie_idToUpdate = movie_id;
            
            var query = _context.Movies
                .Join(
                    _context.MovieKeywords, 
                    movie => movie.MovieId, 
                    movieKeyword => movieKeyword.MovieId,
                    (movie, movieKeyword) => new { movie, movieKeyword }
                )
                .Join(
                    _context.Keywords,
                    combined => combined.movieKeyword.KeywordId,
                    keyword => keyword.KeywordId,
                    (combined, keyword) => new { combined.movie, keyword }
                )
                .Where(result => result.movie.MovieId == movie_idToUpdate)
                .Select(result => new KeywordsViewModel(){Keyword = result.keyword.KeywordName, Movie_id = movie_idToUpdate})
                .ToList();
            
            return View(query);
        }
        
        public IActionResult CreateKeyword()
        {
            return View(new KeywordCreateForm());
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateKeyword(KeywordCreateForm model)
        {
            if (ModelState.IsValid)
            {
                var maxId = await _context.Keywords.MaxAsync(g => (int?)g.KeywordId) ?? 0;

                 _context.Keywords.Add(new Keyword
                {
                    KeywordId = maxId+1,
                    KeywordName = model.Keyword
                });


                _context.MovieKeywords.Add(new MovieKeyword
                {
                    MovieId = movie_idToUpdate,
                    KeywordId = maxId + 1
                });
                
                await _context.SaveChangesAsync();
                
                var query = _context.Movies
                    .Join(
                        _context.MovieKeywords, 
                        movie => movie.MovieId, 
                        movieKeyword => movieKeyword.MovieId,
                        (movie, movieKeyword) => new { movie, movieKeyword }
                    )
                    .Join(
                        _context.Keywords,
                        combined => combined.movieKeyword.KeywordId,
                        keyword => keyword.KeywordId,
                        (combined, keyword) => new { combined.movie, keyword }
                    )
                    .Where(result => result.movie.MovieId == movie_idToUpdate)
                    .Select(result => new KeywordsViewModel(){Keyword = result.keyword.KeywordName, Movie_id = movie_idToUpdate})
                    .ToList();

                
                return View("Keywords",query);
            }
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
