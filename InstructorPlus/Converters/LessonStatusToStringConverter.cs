using System;
using System.Globalization;
using Avalonia.Data.Converters;
using InstructorPlus.Models.Enums;

namespace InstructorPlus.Converters;

public class LessonStatusToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LessonStatus status)
        {
            return status switch
            {
                LessonStatus.scheduled => "Запланировано",
                LessonStatus.in_progress => "Проводится",
                LessonStatus.completed => "Завершено",
                LessonStatus.cancelled => "Отменено",
                _ => status.ToString()
            };
        }
        return "-";
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}