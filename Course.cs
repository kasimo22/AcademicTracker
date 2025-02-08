using SQLite;

namespace AcademicTracker;

public class Course
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int TermId { get; set; } // Foreign key linking to Term
    public string? Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Status { get; set; } // Example: "In Progress", "Completed"
    public string? InstructorName { get; set; }
    public string? InstructorPhone { get; set; }
    public string? InstructorEmail { get; set; }
    public string? Notes { get; set; }
    public bool AlertStart { get; set; } // Start date alert toggle
    public bool AlertEnd { get; set; } // End date alert toggle
}
