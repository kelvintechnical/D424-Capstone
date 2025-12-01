#if ANDROID || IOS
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
#endif
#if ANDROID
using Android.App;
using AndroidX.Core.App;
#endif

namespace StudentProgressTracker.Services;

public class NotificationService
{
#if ANDROID || IOS
	internal const string NotificationChannelId = "general";
	private const int TestNotificationId = 9_999_999;
	private static readonly NotificationPermission PermissionRequest = new();
#if ANDROID
	private const string NotificationIconName = "notification_icon";
	private static AndroidIcon? CreateSmallIcon()
	{
		try
		{
			// Try to use the custom notification icon
			var icon = new AndroidIcon(NotificationIconName);
			return icon;
		}
		catch
		{
			// Fallback: Use the app icon if custom icon fails
			// Android will use the app icon by default if no icon is specified
			return null;
		}
	}

	static NotificationService()
	{
		PermissionRequest.Android.RequestPermissionToScheduleExactAlarm = true;
	}

	/// <summary>
	/// Check if notifications are enabled using Android's native NotificationManager (fallback method)
	/// </summary>
	public static bool AreNotificationsEnabledNative()
	{
#if ANDROID
		try
		{
			var context = Android.App.Application.Context;
			if (context == null)
			{
				System.Diagnostics.Debug.WriteLine("[NotificationService] Application.Context is null");
				return false;
			}
			
			var notificationManager = NotificationManager.FromContext(context);
			if (notificationManager == null)
			{
				System.Diagnostics.Debug.WriteLine("[NotificationService] NotificationManager is null");
				return false;
			}
			
			return notificationManager.AreNotificationsEnabled();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Error checking native notification status: {ex.Message}");
			return false;
		}
#else
		return false;
#endif
	}
#endif
#endif

	public async Task ScheduleCourseNotificationsAsync(int courseId, string courseTitle, DateTime startUtc, DateTime endUtc, bool enabled)
	{
#if ANDROID || IOS
		var startId = GetNotificationId(courseId, "course_start");
		var endId = GetNotificationId(courseId, "course_end");
		LocalNotificationCenter.Current.Cancel(startId);
		LocalNotificationCenter.Current.Cancel(endId);
		if (!enabled) return;
		await ScheduleAsync(startId, $"Course Starts: {courseTitle}", "Starts today", ToLocal(startUtc));
		await ScheduleAsync(endId, $"Course Ends: {courseTitle}", "Ends today", ToLocal(endUtc));
#else
		// Notifications not supported on Windows
		await Task.CompletedTask;
#endif
	}

	public async Task ScheduleAssessmentNotificationsAsync(int assessmentId, string assessmentName, DateTime startUtc, DateTime dueUtc, bool enabled)
	{
#if ANDROID || IOS
		var startId = GetNotificationId(assessmentId, "assessment_start");
		var dueId = GetNotificationId(assessmentId, "assessment_due");
		LocalNotificationCenter.Current.Cancel(startId);
		LocalNotificationCenter.Current.Cancel(dueId);
		if (!enabled) return;
		await ScheduleAsync(startId, $"Assessment Starts: {assessmentName}", "Starts today", ToLocal(startUtc));
		await ScheduleAsync(dueId, $"Assessment Due: {assessmentName}", "Due today", ToLocal(dueUtc));
#else
		// Notifications not supported on Windows
		await Task.CompletedTask;
#endif
	}

	// IMMEDIATE NOTIFICATION METHODS
	public async Task SendImmediateCourseNotificationsAsync(int courseId, string courseTitle, bool enabled)
	{
#if ANDROID || IOS
		var startId = GetNotificationId(courseId, "course_start");
		var endId = GetNotificationId(courseId, "course_end");
		LocalNotificationCenter.Current.Cancel(startId);
		LocalNotificationCenter.Current.Cancel(endId);
		if (!enabled) return;
		await ScheduleImmediateAsync(startId, $"Course Starts: {courseTitle}", "Starts today");
		await ScheduleImmediateAsync(endId, $"Course Ends: {courseTitle}", "Ends today");
#else
		// Notifications not supported on Windows
		await Task.CompletedTask;
#endif
	}

	public async Task SendImmediateAssessmentNotificationsAsync(int assessmentId, string assessmentName, bool enabled)
	{
#if ANDROID || IOS
		var startId = GetNotificationId(assessmentId, "assessment_start");
		var dueId = GetNotificationId(assessmentId, "assessment_due");
		LocalNotificationCenter.Current.Cancel(startId);
		LocalNotificationCenter.Current.Cancel(dueId);
		if (!enabled) return;
		await ScheduleImmediateAsync(startId, $"Assessment Starts: {assessmentName}", "Starts today");
		await ScheduleImmediateAsync(dueId, $"Assessment Due: {assessmentName}", "Due today");
#else
		// Notifications not supported on Windows
		await Task.CompletedTask;
#endif
	}

	public async Task<bool> SendTestNotificationAsync(string? title = null, string? message = null)
	{
#if ANDROID || IOS
		// Step 5: Fallback notification detection using Android's native API
#if ANDROID
		var nativeNotificationsEnabled = AreNotificationsEnabledNative();
		if (!nativeNotificationsEnabled)
		{
			System.Diagnostics.Debug.WriteLine("[NotificationService] Notifications are disabled in system settings.");
			// Return false - user needs to enable in settings
			return false;
		}
#endif

		var notificationsEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
		if (!notificationsEnabled)
		{
			notificationsEnabled = await LocalNotificationCenter.Current.RequestNotificationPermission(PermissionRequest);
		}

		if (!notificationsEnabled)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine("[NotificationService] Notifications disabled or permission denied.");
#endif
			return false;
		}

		var request = new NotificationRequest
		{
			NotificationId = TestNotificationId,
			Title = title ?? "Student Progress Tracker",
			Description = message ?? "If you see this, local notifications are configured correctly.",
			Android = new AndroidOptions
			{
				ChannelId = NotificationChannelId,
				LaunchAppWhenTapped = true
#if ANDROID
				,
				IconSmallName = CreateSmallIcon() ?? new AndroidIcon("@mipmap/icon") // Fallback to app icon
#endif
			}
		};

#if DEBUG
		System.Diagnostics.Debug.WriteLine("[NotificationService] Sending test notification.");
#endif
		LocalNotificationCenter.Current.Cancel(TestNotificationId);
		await LocalNotificationCenter.Current.Show(request);
		return true;
#else
		await Task.CompletedTask;
		return false;
#endif
	}

#if ANDROID || IOS
	private static int GetNotificationId(int entityId, string suffix) => (entityId.ToString() + "_" + suffix).GetHashCode();
	private static async Task ScheduleAsync(int id, string title, string body, DateTime localTime)
	{
		// Check and request notification permission before scheduling (per instructor guide)
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Checking notification permission before scheduling {id}");
#endif
		if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notifications not enabled, requesting permission...");
#endif
			var permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
			if (!permissionGranted)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[NotificationService] Cannot schedule notification {id}: Notification permission denied");
#endif
				return;
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notification permission granted");
#endif
		}
#if DEBUG
		else
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notifications already enabled");
		}
#endif

#if ANDROID
		// Check native notification status before scheduling (fallback check)
		if (!AreNotificationsEnabledNative())
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Cannot schedule notification {id}: Notifications disabled in system settings");
			return;
		}
#endif
		var normalizedLocal = EnsureLocal(localTime);
		var request = new NotificationRequest
		{
			NotificationId = id,
			Title = title,
			Description = body,
			Android = new AndroidOptions
			{
				ChannelId = NotificationChannelId
#if ANDROID
				,
				IconSmallName = CreateSmallIcon() ?? new AndroidIcon("@mipmap/icon") // Fallback to app icon
#endif
			},
			Schedule = new NotificationRequestSchedule
			{
				NotifyTime = normalizedLocal,
				NotifyRepeatInterval = null
			}
		};
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Scheduling notification {id}: {title} at {localTime} (UTC: {localTime.ToUniversalTime()})");
#endif
		try
		{
			// Cancel previous if re-scheduling
			LocalNotificationCenter.Current.Cancel(id);
			await LocalNotificationCenter.Current.Show(request);
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Successfully scheduled notification {id}");
#endif
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Error scheduling notification {id}: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Stack trace: {ex.StackTrace}");
		}
	}

	private static async Task ScheduleImmediateAsync(int id, string title, string body)
	{
		// Check and request notification permission before sending (per instructor guide)
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Checking notification permission before sending immediate {id}");
#endif
		if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notifications not enabled, requesting permission...");
#endif
			var permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
			if (!permissionGranted)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[NotificationService] Cannot send immediate notification {id}: Notification permission denied");
#endif
				return;
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notification permission granted");
#endif
		}
#if DEBUG
		else
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Notifications already enabled");
		}
#endif

#if ANDROID
		// Check native notification status before sending (fallback check)
		if (!AreNotificationsEnabledNative())
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Cannot send immediate notification {id}: Notifications disabled in system settings");
			return;
		}
#endif
		var request = new NotificationRequest
		{
			NotificationId = id,
			Title = title,
			Description = body,
			Android = new AndroidOptions
			{
				ChannelId = NotificationChannelId
#if ANDROID
				,
				IconSmallName = CreateSmallIcon() ?? new AndroidIcon("@mipmap/icon") // Fallback to app icon
#endif
			}
		};
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Sending immediate notification {id}: {title} - {body}");
#endif
		try
		{
			// Cancel previous
			LocalNotificationCenter.Current.Cancel(id);
			// Use Show() for immediate notifications (no schedule)
			await LocalNotificationCenter.Current.Show(request);
#if DEBUG
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Successfully sent immediate notification {id}");
#endif
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Error sending immediate notification {id}: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[NotificationService] Stack trace: {ex.StackTrace}");
		}
	}
	private static DateTime ToLocal(DateTime dateTime)
	{
		return dateTime.Kind switch
		{
			DateTimeKind.Local => dateTime,
			DateTimeKind.Utc => dateTime.ToLocalTime(),
			_ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime()
		};
	}

	private static DateTime EnsureLocal(DateTime dateTime)
	{
		return dateTime.Kind switch
		{
			DateTimeKind.Local => dateTime,
			DateTimeKind.Utc => dateTime.ToLocalTime(),
			_ => DateTime.SpecifyKind(dateTime, DateTimeKind.Local)
		};
	}
#endif
}



