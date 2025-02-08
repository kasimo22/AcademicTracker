namespace AcademicTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddTermPage), typeof(AddTermPage));
            Routing.RegisterRoute(nameof(CoursePage), typeof(CoursePage));
            Routing.RegisterRoute(nameof(AddCoursePage), typeof(AddCoursePage));
            Routing.RegisterRoute(nameof(CourseDetailPage), typeof(CourseDetailPage));
            Routing.RegisterRoute(nameof(AssessmentDetailPage), typeof(AssessmentDetailPage));
            Routing.RegisterRoute(nameof(AssessmentListPage), typeof(AssessmentListPage));
            Routing.RegisterRoute(nameof(EditTermPage), typeof(EditTermPage));
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            try
            {
                // Reset the navigation stack and navigate to MainPage
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to navigate to Home: {ex.Message}");
            }
        }
    }
}
