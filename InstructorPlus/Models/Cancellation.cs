using System;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Models;

public class Cancellation
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public CancelledBy CancelledBy { get; set; }
    public string Reason { get; set; }
    public DateTime CancelledAt { get; set; }
}