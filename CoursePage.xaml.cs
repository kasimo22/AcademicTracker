namespace AcademicTracker;

[QueryProperty(nameof(TermId), "TermId")]
public partial class CoursePage : ContentPage
{
    private Term _term = new Term();

    private int _termId;
    public int TermId
    {
        get => _termId;
        set
        {
            _termId = value;
            LoadTermAndCourses(_termId);
        }
    }

    public CoursePage()
    {
        InitializeComponent();
    }

    private async void LoadTermAndCourses(int termId)
    {
        try
        {
            // Retrieve the term from the database
            _term = (await App.Database.GetTermsAsync()).FirstOrDefault(t => t.Id == termId);

            if (_term == null)
            {
                Console.WriteLine($"Term with ID {termId} not found.");
                return;
            }

            // Set the page title to the term title
            Title = _term.Title;

            // Retrieve and display courses for the term
            var courses = await App.Database.GetCoursesAsync(termId);
            CoursesListView.ItemsSource = courses;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load term or courses: {ex.Message}");
        }
    }

    private async void OnAddCourseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AddCoursePage)}?TermId={_termId}");
    }

    private async void LoadCourses()
    {
        try
        {
            // Fetch and display courses for the selected term
            CoursesListView.ItemsSource = await App.Database.GetCoursesAsync(_termId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load courses: {ex.Message}");
        }
    }

    private async void OnCourseTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Course selectedCourse)
        {
            try
            {
                await Shell.Current.GoToAsync($"CourseDetailPage?CourseId={selectedCourse.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation to CourseDetailPage failed: {ex.Message}");
            }
        }
    }

    private async void OnEditTermClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(EditTermPage)}?TermId={_termId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation to EditTermPage failed: {ex.Message}");
        }
    }

    private async void OnDeleteTermClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this term and all its courses?", "Yes", "No");

        if (confirm)
        {
            try
            {
                // Delete all courses associated with the term
                var courses = await App.Database.GetCoursesAsync(_term.Id);
                foreach (var course in courses)
                {
                    await App.Database.DeleteCourseAsync(course);
                }

                // Delete the term itself
                await App.Database.DeleteTermAsync(_term);

                await DisplayAlert("Deleted", "Term and its courses have been deleted.", "OK");
                await Shell.Current.GoToAsync("//MainPage");

                // Refresh the term list
                if (Shell.Current.CurrentPage is MainPage mainPage)
                {
                    mainPage.LoadTerms();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting term: {ex.Message}");
                await DisplayAlert("Error", "Failed to delete term.", "OK");
            }
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
}