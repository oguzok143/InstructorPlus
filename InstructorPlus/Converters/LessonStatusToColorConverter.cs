using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using InstructorPlus.Models.Enums;


namespace InstructorPlus.Converters;

public class LessonStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LessonStatus status)
        {
            return status switch
            {
                LessonStatus.scheduled => new SolidColorBrush(Color.Parse("#3B82F6")), // синий
                LessonStatus.in_progress => new SolidColorBrush(Color.Parse("#F59E0B")), // оранжевый
                LessonStatus.completed => new SolidColorBrush(Color.Parse("#10B981")), // зелёный
                LessonStatus.cancelled => new SolidColorBrush(Color.Parse("#EF4444")), // красный
                _ => new SolidColorBrush(Color.Parse("#6B7280"))  // серый
            };
        }
        return new SolidColorBrush(Color.Parse("#6B7280"));
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}