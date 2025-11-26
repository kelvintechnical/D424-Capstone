#if ANDROID || IOS
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
#endif

namespace StudentProgressTracker.Services;

public class NotificationService
{
#if ANDROID || IOS
	private const string NotificationChannelId = "general";
	private const int TestNotificationId = 9_999_999;
#endif

	public async Task ScheduleCourseNotificationsAsync(int courseId, string courseTitle, DateTime startUtc, DateTime endUtc, bool enabled)
	{
#if ANDROID || IOS
		var startId = GetNotificationId(courseId, "course_start");
		var endId = GetNotificationId(courseId, "course_end");
		LocalNotificationCenter.Current.Cancel(startId);
		LocalNotificationCenter.Current.Cancel(endId);
		if (!enabled) return;
		await ScheduleAsync(startId, $"Course Starts: {courseTitle}", "Starts today", ToNineAmLocal(startUtc));
		await ScheduleAsync(endId, $"Course Ends: {courseTitle}", "Ends today", ToNineAmLocal(endUtc));
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
		await ScheduleAsync(startId, $"Assessment Starts: {assessmentName}", "Starts today", ToNineAmLocal(startUtc));
		await ScheduleAsync(dueId, $"Assessment Due: {assessmentName}", "Due today", ToNineAmLocal(dueUtc));
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
			notificationsEnabled = await LocalNotificationCenter.Current.RequestNotificationPermission();
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
			}
		};

#if DEBUG
		System.Diagnostics.Debug.WriteLine("[NotificationService] Sending test notification.");
#endif
		await LocalNotificationCenter.Current.Cancel(TestNotificationId);
		await LocalNotificationCenter.Current.Show(request);
		return true;
#else
		await Task.CompletedTask;
		return false;
#endif
	}

#if ANDROID || IOS
	private static int GetNotificationId(int entityId, string suffix) => (entityId.ToString() + "_" + suffix).GetHashCode();
	private static DateTime ToNineAmLocal(DateTime utc)
	{
		var localDate = utc.ToLocalTime().Date;
		return new DateTime(localDate.Year, localDate.Month, localDate.Day, 9, 0, 0, DateTimeKind.Local);
	}

	private static async Task ScheduleAsync(int id, string title, string body, DateTime localTime)
	{
		var request = new NotificationRequest
		{
			NotificationId = id,
			Title = title,
			Description = body,
			Android = new AndroidOptions
			{
				ChannelId = NotificationChannelId
			},
			Schedule = new NotificationRequestSchedule
			{
				NotifyTime = localTime.ToUniversalTime(),
				NotifyRepeatInterval = null
			}
		};
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Scheduling notification {id}: {title} at {localTime} (UTC: {localTime.ToUniversalTime()})");
#endif
		// Cancel previous if re-scheduling
		await LocalNotificationCenter.Current.Cancel(id);
		// MUST use Schedule(), not Show()
		await LocalNotificationCenter.Current.Schedule(request);
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
			}
		};
#if DEBUG
		System.Diagnostics.Debug.WriteLine($"[NotificationService] Sending immediate notification {id}: {title} - {body}");
#endif
		// Cancel previous
		await LocalNotificationCenter.Current.Cancel(id);
		// Use Show() for immediate notifications (no schedule)
		await LocalNotificationCenter.Current.Show(request);
	}
#endif
}



