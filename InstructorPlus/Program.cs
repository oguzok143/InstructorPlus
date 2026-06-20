using Avalonia;
using System;
using InstroctorPlus.Views;
// using InstroctorPlus.Views;
using InstructorPlus.Repositories;
using InstructorPlus.Services;
using InstructorPlus.ViewModels;
using InstructorPlus.Views;
using InstructorPlus.ViewModels;
using InstructorPlus.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InstructorPlus;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder().
            ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            }).
            ConfigureServices((c,s) =>
            {
                s.Configure<DatabaseSettings>(c.Configuration.
                    GetSection("DatabaseSettings"));
                
                s.AddSingleton<AdminRepository>();
                s.AddSingleton<BranchRepository>();
                s.AddSingleton<CarRepository>();
                s.AddSingleton<InstructorRepository>();
                s.AddSingleton<StudentRepository>();
                s.AddSingleton<RouteRepository>();
                s.AddSingleton<LessonRepository>();
                s.AddSingleton<DocumentRepository>();
                s.AddSingleton<ExamRepository>();
                s.AddSingleton<CancellationRepository>();
                
                s.AddSingleton<StatsService>();
                s.AddSingleton<NavigationService>();
                
                s.AddTransient<MainWindow>();
                s.AddTransient<MainWindowViewModel>();
                s.AddTransient<AutorizationWindow>();
                s.AddTransient<AutorizationWindowViewModel>();
                s.AddTransient<LessonEditWindow>();
                s.AddTransient<LessonEditViewModel>();
                s.AddTransient<StudentEditWindow>();
                s.AddTransient<StudentEditViewModel>();
                s.AddTransient<StudentStatisticWindow>();
                s.AddTransient<StudentStatisticViewModel>();
                s.AddTransient<BranchEditWindow>();
                s.AddTransient<BranchEditViewModel>();
                s.AddTransient<InstructorEditWindow>();
                s.AddTransient<InstructorEditViewModel>();
            }).
            Build();
        BuildAvaloniaApp(host.Services)
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(()=> new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}