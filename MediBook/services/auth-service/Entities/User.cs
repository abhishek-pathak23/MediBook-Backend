using System.ComponentModel.DataAnnotations;

namespace auth_service.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; set; }

    public string? PasswordHash { get; set; }

    [Phone]
    [MaxLength(30)]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Patient";

    [MaxLength(50)]
    public string? Provider { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ProfilePicUrl { get; set; }
}
