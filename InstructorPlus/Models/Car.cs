using System;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Models;

public class Car
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string PlateNumber { get; set; }
    public Transmission Transmission { get; set; }
    public int BranchId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CommissioningDate { get; set; }
    public string? BranchName { get; set; }
}