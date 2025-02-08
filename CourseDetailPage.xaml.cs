using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace AcademicTracker;

[QueryProperty(nameof(CourseId), "CourseId")]
public partial class CourseDetailPage : ContentPage
{
    private int _courseId;
    private Course _course = new Course();

    private readonly List<string> _statusOptions = new List<string>
    {
        "In Progress",
        "Completed",
        "Planned",
        "Dropped"
    };

    public int CourseId
    {
        get => _courseId;
        set
        {
            _courseId = value;
            LoadCourse(_courseId);
        }
    }

    public CourseDetailPage()
    {
        InitializeComponent();
        StatusPicker.ItemsSource = _statusOptions; // Bind status options to Picker
    }

    private async void LoadCourse(int courseId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading course with ID: {courseId}");
            _course = await App.Database.GetCourseAsync(courseId);

            if (_course == null)
            {
                System.Diagnostics.Debug.WriteLine($"No course found with ID: {courseId}");
                await DisplayAlert("Error", "Course not found.", "OK");
                await Shell.Current.GoToAsync(".."); // Navigate back if course is not found
                return;
            }

            BindingContext = _course;
            System.Diagnostics.Debug.WriteLine($"Course loaded: {_course.Title}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load course: {ex.Message}");
            await DisplayAlert("Error", "Failed to load course details.", "OK");
        }
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        ViewLayout.IsVisible = false;
        EditLayout.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Revert to view mode without saving changes
        ViewLayout.IsVisible = true;
        EditLayout.IsVisible = false;

        // Reload course to discard unsaved changes
        BindingContext = null;
        LoadCourse(_courseId);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!IsCourseInputValid())
            return;

        try
        {
            // Save the course to the database
            await App.Database.SaveCourseAsync(_course);

            // Schedule notifications
            ScheduleNotifications(_course);

            // Refresh the course details on the page
            BindingContext = null; // Clear the old binding
            BindingContext = _course; // Re-bind with the updated course

            // Switch back to view mode
            ViewLayout.IsVisible = true;
            EditLayout.IsVisible = false;

            await DisplayAlert("Success", "Course saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save course: {ex.Message}");
            await DisplayAlert("Error", "Failed to save course.", "OK");
        }
    }

    private bool IsCourseInputValid()
    {
        // Validate Instructor Name
        if (string.IsNullOrWhiteSpace(_course.InstructorName))
        {
            DisplayAlert("Validation Error", "Instructor name cannot be empty.", "OK");
            return false;
        }

        // Validate Phone Number (basic format check)
        if (string.IsNullOrWhiteSpace(_course.InstructorPhone) || !IsValidPhoneNumber(_course.InstructorPhone))
        {
            DisplayAlert("Validation Error", "Please enter a valid phone number (e.g., 555-123-4567).", "OK");
            return false;
        }

        // Validate Email Address
        if (string.IsNullOrWhiteSpace(_course.InstructorEmail) || !IsValidEmail(_course.InstructorEmail))
        {
            DisplayAlert("Validation Error", "Please enter a valid email address.", "OK");
            return false;
        }

        return true;
    }

    // Validate Phone Number (Basic Format: 555-123-4567)
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        string pattern = @"^\d{3}-\d{3}-\d{4}$";  // Format: 555-123-4567
        return Regex.IsMatch(phoneNumber, pattern);
    }

    // Validate Email Address
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

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Deleting course with ID: {_course.Id}");
            await App.Database.DeleteCourseAsync(_course);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to delete course: {ex.Message}");
            await DisplayAlert("Error", "Failed to delete course.", "OK");
        }
    }

    private async void OnViewAssessmentsClicked(object sender, EventArgs e)
    {
        if (_course != null)
        {
            await Shell.Current.GoToAsync($"{nameof(AssessmentListPage)}?CourseId={_course.Id}");
        }
    }

    private void ScheduleNotifications(Course course)
    {
        // Cancel any existing notifications for this course
        LocalNotificationCenter.Current.Cancel(course.Id * 10); // Start notification
        LocalNotificationCenter.Current.Cancel(course.Id * 10 + 1); // End notification

        // Schedule start date notification
        if (course.AlertStart)
        {
            var notifyTime = course.StartDate.Date == DateTime.Now.Date
                ? DateTime.Now.AddSeconds(5) // Notify in 5 seconds if today
                : course.StartDate.Date.AddHours(12); // Default time: 12:00 PM

            var startNotification = new NotificationRequest
            {
                NotificationId = course.Id * 10,
                Title = "Course Start Reminder",
                Description = $"Your course '{course.Title}' starts today!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                },
                ReturningData = $"{nameof(CourseDetailPage)}?CourseId={course.Id}"
            };

            LocalNotificationCenter.Current.Show(startNotification);
        }

        // Schedule end date notification
        if (course.AlertEnd)
        {
            var notifyTime = course.EndDate.Date == DateTime.Now.Date
                ? DateTime.Now.AddSeconds(5) // Notify in 5 seconds if today
                : course.EndDate.Date.AddHours(12); // Default time: 12:00 PM

            var endNotification = new NotificationRequest
            {
                NotificationId = course.Id * 10 + 1,
                Title = "Course End Reminder",
                Description = $"Your course '{course.Title}' ends today!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                },
                ReturningData = $"{nameof(CourseDetailPage)}?CourseId={course.Id}"
            };

            LocalNotificationCenter.Current.Show(endNotification);
        }
    }

    private async void GoHomeClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to navigate to Home: {ex.Message}");
        }
    }

    private async void OnShareNotesClicked(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_course.Notes))
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = _course.Notes,
                    Title = $"Notes for {_course.Title}"
                });
            }
            else
            {
                await DisplayAlert("No Notes", "There are no notes to share for this course.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to share notes: {ex.Message}");
        }
    }
}