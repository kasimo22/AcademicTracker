namespace AcademicTracker;

[QueryProperty(nameof(CourseId), "CourseId")]
public partial class AssessmentListPage : ContentPage
{
    private int _courseId;

    public int CourseId
    {
        get => _courseId;
        set
        {
            _courseId = value;
            LoadAssessments();
        }
    }

    public AssessmentListPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAssessments();  // Refresh assessments every time the page appears
    }

    // Load assessments for the selected course
    private async void LoadAssessments()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading assessments for CourseId: {_courseId}");
            var assessments = await App.Database.GetAssessmentsAsync(_courseId);
            System.Diagnostics.Debug.WriteLine($"Found {assessments.Count} assessments.");
            AssessmentsListView.ItemsSource = assessments;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load assessments: {ex.Message}");
        }
    }

    // Navigate to AssessmentDetailPage when an assessment is selected
    private async void OnAssessmentSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Assessment selectedAssessment)
        {
            await Shell.Current.GoToAsync($"{nameof(AssessmentDetailPage)}?CourseId={selectedAssessment.CourseId}&AssessmentId={selectedAssessment.Id}");
        }

        // Deselect the item
        AssessmentsListView.SelectedItem = null;
    }

    // Navigate to AssessmentDetailPage to add a new assessment
    private async void OnAddAssessmentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AssessmentDetailPage)}?CourseId={_courseId}");
    }
}