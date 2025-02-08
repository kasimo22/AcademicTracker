using System.Text.RegularExpressions;
using System.Net.Mail;
using Plugin.LocalNotification;

namespace AcademicTracker;

[QueryProperty(nameof(TermId), "TermId")]
public partial class AddCoursePage : ContentPage
{
    private int _termId;

    public int TermId
    {
        get => _termId;
        set => _termId = value;
    }

    public AddCoursePage()
    {
        InitializeComponent();
    }

    private async void OnSaveCourseClicked(object sender, EventArgs e)
    {
        if (!IsCourseInputValid())
            return;

        var newCourse = new Course
        {
            Title = TitleEntry.Text,
            StartDate = StartDatePicker.Date,
            EndDate = EndDatePicker.Date,
            Status = StatusPicker.SelectedItem?.ToString() ?? "Planned",
            InstructorName = InstructorNameEntry.Text,
            InstructorPhone = InstructorPhoneEntry.Text,
            InstructorEmail = InstructorEmailEntry.Text,
            Notes = NotesEditor.Text,
            TermId = _termId,
            AlertStart = AlertStartSwitch.IsToggled,
            AlertEnd = AlertEndSwitch.IsToggled
        };

        try
        {
            // Save the course to the database
            await App.Database.SaveCourseAsync(newCourse);

            // Schedule notifications if alerts are enabled
            if (newCourse.AlertStart || newCourse.AlertEnd)
            {
                ScheduleNotifications(newCourse);
            }

            await DisplayAlert("Success", "Course added successfully.", "OK");
            await Shell.Current.GoToAsync($"..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save course: {ex.Message}", "OK");
        }
    }

    private bool IsCourseInputValid()
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            DisplayAlert("Validation Error", "Course title is required.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(InstructorNameEntry.Text))
        {
            DisplayAlert("Validation Error", "Instructor name cannot be empty.", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(InstructorPhoneEntry.Text) || !IsValidPhoneNumber(InstructorPhoneEntry.Text))
        {
            DisplayAlert("Validation Error", "Please enter a valid phone number (e.g., 555-123-4567).", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(InstructorEmailEntry.Text) || !IsValidEmail(InstructorEmailEntry.Text))
        {
            DisplayAlert("Validation Error", "Please enter a valid email address.", "OK");
            return false;
        }

        if (StartDatePicker.Date > EndDatePicker.Date)
        {
            DisplayAlert("Validation Error", "Start date cannot be after the end date.", "OK");
            return false;
        }

        return true;
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        string pattern = @"^\d{3}-\d{3}-\d{4}$";  // Format: 555-123-4567
        return Regex.IsMatch(phoneNumber, pattern);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ScheduleNotifications(Course course)
    {
        LocalNotificationCenter.Current.Cancel(course.Id * 10);
        LocalNotificationCenter.Current.Cancel(course.Id * 10 + 1);

        if (course.AlertStart)
        {
            var notifyTime = course.StartDate.Date == DateTime.Now.Date
                ? DateTime.Now.AddSeconds(5)
                : course.StartDate.Date.AddHours(12);

            var startNotification = new NotificationRequest
            {
                NotificationId = course.Id * 10,
                Title = "Course Start Reminder",
                Description = $"Your course '{course.Title}' starts today!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };

            LocalNotificationCenter.Current.Show(startNotification);
        }

        if (course.AlertEnd)
        {
            var notifyTime = course.EndDate.Date == DateTime.Now.Date
                ? DateTime.Now.AddSeconds(5)
                : course.EndDate.Date.AddHours(12);

            var endNotification = new NotificationRequest
            {
                NotificationId = course.Id * 10 + 1,
                Title = "Course End Reminder",
                Description = $"Your course '{course.Title}' ends today!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };

            LocalNotificationCenter.Current.Show(endNotification);
        }
    }


    private async void OnCancelCourseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");  // Navigate back without saving
    }
}