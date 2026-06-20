using System;

namespace InstructorPlus.Models;

public class Route
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BranchId { get; set; }
    
    public string? BranchName { get; set; }
}