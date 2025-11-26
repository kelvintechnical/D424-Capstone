#if ANDROID || IOS
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
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
	private static AndroidIcon CreateSmallIcon() => new(NotificationIconName);

	static NotificationService()
	{
		PermissionRequest.Android.RequestPermissionToScheduleExactAlarm = true;
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
				IconSmallName = CreateSmallIcon()
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
				IconSmallName = CreateSmallIcon()
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
		// Cancel previous if re-scheduling
		LocalNotificationCenter.Current.Cancel(id);
		await LocalNotificationCenter.Current.Show(request);
	}

	private static async Task ScheduleImmediateAsync(int id, string title, string body)
	{
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
				IconSmallName = CreateSmallIcon()
#endif
			}
		};
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Sending immediate notification {id}: {title} - {body}");
#endif
		// Cancel previous
		LocalNotificationCenter.Current.Cancel(id);
		// Use Show() for immediate notifications (no schedule)
		await LocalNotificationCenter.Current.Show(request);
	}
#endif
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



