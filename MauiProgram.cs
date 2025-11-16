using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using StudentProgressTracker.Services;
using StudentProgressTracker.ViewModels;
using StudentProgressTracker.Helpers;
#if ANDROID || IOS
using Plugin.LocalNotification;
#endif

namespace StudentProgressTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
#if ANDROID || IOS
			.UseLocalNotification()
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Services (DI)
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "student-progress.db3");
		builder.Services.AddSingleton(new DatabaseService(dbPath));
		builder.Services.AddSingleton<NotificationService>();

		// ViewModels
		builder.Services.AddSingleton<TermsViewModel>();
		builder.Services.AddSingleton<TermDetailViewModel>();
		builder.Services.AddSingleton<CourseListViewModel>();
		builder.Services.AddSingleton<CourseDetailViewModel>();
		builder.Services.AddSingleton<AssessmentViewModel>();
        builder.Services.AddSingleton<IAlertService, AlertService>();

        var app = builder.Build();

		// Initialize
		try
		{
			var db = app.Services.GetRequiredService<DatabaseService>();
			DatabaseService.Current = db;
			db.InitializeAsync().GetAwaiter().GetResult();
			ServiceHelper.Services = app.Services;
		}
		catch (Exception ex)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[MauiProgram] Initialization error: {ex}");
			System.Diagnostics.Debugger.Break();
#endif
			// Log but don't crash - app can still start
		}

		return app;
	}
}

