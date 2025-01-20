using System;
using System.Collections.Generic;

namespace WebApplication1.Models.Movies;

public class MovieViewModel
{
    public List<MovieViewModelEntity> MovieModels;
    public int total_pages;
    public int current_page;
    
}