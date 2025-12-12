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
		
		// API Service
		builder.Services.AddSingleton<Services.ApiService>();
		builder.Services.AddSingleton<Services.GPAService>();
		builder.Services.AddSingleton<Services.SearchService>();
		builder.Services.AddSingleton<Services.FinancialService>();

		// ViewModels
		builder.Services.AddTransient<ViewModels.LoginViewModel>();
		builder.Services.AddTransient<ViewModels.RegisterViewModel>();
		builder.Services.AddSingleton<TermsViewModel>();
		builder.Services.AddSingleton<TermDetailViewModel>();
		builder.Services.AddSingleton<CourseListViewModel>();
		builder.Services.AddSingleton<CourseDetailViewModel>();
		builder.Services.AddSingleton<AssessmentViewModel>();
		builder.Services.AddSingleton<GPAViewModel>();
		builder.Services.AddSingleton<SearchViewModel>();
		builder.Services.AddSingleton<FinancialViewModel>();
		builder.Services.AddSingleton<IncomeViewModel>();
		builder.Services.AddSingleton<ExpenseViewModel>();
		builder.Services.AddSingleton<CategoryViewModel>();
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
			// Fire and forget - UI will handle loading state
			_ = Task.Run(async () =>
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
		try
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
			
#if DEBUG
			System.Diagnostics.Debug.WriteLine("[MauiProgram] Local notifications configured successfully");
			System.Diagnostics.Debug.WriteLine($"[MauiProgram] Notification channel ID: {NotificationService.NotificationChannelId}");
#endif
		}
		catch (Exception ex)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[MauiProgram] Error configuring local notifications: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[MauiProgram] Stack trace: {ex.StackTrace}");
#endif
			// Continue without notifications - app should still work
		}
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
