﻿using System;
using System.Collections.Generic;

namespace WebApplication1.Models.Movies;

public partial class Language
{
    public int LanguageId { get; set; }

    public string? LanguageCode { get; set; }

    public string? LanguageName { get; set; }
}
