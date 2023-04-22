﻿using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Movie : Product
{
    [MinLength(10)]
    public string? Trailer { get; set; }
}
