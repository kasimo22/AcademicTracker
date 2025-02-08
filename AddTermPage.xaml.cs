namespace AcademicTracker;

public partial class AddTermPage : ContentPage
{
    public AddTermPage()
    {
        InitializeComponent();
    }

    private async void OnSaveTermClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TermTitleEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a term title.", "OK");
            return;
        }

        if (StartDatePicker.Date > EndDatePicker.Date)
        {
            await DisplayAlert("Error", "Start date cannot be after the end date.", "OK");
            return;
        }

        var newTerm = new Term
        {
            Title = TermTitleEntry.Text.Trim(),
            StartDate = StartDatePicker.Date,
            EndDate = EndDatePicker.Date
        };

        await App.Database.SaveTermAsync(newTerm);

        await DisplayAlert("Success", "Term added successfully!", "OK");
        await Shell.Current.GoToAsync("..");

        // Refresh the term list after navigating back
        if (Shell.Current.CurrentPage is MainPage mainPage)
        {
            mainPage.LoadTerms();  // Ensure LoadTerms reloads the terms
        }
    }
}