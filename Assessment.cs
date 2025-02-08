using SQLite;

namespace AcademicTracker
{
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CourseId { get; set; }  // Foreign Key to Course

        public string Title { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;  // "Objective" or "Performance"

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool AlertStart { get; set; }

        public bool AlertEnd { get; set; }
    }
}
