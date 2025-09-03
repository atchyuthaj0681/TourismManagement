using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "Full Name")]
    public string Name { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    public override string Email { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public override string PhoneNumber { get; set; }

    [Display(Name = "Nationality")]
    public string Nationality { get; set; }

    [Display(Name = "Passport Number or ID")]
    public string? PassportNumber { get; set; }

    [Display(Name = "Preferred Language")]
    public string PreferredLanguage { get; set; }

    [Display(Name = "Gender")]
    public string? Gender { get; set; } // Could be "Male", "Female", "Other", etc.
}
