using BlazorApp1.Components;
using BlazorApp1.Components.Account;
using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// ?? Razor bileþenleri ve Blazor server ayarlarý
// ---------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// ---------------------------------------------------------
// ?? Kimlik Doðrulama & Yetkilendirme
// ---------------------------------------------------------
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// ---------------------------------------------------------
// ?? Uygulama Servisleri (Scoped)
// ---------------------------------------------------------
builder.Services.AddScoped<ITermService, TermService>();
builder.Services.AddScoped<ITeacherCourseService, TeacherCourseService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
builder.Services.AddScoped<ITeacherAvailabilityService, TeacherAvailabilityService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<ICourseSelectionService, CourseSelectionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ITeacherCourseTimeService, TeacherCourseTimeService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ITeacherGradingService, TeacherGradingService>();
builder.Services.AddScoped<IStudentGradeService, StudentGradeService>();
builder.Services.AddScoped<ITranscriptService, TranscriptService>();
builder.Services.AddScoped<IInternshipService, InternshipService>();
builder.Services.AddScoped<IStudentAdvisorService, StudentAdvisorService>();




// ---------------------------------------------------------
// ??? Veritabaný Contextleri (SIRALAMA KORUNDU)
// ---------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMudServices();

// ---------------------------------------------------------
// ?? Uygulama Oluþturuluyor
// ---------------------------------------------------------
var app = builder.Build();

// ------------------- Pipeline ------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();              // <-- 1
app.UseAuthentication();       // <-- 2 (eðer login varsa)
app.UseAuthorization();        // <-- 3 (Authorize attribute için þart)
app.UseAntiforgery();
// ---------------------------------------------------------
// ?? Razor bileþenlerinin ve Identity endpointlerinin baðlanmasý
// ---------------------------------------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// ---------------------------------------------------------
// ?? Rollerin ve baþlangýç verilerinin yüklenmesi
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    //await SeedData.Initialize(scope.ServiceProvider);
}

// ---------------------------------------------------------
app.Run();