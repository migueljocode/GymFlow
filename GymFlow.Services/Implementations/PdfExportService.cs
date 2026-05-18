namespace GymFlow.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;

    public PdfExportService(
        IWorkoutPlanRepository workoutPlanRepository,
        IProgressLogRepository progressLogRepository,
        IUserRepository userRepository,
        IWorkoutSessionRepository workoutSessionRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
        _progressLogRepository = progressLogRepository;
        _userRepository = userRepository;
        _workoutSessionRepository = workoutSessionRepository;
        
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    public async Task<byte[]> ExportWorkoutPlanToPdfAsync(int workoutPlanId)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanWithDetailsAsync(workoutPlanId);
        if (plan is null) throw new Exception($"Workout plan with ID {workoutPlanId} not found");
        
        var user = await _userRepository.GetUserWithPersonAsync(plan.UserId);
        if (user?.Person is null) throw new Exception($"User with ID {plan.UserId} not found");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                AddWorkoutPlanHeader(page, user.Person, plan);
                
                page.Content().Column(column =>
                {
                    foreach (var workoutDay in plan.WorkoutDays.OrderBy(wd => wd.DayOfWeek))
                    {
                        column.Item().PaddingBottom(15).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Column(innerCol =>
                        {
                            innerCol.Item().Row(row =>
                            {
                                row.AutoItem().Text($"[DAY] {workoutDay.DayOfWeek}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                                row.RelativeItem().AlignRight().Text($"[TARGET] {workoutDay.TargetMuscles}").FontSize(12).FontColor(Colors.Grey.Darken1);
                            });
                            
                            innerCol.Item().Height(5);
                            AddExerciseTable(innerCol, workoutDay);
                            
                            if (!string.IsNullOrEmpty(workoutDay.Notes))
                            {
                                innerCol.Item().PaddingTop(5).Text($"Note: {workoutDay.Notes}").FontSize(10).FontColor(Colors.Grey.Darken2).Italic();
                            }
                        });
                    }
                });
                
                AddFooter(page);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ExportProgressReportToPdfAsync(int userId, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user?.Person is null) throw new Exception($"User with ID {userId} not found");
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        
        var filteredLogs = FilterLogsByDate(logs.ToList(), fromDate, toDate);
        var stats = CalculateProgressStats(filteredLogs);
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                AddProgressReportHeader(page, user.Person, fromDate, toDate);
                
                page.Content().Column(column =>
                {
                    AddProgressStatsCards(column, stats);
                    column.Item().Height(15);
                    column.Item().Text("WEIGHT HISTORY").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                    column.Item().Height(5);
                    AddWeightHistoryTable(column, filteredLogs);
                });
                
                AddFooter(page);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ExportWeeklySummaryToPdfAsync(int userId, DateOnly? weekStart = null)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user?.Person is null) throw new Exception($"User with ID {userId} not found");
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = weekStart ?? today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        
        var sessions = (await _workoutSessionRepository.GetSessionsByDateRangeAsync(userId, startOfWeek, endOfWeek)).ToList();
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                AddWeeklySummaryHeader(page, user.Person, startOfWeek, endOfWeek);
                
                page.Content().Column(column =>
                {
                    AddWeeklyStatsCards(column, sessions, activePlan);
                    column.Item().Height(15);
                    column.Item().Text("DAILY BREAKDOWN").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                    column.Item().Height(5);
                    AddDailyBreakdownTable(column, sessions);
                });
                
                AddFooter(page);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ExportAchievementsCertificateAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user?.Person is null) throw new Exception($"User with ID {userId} not found");
        
        var sessions = (await _workoutSessionRepository.GetSessionsByUserAsync(userId)).ToList();
        var achievements = GetAchievements(sessions.Count, await GetCurrentStreakAsync(sessions));
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.Grey.Lighten4);

                AddCertificateContent(page, user.Person, sessions.Count, achievements);
            });
        }).GeneratePdf();
    }

    // ========== Private Helper Methods ==========
    
    private void AddWorkoutPlanHeader(PageDescriptor page, Person person, WorkoutPlan plan)
    {
        page.Header().ShowOnce().Column(col =>
        {
            col.Item().AlignCenter().Text("GYMFLOW WORKOUT PLAN").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            col.Item().Height(5);
            col.Item().LineHorizontal(0.5f);
            col.Item().Height(10);
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"User: {person.FirstName} {person.LastName}");
                row.RelativeItem().Text($"Phase: {plan.Phase}");
                row.RelativeItem().Text($"Start Date: {plan.StartDate}");
            });
            
            col.Item().Height(5);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Goal: {person.User?.Goal}");
                row.RelativeItem().Text($"Sessions/Week: {plan.SessionsPerWeek}");
                row.RelativeItem().Text($"Status: {(plan.IsActive ? "ACTIVE" : "INACTIVE")}");
            });
            
            col.Item().Height(10);
            col.Item().LineHorizontal(0.5f);
        });
    }
    
    private void AddExerciseTable(ColumnDescriptor col, WorkoutDay workoutDay)
    {
        col.Item().Table(table =>
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
    }
    
    private void AddProgressReportHeader(PageDescriptor page, Person person, DateOnly? fromDate, DateOnly? toDate)
    {
        page.Header().ShowOnce().Column(col =>
        {
            col.Item().AlignCenter().Text("GYMFLOW PROGRESS REPORT").FontSize(22).Bold().FontColor(Colors.Green.Darken2);
            col.Item().Height(5);
            col.Item().LineHorizontal(0.5f);
            col.Item().Height(10);
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"User: {person.FirstName} {person.LastName}");
                row.RelativeItem().Text($"Report Period: {fromDate?.ToString() ?? "Start"} - {toDate?.ToString() ?? "Present"}");
                row.RelativeItem().Text($"Generated: {DateTime.Now:yyyy-MM-dd}");
            });
            
            col.Item().Height(10);
            col.Item().LineHorizontal(0.5f);
        });
    }
    
    private void AddProgressStatsCards(ColumnDescriptor column, ProgressStats stats)
    {
        column.Item().Row(row =>
        {
            AddStatCard(row, "Starting Weight", $"{stats.StartingWeight:F1} kg", Colors.Green.Darken2);
            AddStatCard(row, "Current Weight", $"{stats.CurrentWeight:F1} kg", Colors.Blue.Darken2);
            AddStatCard(row, "Total Change", $"{(stats.TotalChange > 0 ? "+" : "")}{stats.TotalChange:F1} kg", 
                stats.TotalChange < 0 ? Colors.Green.Medium : Colors.Red.Medium);
            AddStatCard(row, "Total Workouts", $"{stats.TotalWorkouts}", Colors.Green.Darken2);
        });
    }
    
    private void AddStatCard(RowDescriptor row, string label, string value, Color color)
    {
        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Column(c =>
        {
            c.Item().Text(label).FontSize(10).FontColor(Colors.Grey.Darken1);
            c.Item().Text(value).FontSize(16).Bold().FontColor(color);
        });
    }
    
    private void AddWeightHistoryTable(ColumnDescriptor column, List<ProgressLog> logs)
    {
        column.Item().Table(table =>
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
            
            foreach (var log in logs.Take(20))
            {
                table.Cell().Padding(5).Text(log.LogDate.ToString());
                table.Cell().Padding(5).Text($"{log.Weight:F1}");
                table.Cell().Padding(5).Text(log.BodyFatPercentage?.ToString("F1") ?? "-");
                table.Cell().Padding(5).Text(log.Notes ?? "-");
            }
        });
    }
    
    private void AddWeeklySummaryHeader(PageDescriptor page, Person person, DateOnly startOfWeek, DateOnly endOfWeek)
    {
        page.Header().ShowOnce().Column(col =>
        {
            col.Item().AlignCenter().Text("WEEKLY WORKOUT SUMMARY").FontSize(22).Bold().FontColor(Colors.Orange.Darken2);
            col.Item().Height(5);
            col.Item().LineHorizontal(0.5f);
            col.Item().Height(10);
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"User: {person.FirstName} {person.LastName}");
                row.RelativeItem().Text($"Week: {startOfWeek} - {endOfWeek}");
            });
            
            col.Item().Height(10);
            col.Item().LineHorizontal(0.5f);
        });
    }
    
    private void AddWeeklyStatsCards(ColumnDescriptor column, List<WorkoutSession> sessions, WorkoutPlan? activePlan)
    {
        var percentage = activePlan?.SessionsPerWeek > 0 
            ? (int)((double)sessions.Count / activePlan.SessionsPerWeek * 100) 
            : 0;
        
        column.Item().Row(row =>
        {
            AddStatCard(row, "Completed", $"{sessions.Count}", Colors.Green.Darken2);
            AddStatCard(row, "Planned", $"{activePlan?.SessionsPerWeek ?? 0}", Colors.Blue.Darken2);
            AddStatCard(row, "Completion", $"{percentage}%", Colors.Orange.Darken2);
            AddStatCard(row, "Total Minutes", $"{sessions.Sum(s => s.ActualDurationMinutes)}", Colors.Purple.Darken2);
        });
    }
    
    private void AddDailyBreakdownTable(ColumnDescriptor column, List<WorkoutSession> sessions)
    {
        column.Item().Table(table =>
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
            
            var sessionsByDay = sessions
                .GroupBy(s => s.ActualDate.DayOfWeek)
                .OrderBy(g => g.Key);
            
            foreach (var group in sessionsByDay)
            {
                var session = group.First();
                table.Cell().Padding(5).Text(group.Key.ToString());
                table.Cell().Padding(5).Text($"{group.Sum(s => s.ActualDurationMinutes)} min");
                table.Cell().Padding(5).Text(session.WorkoutDay?.TargetMuscles.ToString() ?? "-");
                table.Cell().Padding(5).Text(session.Feeling ?? "-");
            }
        });
    }
    
    private void AddCertificateContent(PageDescriptor page, Person person, int totalWorkouts, List<string> achievements)
    {
        page.Content().Column(col =>
        {
            col.Item().PaddingTop(50).AlignCenter().Text("GYMFLOW ACHIEVEMENT CERTIFICATE")
                .FontSize(28).Bold().FontColor(Colors.Orange.Darken2);
            
            col.Item().PaddingTop(30).AlignCenter().Text("This certificate is proudly presented to")
                .FontSize(16).FontColor(Colors.Grey.Darken2);
            
            col.Item().PaddingTop(10).AlignCenter().Text($"{person.FirstName} {person.LastName}")
                .FontSize(32).Bold().FontColor(Colors.Blue.Darken2);
            
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
                .FontSize(10).FontColor(Colors.Grey.Medium);
        });
    }
    
    private void AddFooter(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text($"Generated by GymFlow - {DateTime.Now:yyyy-MM-dd HH:mm}")
            .FontSize(9).FontColor(Colors.Grey.Medium);
    }
    
    // ========== Private Data Helpers ==========
    
    private List<ProgressLog> FilterLogsByDate(List<ProgressLog> logs, DateOnly? fromDate, DateOnly? toDate)
    {
        var startDate = fromDate ?? logs.LastOrDefault()?.LogDate ?? DateOnly.FromDateTime(DateTime.Now.AddMonths(-3));
        var endDate = toDate ?? logs.FirstOrDefault()?.LogDate ?? DateOnly.FromDateTime(DateTime.Now);
        
        return logs.Where(l => l.LogDate >= startDate && l.LogDate <= endDate).ToList();
    }
    
    private ProgressStats CalculateProgressStats(List<ProgressLog> logs)
    {
        var firstLog = logs.LastOrDefault();
        var lastLog = logs.FirstOrDefault();
        
        return new ProgressStats
        {
            StartingWeight = firstLog?.Weight ?? 0,
            CurrentWeight = lastLog?.Weight ?? 0,
            TotalChange = (lastLog?.Weight ?? 0) - (firstLog?.Weight ?? 0),
            TotalWorkouts = 0
        };
    }
    
    private async Task<int> GetCurrentStreakAsync(List<WorkoutSession> sessions)
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
    
    private List<string> GetAchievements(int totalWorkouts, int streak)
    {
        var achievements = new List<string>();
        
        if (totalWorkouts >= 10) achievements.Add("First Milestone: 10 Workouts");
        if (totalWorkouts >= 50) achievements.Add("Dedicated Athlete: 50 Workouts");
        if (totalWorkouts >= 100) achievements.Add("Iron Warrior: 100 Workouts");
        if (streak >= 7) achievements.Add($"Consistency King: {streak} Day Streak");
        if (streak >= 30) achievements.Add($"Unstoppable: {streak} Day Streak");
        
        return achievements;
    }
    
    private class ProgressStats
    {
        public float StartingWeight { get; set; }
        public float CurrentWeight { get; set; }
        public float TotalChange { get; set; }
        public int TotalWorkouts { get; set; }
    }
}