using System;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Models;

public class Document
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public DocType DocType { get; set; }
    public string FilePath { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsValid { get; set; }
    
    public string? StudentName { get; set; }
}
