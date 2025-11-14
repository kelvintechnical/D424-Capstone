#if ANDROID || IOS
using Plugin.LocalNotification;
#endif

namespace StudentProgressTracker.Services;

public class NotificationService
{
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
			Schedule = new NotificationRequestSchedule { NotifyTime = localTime }
		};
		await LocalNotificationCenter.Current.Show(request);
	}
#endif
}



