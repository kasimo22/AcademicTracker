using Plugin.LocalNotification;
using SQLite;
using System.IO;
using System.Threading.Tasks;

namespace AcademicTracker
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            // Initialize the SQLite connection
            _database = new SQLiteAsyncConnection(dbPath);

            // Ensure tables are created
            _database.CreateTableAsync<Term>().Wait();
            _database.CreateTableAsync<Course>().Wait();
            _database.CreateTableAsync<Assessment>().Wait();
        }

        public async Task<List<Term>> GetTermsAsync()
        {
            return await _database.Table<Term>().ToListAsync();
        }

        public async Task<int> SaveTermAsync(Term term)
        {
            if (term.Id != 0)
                return await _database.UpdateAsync(term);
            else
                return await _database.InsertAsync(term);
        }

        public async Task SaveCourseAsync(Course course)
        {
            if (course.Id != 0) // Update existing course
            {
                await _database.UpdateAsync(course);
            }
            else // Add new course
            {
                await _database.InsertAsync(course);
            }
        }

        public async Task<List<Course>> GetCoursesAsync(int termId)
        {
            return await _database.Table<Course>().Where(c => c.TermId == termId).ToListAsync();
        }

        public async Task<Course> GetCourseAsync(int courseId)
        {
            return await _database.Table<Course>().FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<int> DeleteTermAsync(Term term)
        {
            // Delete all courses related to the term
            var courses = await GetCoursesAsync(term.Id);
            foreach (var course in courses)
            {
                await DeleteCourseAsync(course);
            }

            // Delete the term
            return await _database.DeleteAsync(term);
        }

        public async Task<Term> GetTermAsync(int termId)
        {
            return await _database.Table<Term>().FirstOrDefaultAsync(t => t.Id == termId);
        }

        public async Task DeleteCourseAsync(Course course)
        {
            if (course.Id != 0)
            {
                await _database.DeleteAsync(course);
            }
        }

        // Get all assessments for a specific course
        public async Task<List<Assessment>> GetAssessmentsAsync(int courseId)
        {
            return await _database.Table<Assessment>().Where(a => a.CourseId == courseId).ToListAsync();
        }

        // Get a specific assessment by ID
        public async Task<Assessment> GetAssessmentAsync(int assessmentId)
        {
            return await _database.Table<Assessment>().FirstOrDefaultAsync(a => a.Id == assessmentId);
        }

        // Save or update an assessment
        public async Task<int> SaveAssessmentAsync(Assessment assessment)
        {
            if (assessment.CourseId == 0)
            {
                System.Diagnostics.Debug.WriteLine("Error: Assessment has no CourseId assigned.");
                return 0;  // Prevent saving without CourseId
            }

            if (assessment.Id != 0)
            {
                return await _database.UpdateAsync(assessment);
            }
            else
            {
                return await _database.InsertAsync(assessment);
            }
        }

        // Delete an assessment
        public async Task DeleteAssessmentAsync(Assessment assessment)
        {
            if (assessment.Id != 0)
                await _database.DeleteAsync(assessment);
        }

        public void ScheduleAssessmentNotifications(Assessment assessment, bool isStartNotification)
        {
            if (isStartNotification)
            {
                // Start Notification Logic
                var notifyTime = assessment.StartDate.Date == DateTime.Now.Date
                    ? DateTime.Now.AddSeconds(5)
                    : assessment.StartDate.Date.AddHours(12);  // Default to 12:00 PM

                var startNotification = new NotificationRequest
                {
                    NotificationId = assessment.Id * 10,
                    Title = "Assessment Start Reminder",
                    Description = $"Your assessment '{assessment.Title}' starts today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    },
                    ReturningData = $"{nameof(AssessmentDetailPage)}?AssessmentId={assessment.Id}"
                };

                LocalNotificationCenter.Current.Show(startNotification);
            }
            else
            {
                // End Notification Logic
                var notifyTime = assessment.EndDate.Date == DateTime.Now.Date
                    ? DateTime.Now.AddSeconds(5)
                    : assessment.EndDate.Date.AddHours(12);  // Default to 12:00 PM

                var endNotification = new NotificationRequest
                {
                    NotificationId = assessment.Id * 10 + 1,
                    Title = "Assessment End Reminder",
                    Description = $"Your assessment '{assessment.Title}' ends today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    },
                    ReturningData = $"{nameof(AssessmentDetailPage)}?AssessmentId={assessment.Id}"
                };

                LocalNotificationCenter.Current.Show(endNotification);
            }
        }


        public async Task SeedDatabaseAsync()
        {
            try
            {
                // Check if the database is already populated
                var terms = await GetTermsAsync();
                System.Diagnostics.Debug.WriteLine($"Existing terms count: {terms.Count}");
                if (terms.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Database already populated. Skipping seeding.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Seeding database with default terms and courses...");

                // Create terms
                var springTerm = new Term
                {
                    Title = "Spring 2025",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 6, 30)
                };
                var fallTerm = new Term
                {
                    Title = "Fall 2025",
                    StartDate = new DateTime(2025, 7, 1),
                    EndDate = new DateTime(2025, 12, 31)
                };
                await SaveTermAsync(springTerm);
                await SaveTermAsync(fallTerm);

                // Add course with Anika Patel as the instructor
                var foundationsCourse = new Course
                {
                    Title = "Scripting and Programming - Foundations",
                    TermId = springTerm.Id,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 3, 15),
                    Status = "In Progress",
                    InstructorName = "Anika Patel",
                    InstructorPhone = "555-123-4567",
                    InstructorEmail = "anika.patel@strimeuniversity.edu",
                    Notes = string.Empty,
                    AlertStart = false,
                    AlertEnd = false
                };
                await SaveCourseAsync(foundationsCourse);

                // Add two assessments for the course
                var objectiveAssessment = new Assessment
                {
                    CourseId = foundationsCourse.Id,
                    Title = "Objective Assessment 1",
                    Type = "Objective",
                    StartDate = new DateTime(2025, 2, 1),
                    EndDate = new DateTime(2025, 2, 5),
                    AlertStart = false,
                    AlertEnd = false
                };

                var performanceAssessment = new Assessment
                {
                    CourseId = foundationsCourse.Id,
                    Title = "Performance Assessment 1",
                    Type = "Performance",
                    StartDate = new DateTime(2025, 2, 10),
                    EndDate = new DateTime(2025, 2, 15),
                    AlertStart = false,
                    AlertEnd = false
                };

                // Save the assessments to the database
                await SaveAssessmentAsync(objectiveAssessment);
                await SaveAssessmentAsync(performanceAssessment);

                // Create the rest of the sample courses
                var springCourses = new List<Course>
                {
                    new Course { Title = "Introduction to Programming in Python", TermId = springTerm.Id, StartDate = new DateTime(2025, 1, 15), EndDate = new DateTime(2025, 4, 15), Status = "Planned", InstructorName = "Jane Smith", InstructorPhone = "555-333-4444", InstructorEmail = "jane.smith@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "IT Leadership Foundations", TermId = springTerm.Id, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 4, 30), Status = "Completed", InstructorName = "Jim Brown", InstructorPhone = "555-555-6666", InstructorEmail = "jim.brown@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Web Development Foundations", TermId = springTerm.Id, StartDate = new DateTime(2025, 1, 20), EndDate = new DateTime(2025, 4, 10), Status = "In Progress", InstructorName = "Alice White", InstructorPhone = "555-777-8888", InstructorEmail = "alice.white@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Version Control", TermId = springTerm.Id, StartDate = new DateTime(2025, 2, 10), EndDate = new DateTime(2025, 3, 30), Status = "Planned", InstructorName = "Bob Green", InstructorPhone = "555-999-0000", InstructorEmail = "bob.green@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "JavaScript Programming", TermId = springTerm.Id, StartDate = new DateTime(2025, 1, 25), EndDate = new DateTime(2025, 4, 20), Status = "Planned", InstructorName = "Charlie Black", InstructorPhone = "555-123-4567", InstructorEmail = "charlie.black@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false }
                };

                var fallCourses = new List<Course>
                {
                    new Course { Title = "Software I - C#", TermId = fallTerm.Id, StartDate = new DateTime(2025, 9, 10), EndDate = new DateTime(2025, 12, 15), Status = "Planned", InstructorName = "Lucas Brown", InstructorPhone = "555-567-8901", InstructorEmail = "lucas.brown@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Software II - Advanced C#", TermId = fallTerm.Id, StartDate = new DateTime(2025, 9, 15), EndDate = new DateTime(2025, 12, 20), Status = "Planned", InstructorName = "Sarah White", InstructorPhone = "555-678-9012", InstructorEmail = "sarah.white@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Software Security and Testing", TermId = fallTerm.Id, StartDate = new DateTime(2025, 9, 20), EndDate = new DateTime(2025, 12, 25), Status = "Planned", InstructorName = "David Black", InstructorPhone = "555-789-0123", InstructorEmail = "david.black@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Software Design and Quality Assurance", TermId = fallTerm.Id, StartDate = new DateTime(2025, 9, 25), EndDate = new DateTime(2025, 12, 30), Status = "Planned", InstructorName = "Michael Gray", InstructorPhone = "555-890-1234", InstructorEmail = "michael.gray@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Software Engineering", TermId = fallTerm.Id, StartDate = new DateTime(2025, 10, 1), EndDate = new DateTime(2025, 12, 31), Status = "Planned", InstructorName = "Jessica Black", InstructorPhone = "555-012-3456", InstructorEmail = "jessica.black@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false },
                    new Course { Title = "Cloud Foundations", TermId = fallTerm.Id, StartDate = new DateTime(2025, 10, 5), EndDate = new DateTime(2025, 12, 31), Status = "Planned", InstructorName = "Chris Green", InstructorPhone = "555-345-6780", InstructorEmail = "chris.green@example.com", Notes = string.Empty, AlertStart = false, AlertEnd = false }
                };

                System.Diagnostics.Debug.WriteLine("Inserting courses in batches...");
                // Insert courses in batches
                await _database.InsertAllAsync(springCourses);
                await _database.InsertAllAsync(fallCourses);

                System.Diagnostics.Debug.WriteLine("Database seeding completed.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during database seeding: {ex.Message}");
                throw;
            }
        }
    }

}