using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KR2.Models; 

public class Consultation {
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = String.Empty;
    
    [Precision(30, 2)]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }   
}