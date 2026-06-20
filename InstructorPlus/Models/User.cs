using System;

namespace InstructorPlus.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public string? Phone { get; set; }
    public int BranchId { get; set; }
    public DateTime? HireDate { get; set; }
    public string? BranchName { get; set; }
}