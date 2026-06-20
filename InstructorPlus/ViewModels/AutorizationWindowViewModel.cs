using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstructorPlus.Repositories;
using InstructorPlus.ViewModels;
using InstructorPlus.Models;
using InstructorPlus.Services;
using InstructorPlus.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.ViewModels;

public partial class AutorizationWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AdminRepository _adminRepository;
    private readonly InstructorRepository _instructorRepository;
    
    private readonly NavigationService _navigation;

    [ObservableProperty] private string _login;
    [ObservableProperty] private string _password;
    [ObservableProperty] private Instructor? _selectedInstructor;
    [ObservableProperty] private Admin? _selectedAdmin;

    public AutorizationWindowViewModel(IServiceProvider serviceProvider,
        AdminRepository adminRepository,
        InstructorRepository instructorRepository,
        NavigationService navigation)
    {
        _serviceProvider = serviceProvider;
        _adminRepository = adminRepository;
        _instructorRepository = instructorRepository;
        
        _navigation = navigation;
    }
    
    [RelayCommand]
    public async void Enter()
    {
        if (await AutorizateAsync(Login, Password))
        {
            SelectedInstructor = _instructorRepository.GetByLogin(Login);
            SelectedAdmin = _adminRepository.GetByLogin(Login);
            if (SelectedInstructor != null)
            {
                OpenMainWindowCommand(SelectedInstructor);
            }
            else if (SelectedAdmin != null)
            {
                OpenMainWindowCommand(SelectedAdmin);
            }
            else
            {
                await _navigation.ShowError("Ошибка", "Инструктор не найден!");
            }
        }
        else
        {
            await _navigation.ShowError("Ошибка", "Неправильно введён логин или пароль");
        }
    }
    
    private async Task<bool> AutorizateAsync(string login, string password)
    {
        try
        {
            // Проверяем админа
            var admin = _adminRepository.GetByLogin(login);
            if (admin != null && admin.Password == password)
                return true;
            
            var instructor = _instructorRepository.GetByLogin(login);
            if (instructor != null && instructor.Password == password)
                return true;

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth error: {ex.Message}");
            
            await _navigation.ShowError("Ошибка БД", $"Не удалось подключиться к базе данных!\n{ex.Message}");
        
            return false;
        }
    }

    private void OpenMainWindowCommand(object User)
    {
        var vm = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, User);
        var win = _serviceProvider.GetRequiredService<MainWindow>();
        win.DataContext = vm;
        win.Show();
        _navigation.GetWindow().Close();
    }
}