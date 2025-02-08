namespace AcademicTracker;

[QueryProperty(nameof(TermId), "TermId")]
public partial class EditTermPage : ContentPage
{
    private int _termId;
    private Term _term = new Term();

    public int TermId
    {
        get => _termId;
        set
        {
            _termId = value;
            LoadTerm(_termId);
        }
    }

    public EditTermPage()
    {
        InitializeComponent();
    }

    private async void LoadTerm(int termId)
    {
        _term = await App.Database.GetTermAsync(termId);
        if (_term != null)
        {
            TermTitleEntry.Text = _term.Title;
            StartDatePicker.Date = _term.StartDate;
            EndDatePicker.Date = _term.EndDate;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _term.Title = TermTitleEntry.Text;
        _term.StartDate = StartDatePicker.Date;
        _term.EndDate = EndDatePicker.Date;

        await App.Database.SaveTermAsync(_term);
        await Shell.Current.GoToAsync(".."); // Navigate back to CoursePage
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Navigate back to CoursePage
    }
}