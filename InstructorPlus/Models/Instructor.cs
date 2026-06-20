using System;

namespace InstructorPlus.Models;

public class Instructor: User
{
    public long? TelegramId { get; set; }
    public int? CarId { get; set; }
    public string? CarModel { get; set; }
    public string? CarPlate { get; set; }
}