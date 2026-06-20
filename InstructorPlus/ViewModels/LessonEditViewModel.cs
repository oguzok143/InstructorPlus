using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Models;
using InstructorPlus.Repositories;
using InstructorPlus.Services;
using InstructorPlus.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.ViewModels;

public partial class LessonEditViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LessonRepository _lessonRepository;
    private readonly StudentRepository _studentRepository;
    private readonly CarRepository _carRepository;
    private readonly RouteRepository _routeRepository;
    private readonly NavigationService _navigation;
    
    private readonly User _currentUser;

    [ObservableProperty] private Lesson _selectedLesson;
    [ObservableProperty] private List<Student> _students;
    [ObservableProperty] private Student _selectedStudent;
    [ObservableProperty] private List<Car> _cars;
    [ObservableProperty] private Car _selectedCar;
    [ObservableProperty] private List<Route> _routes;
    [ObservableProperty] private Route _selectedRoute;
    [ObservableProperty] private string _selectedBranch;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LessonDateTime))]
    private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LessonDateTime))]
    private TimeSpan _selectedTime = TimeSpan.Zero;
    
    public DateTime LessonDateTime =>  SelectedDate.Date + SelectedTime;
    
    [ObservableProperty] private int _selectedDuration;
    [ObservableProperty] private string _selectedMeetingPlace;
    
    public LessonEditViewModel(
        IServiceProvider serviceProvider,
        LessonRepository lessonRepository,
        StudentRepository studentRepository,
        CarRepository carRepository,
        RouteRepository routeRepository,
        NavigationService navigation,
        Lesson selectedLesson,
        User currentUser)
    {
        _serviceProvider = serviceProvider;
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
        _carRepository = carRepository;
        _routeRepository = routeRepository;
        
        _navigation = navigation;
        
        _selectedLesson = selectedLesson;
        _currentUser = currentUser;
        
        LoadData();
    }

    private void LoadData()
    {
        Cars = _carRepository.GetAll();
        Routes = _routeRepository.GetAll();
        SelectedBranch = _currentUser.BranchName;
        Students = _studentRepository.GetByBranch(_currentUser.BranchId);
    }

    [RelayCommand]
    public void CancelCommand()
    {
        _navigation.CloseWindow();
    }

    [RelayCommand]
    public async void SaveCommand()
    {
        if (LessonDateTime < DateTime.Now)
        {
            await _navigation.ShowError("Ошибка", "Неверно выбрано время!");
            return;
        }

        if (SelectedCar is null)
        {
            await _navigation.ShowError("Ошибка", "Не выбран автомобиль!");
            return;
        }

        var newLesson = new Lesson
        {
            StudentId = SelectedStudent?.Id,
            InstructorId = _currentUser.Id,
            CarId = SelectedCar.Id,
            RouteId = SelectedRoute?.Id,
            BranchId = _currentUser.BranchId,
            LessonDate = LessonDateTime,
            DurationMinutes = SelectedDuration,
            MeetingPlace = SelectedMeetingPlace,
        };
        
        _lessonRepository.Add(newLesson);
        _navigation.CloseWindow();
    }
}