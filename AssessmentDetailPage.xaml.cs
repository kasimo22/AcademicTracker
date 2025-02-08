using Plugin.LocalNotification;

namespace AcademicTracker;

[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(AssessmentId), "AssessmentId")]
public partial class AssessmentDetailPage : ContentPage
{
    private int _courseId;
    private int _assessmentId;
    private Assessment _assessment = new Assessment();

    public int CourseId
    {
        get => _courseId;
        set
        {
            _courseId = value;
            _assessment.CourseId = _courseId;  // Assign CourseId to the assessment
        }
    }

    public int AssessmentId
    {
        get => _assessmentId;
        set
        {
            _assessmentId = value;
            LoadAssessment(_assessmentId);  // Load the assessment if provided
        }
    }

    private async void LoadAssessment(int assessmentId)
    {
        try
        {
            if (assessmentId != 0)
            {
                _assessment = await App.Database.GetAssessmentAsync(assessmentId);
                BindingContext = _assessment;

                // Show the Delete button if this is an existing assessment
                DeleteButton.IsVisible = true;
            }
            else
            {
                // Hide Delete button for a new assessment
                DeleteButton.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load assessment: {ex.Message}");
        }
    }

    public AssessmentDetailPage()
    {
        InitializeComponent();
        // If it's a new assessment, set default start and end dates
        if (_assessment.Id == 0)  // New assessment
        {
            _assessment.StartDate = DateTime.Today;              // Start Date ? Today
            _assessment.EndDate = DateTime.Today.AddDays(1);     // End Date ? Tomorrow

            BindingContext = _assessment;
        }
        // Show the Delete button only if editing an existing assessment
        DeleteButton.IsVisible = _assessment.Id != 0;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            _assessment.CourseId = _courseId;
            await App.Database.SaveAssessmentAsync(_assessment);

            // Schedule notifications for start and end alerts
            if (_assessment.AlertStart)
            {
                App.Database.ScheduleAssessmentNotifications(_assessment, isStartNotification: true);
            }
            else
            {
                LocalNotificationCenter.Current.Cancel(_assessment.Id * 10);  // Cancel start notification
            }

            if (_assessment.AlertEnd)
            {
                App.Database.ScheduleAssessmentNotifications(_assessment, isStartNotification: false);
            }
            else
            {
                LocalNotificationCenter.Current.Cancel(_assessment.Id * 10 + 1);  // Cancel end notification
            }

            await Shell.Current.GoToAsync("..");  // Navigate back after saving
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save assessment: {ex.Message}");
        }
    }

    private void OnStartAlertToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)  // If toggled ON
        {
            App.Database.ScheduleAssessmentNotifications(_assessment, isStartNotification: true);
        }
        else  // If toggled OFF
        {
            LocalNotificationCenter.Current.Cancel(_assessment.Id * 10);  // Cancel start notification
        }
    }

    private void OnEndAlertToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)  // If toggled ON
        {
            App.Database.ScheduleAssessmentNotifications(_assessment, isStartNotification: false);
        }
        else  // If toggled OFF
        {
            LocalNotificationCenter.Current.Cancel(_assessment.Id * 10 + 1);  // Cancel end notification
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirmDelete = await DisplayAlert("Delete", "Are you sure you want to delete this assessment?", "Yes", "No");

        if (confirmDelete)
        {
            try
            {
                await App.Database.DeleteAssessmentAsync(_assessment);
                await Shell.Current.GoToAsync("..");
                System.Diagnostics.Debug.WriteLine("Assessment deleted successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete assessment: {ex.Message}");
            }
        }
    }

}