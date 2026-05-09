using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Services.Interfaces;
using GymFlow.Models.Entities;

namespace GymFlow.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;

    public PdfExportService(
        IWorkoutPlanRepository workoutPlanRepository,
        IWorkoutDayRepository workoutDayRepository,
        IProgressLogRepository progressLogRepository,
        IUserRepository userRepository,
        IWorkoutSessionRepository workoutSessionRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
        _workoutDayRepository = workoutDayRepository;
        _progressLogRepository = progressLogRepository;
        _userRepository = userRepository;
        _workoutSessionRepository = workoutSessionRepository;
        
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false; // غیرفعال کردن بررسی ایموجی
    }

    public async Task<byte[]> ExportWorkoutPlanToPdfAsync(int workoutPlanId)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanWithDetailsAsync(workoutPlanId);
        if (plan is null)
            throw new Exception($"Workout plan with ID {workoutPlanId} not found");
        
        var user = await _userRepository.GetByIdAsync(plan.UserId);
        if (user is null)
            throw new Exception($"User with ID {plan.UserId} not found");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .ShowOnce()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("GYMFLOW WORKOUT PLAN").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Height(5);
                        col.Item().LineHorizontal(0.5f);
                        col.Item().Height(10);
                        
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"User: {user.FirstName} {user.LastName}");
                            row.RelativeItem().Text($"Phase: {plan.Phase}");
                            row.RelativeItem().Text($"Start Date: {plan.StartDate}");
                        });
                        
                        col.Item().Height(5);
                        
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Goal: {user.Goal}");
                            row.RelativeItem().Text($"Sessions/Week: {plan.SessionsPerWeek}");
                            row.RelativeItem().Text($"Status: {(plan.IsActive ? "ACTIVE" : "INACTIVE")}");
                        });
                        
                        col.Item().Height(10);
                        col.Item().LineHorizontal(0.5f);
                    });

                page.Content().Column(col =>
                {
                    foreach (var workoutDay in plan.WorkoutDays.OrderBy(wd => wd.DayOfWeek))
                    {
                        col.Item().PaddingBottom(15).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Column(innerCol =>
                        {
                            innerCol.Item().Row(row =>
                            {
                                row.AutoItem().Text($"[DAY] {workoutDay.DayOfWeek}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                                row.RelativeItem().AlignRight().Text($"[TARGET] {workoutDay.TargetMuscles}").FontSize(12).FontColor(Colors.Grey.Darken1);
                            });
                            
                            innerCol.Item().Height(5);
                            
                            innerCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(60);
                                    columns.RelativeColumn(2);
                                });
                                
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("#").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Exercise").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Sets").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Reps").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Rest").Bold();
                                });
                                
                                var index = 1;
                                foreach (var wde in workoutDay.WorkoutDayExercises)
                                {
                                    table.Cell().Padding(5).Text(index.ToString());
                                    table.Cell().Padding(5).Text(wde.Exercise?.Name ?? "Unknown");
                                    table.Cell().Padding(5).Text(wde.Sets.ToString());
                                    table.Cell().Padding(5).Text(wde.Reps);
                                    table.Cell().Padding(5).Text($"{wde.RestSeconds}s");
                                    index++;
                                }
                            });
                            
                            if (!string.IsNullOrEmpty(workoutDay.Notes))
                            {
                                innerCol.Item().PaddingTop(5).Text($"Note: {workoutDay.Notes}").FontSize(10).FontColor(Colors.Grey.Darken2).Italic();
                            }
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated by GymFlow - {DateTime.Now:yyyy-MM-dd HH:mm}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportProgressReportToPdfAsync(int userId, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception($"User with ID {userId} not found");
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var logList = logs.ToList();
        
        var startDate = fromDate ?? logList.LastOrDefault()?.LogDate ?? DateOnly.FromDateTime(DateTime.Now.AddMonths(-3));
        var endDate = toDate ?? logList.FirstOrDefault()?.LogDate ?? DateOnly.FromDateTime(DateTime.Now);
        
        var filteredLogs = logList.Where(l => l.LogDate >= startDate && l.LogDate <= endDate).ToList();
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sessionsList = sessions.ToList();
        
        var firstLog = filteredLogs.LastOrDefault();
        var lastLog = filteredLogs.FirstOrDefault();
        var totalWeightChange = (lastLog?.Weight ?? 0) - (firstLog?.Weight ?? 0);
        
        var weeklyLogs = filteredLogs
            .GroupBy(l => (l.LogDate.DayNumber - startDate.DayNumber) / 7)
            .Select(g => new { Week = g.Key, AvgWeight = g.Average(l => l.Weight) })
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .ShowOnce()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("GYMFLOW PROGRESS REPORT").FontSize(22).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Height(5);
                        col.Item().LineHorizontal(0.5f);
                        col.Item().Height(10);
                        
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"User: {user.FirstName} {user.LastName}");
                            row.RelativeItem().Text($"Report Period: {startDate} - {endDate}");
                            row.RelativeItem().Text($"Generated: {DateTime.Now:yyyy-MM-dd}");
                        });
                        
                        col.Item().Height(10);
                        col.Item().LineHorizontal(0.5f);
                    });

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Starting Weight").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{firstLog?.Weight:F1} kg").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Current Weight").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{lastLog?.Weight:F1} kg").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Total Change").FontSize(10).FontColor(Colors.Grey.Darken1);
                            var changeColor = totalWeightChange < 0 ? Colors.Green.Medium : Colors.Red.Medium;
                            c.Item().Text($"{(totalWeightChange > 0 ? "+" : "")}{totalWeightChange:F1} kg").FontSize(16).Bold().FontColor(changeColor);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Total Workouts").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{sessionsList.Count}").FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                        });
                    });
                    
                    col.Item().Height(15);
                    
                    col.Item().Text("WEIGHT HISTORY").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                    col.Item().Height(5);
                    
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                        });
                        
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Date").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Weight (kg)").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Body Fat %").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Notes").Bold();
                        });
                        
                        foreach (var log in filteredLogs.Take(20))
                        {
                            table.Cell().Padding(5).Text(log.LogDate.ToString());
                            table.Cell().Padding(5).Text($"{log.Weight:F1}");
                            table.Cell().Padding(5).Text(log.BodyFatPercentage?.ToString("F1") ?? "-");
                            table.Cell().Padding(5).Text(log.Notes ?? "-");
                        }
                    });
                    
                    if (filteredLogs.Count > 20)
                    {
                        col.Item().PaddingTop(5).AlignRight().Text($"Showing 20 of {filteredLogs.Count} entries").FontSize(9).FontColor(Colors.Grey.Medium);
                    }
                    
                    col.Item().Height(15);
                    
                    if (weeklyLogs.Any())
                    {
                        col.Item().Text("WEEKLY AVERAGE WEIGHT").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                        col.Item().Height(5);
                        
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Week").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Average Weight (kg)").Bold();
                            });
                            
                            foreach (var week in weeklyLogs)
                            {
                                table.Cell().Padding(5).Text($"Week {week.Week + 1}");
                                table.Cell().Padding(5).Text($"{week.AvgWeight:F1}");
                            }
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated by GymFlow - {DateTime.Now:yyyy-MM-dd HH:mm}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportWeeklySummaryToPdfAsync(int userId, DateOnly? weekStart = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception($"User with ID {userId} not found");
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = weekStart ?? today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        
        var sessions = await _workoutSessionRepository.GetSessionsByDateRangeAsync(userId, startOfWeek, endOfWeek);
        var sessionsList = sessions.ToList();
        
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .ShowOnce()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("WEEKLY WORKOUT SUMMARY").FontSize(22).Bold().FontColor(Colors.Orange.Darken2);
                        col.Item().Height(5);
                        col.Item().LineHorizontal(0.5f);
                        col.Item().Height(10);
                        
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"User: {user.FirstName} {user.LastName}");
                            row.RelativeItem().Text($"Week: {startOfWeek} - {endOfWeek}");
                        });
                        
                        col.Item().Height(10);
                        col.Item().LineHorizontal(0.5f);
                    });

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Completed").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{sessionsList.Count}").FontSize(20).Bold().FontColor(Colors.Green.Darken2);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Planned").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{activePlan?.SessionsPerWeek ?? 0}").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            var percentage = activePlan?.SessionsPerWeek > 0 
                                ? (int)((double)sessionsList.Count / activePlan.SessionsPerWeek * 100) 
                                : 0;
                            c.Item().Text("Completion").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{percentage}%").FontSize(20).Bold().FontColor(Colors.Orange.Darken2);
                        });
                        
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
                        {
                            c.Item().Text("Total Minutes").FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{sessionsList.Sum(s => s.ActualDurationMinutes)}").FontSize(20).Bold().FontColor(Colors.Purple.Darken2);
                        });
                    });
                    
                    col.Item().Height(15);
                    
                    col.Item().Text("DAILY BREAKDOWN").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                    col.Item().Height(5);
                    
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });
                        
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Day").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Duration").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Muscle Group").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Feeling").Bold();
                        });
                        
                        var sessionsByDay = sessionsList
                            .GroupBy(s => s.ActualDate.DayOfWeek)
                            .OrderBy(g => g.Key);
                        
                        foreach (var group in sessionsByDay)
                        {
                            var session = group.First();
                            var duration = group.Sum(s => s.ActualDurationMinutes);
                            
                            table.Cell().Padding(5).Text(group.Key.ToString());
                            table.Cell().Padding(5).Text($"{duration} min");
                            table.Cell().Padding(5).Text(session.WorkoutDay?.TargetMuscles.ToString() ?? "-");
                            table.Cell().Padding(5).Text(session.Feeling ?? "-");
                        }
                    });
                    
                    col.Item().Height(15);
                    
                    if (sessionsList.Any(s => !string.IsNullOrEmpty(s.Feeling)))
                    {
                        col.Item().Text("WORKOUT NOTES").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                        col.Item().Height(5);
                        
                        foreach (var session in sessionsList.Where(s => !string.IsNullOrEmpty(s.Feeling)))
                        {
                            col.Item().PaddingBottom(5).Row(row =>
                            {
                                row.AutoItem().Text($"{session.ActualDate}: ").Bold();
                                row.RelativeItem().Text(session.Feeling);
                            });
                        }
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated by GymFlow - {DateTime.Now:yyyy-MM-dd HH:mm}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportAchievementsCertificateAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception($"User with ID {userId} not found");
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var totalWorkouts = sessions.Count();
        
        var achievements = new List<string>();
        if (totalWorkouts >= 10) achievements.Add("First Milestone: 10 Workouts");
        if (totalWorkouts >= 50) achievements.Add("Dedicated Athlete: 50 Workouts");
        if (totalWorkouts >= 100) achievements.Add("Iron Warrior: 100 Workouts");
        
        var streak = await GetStreakAsync(sessions.ToList());
        if (streak >= 7) achievements.Add($"Consistency King: {streak} Day Streak");
        if (streak >= 30) achievements.Add($"Unstoppable: {streak} Day Streak");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.Grey.Lighten4);

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(50).AlignCenter().Text("GYMFLOW ACHIEVEMENT CERTIFICATE")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Orange.Darken2);
                    
                    col.Item().PaddingTop(30).AlignCenter().Text("This certificate is proudly presented to")
                        .FontSize(16)
                        .FontColor(Colors.Grey.Darken2);
                    
                    col.Item().PaddingTop(10).AlignCenter().Text($"{user.FirstName} {user.LastName}")
                        .FontSize(32)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                    
                    col.Item().PaddingTop(10).AlignCenter().Text($"Total Workouts Completed: {totalWorkouts}")
                        .FontSize(14);
                    
                    if (achievements.Any())
                    {
                        col.Item().PaddingTop(20).AlignCenter().Text("Achievements:").FontSize(16).Bold();
                        
                        foreach (var achievement in achievements)
                        {
                            col.Item().PaddingTop(5).AlignCenter().Text(achievement).FontSize(12);
                        }
                    }
                    
                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Text("_________________").FontSize(10);
                        row.RelativeItem().AlignCenter().Text("_________________").FontSize(10);
                    });
                    
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Text("Date").FontSize(9);
                        row.RelativeItem().AlignCenter().Text("Signature").FontSize(9);
                    });
                    
                    col.Item().PaddingTop(30).AlignCenter().Text($"Issued on {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task<int> GetStreakAsync(List<WorkoutSession> sessions)
    {
        var sortedSessions = sessions.OrderByDescending(s => s.ActualDate).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentDate = today;
        var streak = 0;
        
        while (sortedSessions.Any(s => s.ActualDate == currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }
        
        return streak;
    }
}