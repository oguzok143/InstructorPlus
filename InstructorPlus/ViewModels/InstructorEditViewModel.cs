using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Models;
using InstructorPlus.Repositories;
using InstructorPlus.Services;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.ViewModels;

public partial class InstructorEditViewModel : ViewModelBase
{
     private readonly IServiceProvider _serviceProvider;
     private readonly NavigationService _navigation;
     private readonly BranchRepository _branchRepository;
     private readonly InstructorRepository _instructorRepository;
     private readonly CarRepository _carRepository;

     [ObservableProperty] private Instructor _instructor;
     [ObservableProperty] private ObservableCollection<Branch> _branches;
     [ObservableProperty] private List<Car> _cars;
     [ObservableProperty] private string _selectedLogin;
     [ObservableProperty] private string _selectedPassword;
     [ObservableProperty] private string _selectedName;
     [ObservableProperty] private string _selectedPhone;
     [ObservableProperty] private Int64? _selectedTelegramId;
     [ObservableProperty] private Branch _selectedBranch;
     [ObservableProperty] private Car _selectedCar;
     // [ObservableProperty] private DateTime _selectedHireDate;
     [ObservableProperty] private bool _isOldInstructor;
     
     [ObservableProperty]
     [NotifyPropertyChangedFor(nameof(InstructorDate))]
     private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    
     public DateTime InstructorDate =>  SelectedDate.Date;

     public InstructorEditViewModel(
         IServiceProvider serviceProvider,
         NavigationService navigation,
         BranchRepository branchRepository,
         InstructorRepository instructorRepository,
         CarRepository carRepository,
         Instructor instructor)
     {
         _serviceProvider = serviceProvider;
         _navigation = navigation;
         _branchRepository = branchRepository;
         _instructorRepository = instructorRepository;
         _carRepository = carRepository;
         Instructor = instructor;

         SelectedLogin = instructor.Login;
         SelectedPassword = instructor.Password;
         SelectedName = instructor.FullName;
         SelectedPhone = instructor.Phone;
         SelectedTelegramId = instructor.TelegramId;
         SelectedBranch = _branchRepository.GetById(instructor.BranchId);
         SelectedCar = _carRepository.GetById(instructor.CarId);
         SelectedDate = instructor.HireDate.Value;
         
         if (Instructor.Id == 0)
             IsOldInstructor = false;
         else
             IsOldInstructor = true;
         
         LoadData();
     }

     private void LoadData()
     {
         Branches = _branchRepository.GetAll();
         Cars = _carRepository.GetAll();
     }

     [RelayCommand]
     public void CancelCommand()
     {
         _navigation.CloseWindow();
     }

     [RelayCommand]
     public async void DeleteCommand()
     {
         var result = await _navigation.ShowConfirm("Подтверждение", "Вы точно хотите удалить инструктора?");
         
         if (result == ButtonResult.Yes)
         {
             _instructorRepository.Delete(Instructor.Id);
             _navigation.CloseWindow();
         }
     }

     [RelayCommand]
     public async void SaveCommand()
     {
         if (SelectedName is null)
         {
             await _navigation.ShowError("Ошибка", "Не указано имя!");
             return;
         }
         if (SelectedLogin == "")
         {
             await _navigation.ShowError("Ошибка", "Не указан логин!");
             return;
         }
         if (SelectedPassword == "")
         {
             await _navigation.ShowError("Ошибка", "Не указан пароль!");
             return;
         }
         if (SelectedBranch is null)
         {
             await _navigation.ShowError("Ошибка", "Не выбран филиал!");
             return;
         }
         if (SelectedCar is null)
         {
             await _navigation.ShowError("Ошибка", "Не выбран автомобиль!");
             return;
         }

         var newInstructor = new Instructor
         {
             Id = Instructor.Id,
             Login = SelectedLogin,
             Password = SelectedPassword,
             FullName = SelectedName,
             Phone = SelectedPhone,
             TelegramId = SelectedTelegramId,
             BranchId = SelectedBranch.Id,
             CarId = SelectedCar.Id,
             HireDate = InstructorDate,
         };
         
         if(Instructor.Id == 0)
             _instructorRepository.Add(newInstructor);
         else
             _instructorRepository.Update(newInstructor);
         _navigation.CloseWindow();
     }

}