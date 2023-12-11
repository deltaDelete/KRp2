using System;
using System.ComponentModel.DataAnnotations;

namespace KR2.Models;

public class Owner {
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Заполните это поле")]
    [Range(0, 9999_999999)]
    public long Passport { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    public DateTimeOffset BirthDate { get; set; } = DateTimeOffset.Now;
}