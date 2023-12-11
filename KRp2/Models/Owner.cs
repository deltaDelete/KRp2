using System;
using System.ComponentModel.DataAnnotations;

namespace KR2.Models;

public class Owner {
    [Key] public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public int Passport { get; set; }

    public DateTimeOffset BirthDate { get; set; } = DateTimeOffset.Now;
}