using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using StudentProgressTracker.Services;
using StudentProgressTracker.ViewModels;
using StudentProgressTracker.Helpers;
#if ANDROID || IOS
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
#endif
#if ANDROID
using Plugin.LocalNotification.AndroidOption;
#endif

namespace StudentProgressTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		
		// Suppress CA1416 warnings - these methods are supported on our target platforms
		#pragma warning disable CA1416
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		#pragma warning restore CA1416

#if ANDROID || IOS
		ConfigureLocalNotifications(builder);
#endif

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

		// Suppress CA1416 warnings - Build() and Services are supported on our target platforms
		#pragma warning disable CA1416
        var app = builder.Build();

		// Set ServiceHelper immediately so UI can access services
		ServiceHelper.Services = app.Services;
		#pragma warning restore CA1416

		// Initialize database - ensure it's ready before UI loads
		// Use a background task that completes quickly
		try
		{
			var db = app.Services.GetRequiredService<DatabaseService>();
			DatabaseService.Current = db;
			
			// Initialize on background thread to avoid blocking UI
			// This ensures database is ready before page loads data
			var initTask = Task.Run(async () =>
			{
				try
				{
					await db.InitializeAsync();
#if DEBUG
					System.Diagnostics.Debug.WriteLine("[MauiProgram] Database initialized successfully");
#endif
				}
				catch (Exception ex)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine($"[MauiProgram] Database initialization failed: {ex.Message}");
#endif
				}
			});
			
			// On Android, wait a short time for initialization to complete
			// This prevents race conditions but doesn't block indefinitely
#if ANDROID
			initTask.Wait(TimeSpan.FromSeconds(2));
#endif
		}
		catch (Exception ex)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[MauiProgram] Service setup error: {ex.Message}");
#endif
			// Continue anyway - database operations will handle the error
		}

		return app;
	}

#if ANDROID || IOS
	private static void ConfigureLocalNotifications(MauiAppBuilder builder)
	{
		builder.UseLocalNotification(config =>
		{
#if ANDROID
			config.AddAndroid(android =>
			{
				android.AddChannel(new NotificationChannelRequest
				{
					Id = NotificationService.NotificationChannelId,
					Name = "General Notifications",
					Description = "Course and assessment reminders",
					Importance = AndroidImportance.High
				});
			});
#endif
		});

		LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
	}
#endif
	
#if ANDROID || IOS
	private static void OnNotificationActionTapped(NotificationActionEventArgs e)
	{
#if DEBUG
		var tappedId = e.Request?.NotificationId ?? -1;
		System.Diagnostics.Debug.WriteLine($"[MauiProgram] Notification tapped: {tappedId}, ActionId: {e.ActionId}, Dismissed: {e.IsDismissed}");
#endif
	}
#endif
}
