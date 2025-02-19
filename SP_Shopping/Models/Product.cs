﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models;

[Index(nameof(Name),IsUnique = true)]
public class Product
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage="The name of the product can at most be 100 characters.")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    [MaxLength(1000, ErrorMessage = "The description can be at maximum 1000 characters long")]
    public string? Description { get; set; }
    [ForeignKey(nameof(ApplicationUser))]
    public string? SubmitterId { get; set; }
    public ApplicationUser? Submitter { get; set; }
    public List<CartItem> CartItem { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime InsertionDate { get; set; } 
    public DateTime? ModificationDate { get; set; }
}

