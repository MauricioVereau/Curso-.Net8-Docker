using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEcommerce.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public string SKU { get; set; } = string.Empty; // PROD-001-BLK-M

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; } = null;

    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public required Category Category { get; set; } = null!;
}
