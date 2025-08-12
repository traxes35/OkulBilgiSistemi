namespace BlazorApp1.Models
{
    public enum InternshipStatus
    {
        Submitted = 0,               // Öğrenci yükledi -> direkt InTeacherReview'a da çekebilirsin
        InTeacherReview = 1,         // Öğretmen incelemesinde
        InAdminReview = 2,           // Admin incelemesinde (öğretmen onayından sonra)
        AdminApproved = 3,           // Final onay
        NeedsFixByStudent = 4,       // Düzenleme için tekrar öğrenciye döndü
        AdminRejectedWaitingTeacher = 5 // Admin reddetti; öğretmenin öğrenciye bilgilendirmesi bekleniyor
    }

    public enum ReviewDecision
    {
        Approve = 0,
        Reject = 1
    }

    public enum ReviewerRole
    {
        Teacher = 0,
        Admin = 1
    }

}
