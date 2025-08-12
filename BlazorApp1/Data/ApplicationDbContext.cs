using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;

namespace BlazorApp1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Identity dýþý tablolar
        public DbSet<Course> Courses { get; set; }
        public DbSet<GradeType> GradeTypes { get; set; }
        public DbSet<LetterGradeRange> LetterGradeRanges { get; set; }
        public DbSet<Simulation> Simulations { get; set; }
        public DbSet<SimulationCourse> SimulationCourses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<TeacherCourseTime> TeacherCourseTimes { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<CourseSelectionSetting> CourseSelectionSettings { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<CourseGradeType> CourseGradeTypes {  get; set; }
        public DbSet<TranscriptRecord> TranscriptRecords { get; set; }
        public DbSet<InternshipApplication> InternshipApplications => Set<InternshipApplication>();
        public DbSet<InternshipDocument> InternshipDocuments => Set<InternshipDocument>();
        public DbSet<InternshipReview> InternshipReviews => Set<InternshipReview>();
        public DbSet<InternshipMessage> InternshipMessages => Set<InternshipMessage>();
        public DbSet<StudentAdvisor> StudentAdvisors => Set<StudentAdvisor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ?? Identity tablolarýný unutma
            
            // Course
            modelBuilder.Entity<Course>().HasKey(e => e.Id);
            modelBuilder.Entity<Course>().Property(e => e.CourseName).HasMaxLength(200);
            modelBuilder.Entity<Course>()
                .HasOne(e => e.Term)
                .WithMany(t => t.Courses)
                .HasForeignKey(e => e.TermId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Course>()
                .Property(e => e.WeeklySlotCount)
                .HasDefaultValue(2) // varsayýlan 2 olsun, istersen deðiþtirebilirsin
                .IsRequired();
            modelBuilder.Entity<Course>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Courses)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // GradeType
            modelBuilder.Entity<GradeType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.GradeName)
                      .HasMaxLength(50);

                entity.Property(e => e.IsMandatory)
                      .IsRequired();

                entity.Property(e => e.IsUniquePerCourse)
                      .IsRequired();
            });

            //CourseGradeType
            modelBuilder.Entity<CourseGradeType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Weight)
                      .IsRequired();

                entity.HasIndex(e => new { e.CourseId, e.GradeTypeId })
                      .IsUnique(); // ?? Her ders için bir sýnav türü sadece 1 kez tanýmlanabilir

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.CourseGradeTypes)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.GradeType)
                      .WithMany(g => g.CourseGradeTypes)
                      .HasForeignKey(e => e.GradeTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // LetterGradeRange
            modelBuilder.Entity<LetterGradeRange>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Letter)
                      .HasMaxLength(5)
                      .IsRequired();

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.LetterGradeRanges)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CourseId, e.Letter })
                      .IsUnique(); // ?? Bir ders için ayný harf bir kez tanýmlanýr
            });

            modelBuilder.Entity<Simulation>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Title zorunlu ve max 100 karakter
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(100);

                // Description opsiyonel ama sýnýrlý uzunlukta
                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                // Goal opsiyonel, max 250 karakter
                entity.Property(e => e.Goal)
                      .HasMaxLength(250);

                // CreatedAt zorunlu
                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                // Foreign key baðlantýsý: Her Simulation bir ApplicationUser'a (öðrenci) aittir
                entity.HasOne(e => e.Student)
                      .WithMany(u => u.Simulations) // ApplicationUser tarafýnda: ICollection<Simulation> Simulations
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // SimulationCourse
            modelBuilder.Entity<SimulationCourse>().HasKey(e => e.Id);
            modelBuilder.Entity<SimulationCourse>().Property(e => e.CourseName).HasMaxLength(200);
            modelBuilder.Entity<SimulationCourse>().Property(e => e.LetterGrade).HasMaxLength(5);
            modelBuilder.Entity<SimulationCourse>().Property(e => e.TermInfo).HasMaxLength(100);


            // StudentCourse
            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.LetterGrade).HasMaxLength(5);

                entity.HasOne(sc => sc.Course)
                      .WithMany(c => c.StudentCourses)
                      .HasForeignKey(sc => sc.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sc => sc.Term)
                      .WithMany()
                      .HasForeignKey(sc => sc.TermId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(sc => sc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // TeacherCourse
            modelBuilder.Entity<TeacherCourse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(tc => tc.Course)
                      .WithMany(c => c.TeacherCourses)
                      .HasForeignKey(tc => tc.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(tc => tc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TeacherCourseTime
            modelBuilder.Entity<TeacherCourseTime>().HasKey(e => e.Id); // ? composite key yok

            modelBuilder.Entity<TeacherCourseTime>()
                .HasOne(tct => tct.Course)
                .WithMany(c => c.TeacherCourseTimes)
                .HasForeignKey(tct => tct.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherCourseTime>()
                .HasOne(tct => tct.TimeSlot)
                .WithMany(ts => ts.TeacherCourseTimes)
                .HasForeignKey(tct => tct.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherCourseTime>()
                .HasOne(tct => tct.Classroom)
                .WithMany(c => c.TeacherCourseTimes)
                .HasForeignKey(tct => tct.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);
            // Term
            modelBuilder.Entity<Term>().HasKey(e => e.Id);

            // TimeSlot
            modelBuilder.Entity<TimeSlot>().HasKey(e => e.Id);
            modelBuilder.Entity<TimeSlot>().Property(e => e.Day).HasMaxLength(15);

            // CourseSelectionSetting
            modelBuilder.Entity<CourseSelectionSetting>().HasKey(e => e.Id);
            modelBuilder.Entity<CourseSelectionSetting>().HasData(new CourseSelectionSetting
            {
                Id = 1,
                IsSelectionOpen = false
            });

            // Faculty
            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Faculty");
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            });

            // Department
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Department");
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.HasOne(d => d.Faculty)
                      .WithMany(f => f.Departments)
                      .HasForeignKey(d => d.FacultyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // StudentGrade (tamamlandýktan sonra kapatýlmalý)
            modelBuilder.Entity<StudentGrade>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Score).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.VersionNumber).IsRequired();
                entity.Property(e => e.IsDraft).IsRequired();

                entity.HasIndex(e => new { e.UserId, e.CourseGradeTypeId }).IsUnique();

                entity.HasOne(e => e.CourseGradeType)
                      .WithMany(cgt => cgt.StudentGrades)
                      .HasForeignKey(e => e.CourseGradeTypeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            }); // ?? BU SATIR ÞART!
            modelBuilder.Entity<TranscriptRecord>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PlaceholderName)
                      .HasMaxLength(100); // “Seçmeli 1” gibi metinler için yeterli

                entity.Property(e => e.LetterGrade)
                      .HasMaxLength(5); // AA, BA gibi

                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Term)
                      .WithMany()
                      .HasForeignKey(e => e.TermId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.SetNull); // Seçmeli deðiþirse veya silinirse bozulmasýn
            });
            // InternshipApplication
            modelBuilder.Entity<InternshipApplication>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Status)
                 .HasConversion<int>()
                 .IsRequired();

                e.Property(x => x.CurrentVersion)
                 .HasDefaultValue(1);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.Property(x => x.UpdatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.Student)
                 .WithMany() // istersen ApplicationUser içine ICollection<InternshipApplication> ekleyebilirsin
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Term)
                 .WithMany()
                 .HasForeignKey(x => x.TermId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.StudentId, x.Status });
            });
            modelBuilder.Entity<InternshipDocument>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
                e.Property(x => x.FilePath).HasMaxLength(1024).IsRequired();
                e.Property(x => x.ContentHash).HasMaxLength(128).IsRequired();

                e.Property(x => x.UploadedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.Application)
                 .WithMany(a => a.Documents)
                 .HasForeignKey(x => x.ApplicationId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Ayný baþvuruda versiyon unique
                e.HasIndex(x => new { x.ApplicationId, x.Version }).IsUnique();

                // Ayný baþvuruda ayný hash tekrar edilmesin
                e.HasIndex(x => new { x.ApplicationId, x.ContentHash }).IsUnique();
            });
            modelBuilder.Entity<InternshipReview>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.ReviewerRole)
                 .HasConversion<int>()
                 .IsRequired();

                e.Property(x => x.Decision)
                 .HasConversion<int>()
                 .IsRequired();

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.Application)
                 .WithMany(a => a.Reviews)
                 .HasForeignKey(x => x.ApplicationId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Reviewer)
                 .WithMany()
                 .HasForeignKey(x => x.ReviewerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.ApplicationId, x.CreatedAt });
            });
            // InternshipMessage
            modelBuilder.Entity<InternshipMessage>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Body)
                 .IsRequired()
                 .HasMaxLength(4000);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.Application)
                 .WithMany(a => a.Messages)
                 .HasForeignKey(x => x.ApplicationId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Sender)
                 .WithMany()
                 .HasForeignKey(x => x.SenderId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.ApplicationId, x.CreatedAt });
            });
            // StudentAdvisor (opsiyonel)
            modelBuilder.Entity<StudentAdvisor>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.Student)
                 .WithMany()
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Teacher)
                 .WithMany()
                 .HasForeignKey(x => x.TeacherId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Bir öðrencinin ayný anda tek aktif danýþmaný olsun (filtered unique index)
                e.HasIndex(x => new { x.StudentId, x.IsActive })
                 .HasFilter("[IsActive] = 1")
                 .IsUnique();
            });


            // ? Þimdi Classroom baðýmsýz tanýmlanmalý
            modelBuilder.Entity<Classroom>().HasKey(c => c.Id);

            modelBuilder.Entity<Classroom>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Classroom>()
                .Property(c => c.Capacity)
                .HasDefaultValue(0);
            modelBuilder.Entity<Classroom>()
    .HasOne(c => c.Department)
    .WithMany() // Eðer Department ? Classrooms koleksiyonu varsa .WithMany(d => d.Classrooms)
    .HasForeignKey(c => c.DepartmentId)
    .IsRequired(false) // Nullable iliþkidir
    .OnDelete(DeleteBehavior.Restrict); // Ýsteðe baðlý: departman silinse sýnýf silinmes
        }    
    }
}