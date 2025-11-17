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
		var appDataDir = FileSystem.AppDataDirectory;
		if (!Directory.Exists(appDataDir))
		{
			Directory.CreateDirectory(appDataDir);
		}
		var dbPath = Path.Combine(appDataDir, "student-progress.db3");
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

		// Set ServiceHelper immediately so UI can access services
		ServiceHelper.Services = app.Services;

		// Initialize database asynchronously (don't block window creation)
		_ = Task.Run(async () =>
		{
			try
			{
				var db = app.Services.GetRequiredService<DatabaseService>();
				DatabaseService.Current = db;
				await db.InitializeAsync();
#if DEBUG
				System.Diagnostics.Debug.WriteLine("[MauiProgram] Database initialized successfully");
#endif
			}
			catch (Exception ex)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Initialization error: {ex}");
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Exception Type: {ex.GetType().Name}");
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Exception Message: {ex.Message}");
				if (ex.InnerException != null)
				{
					System.Diagnostics.Debug.WriteLine($"[MauiProgram] Inner Exception: {ex.InnerException.Message}");
				}
				System.Diagnostics.Debug.WriteLine($"[MauiProgram] Stack Trace: {ex.StackTrace}");
				// Don't break here - let window appear, error is logged
#endif
			}
		});

		return app;
	}
}

