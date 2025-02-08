namespace AcademicTracker
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            LoadTerms();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadTerms();
        }

        public async void LoadTerms()
        {
            try
            {
                var terms = await App.Database.GetTermsAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded terms count: {terms.Count}");
                TermsListView.ItemsSource = null;
                TermsListView.ItemsSource = terms;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load terms: {ex.Message}");
            }
        }

        public void RefreshTerms()
        {
            LoadTerms();
        }

        private async void OnAddNewTermClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddTermPage));
        }

        private async void OnTermTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Term selectedTerm)
            {
                try
                {
                    // Navigate to CoursePage with TermId
                    await Shell.Current.GoToAsync($"//Courses?TermId={selectedTerm.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Navigation to CoursePage failed: {ex.Message}");
                }
            }
        }
    }

}

