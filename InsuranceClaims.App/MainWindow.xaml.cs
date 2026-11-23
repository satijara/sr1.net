using System.IO;
using System.Windows;
using InsuranceClaims.App.Analytics;
using InsuranceClaims.App.Data;
using InsuranceClaims.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace InsuranceClaims.App;

public partial class MainWindow : Window
{
    private readonly string _databasePath;
    private CancellationTokenSource? _importCancellation;

    public MainWindow()
    {
        InitializeComponent();

        string appDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InsuranceClaimsSelfWork19");
        Directory.CreateDirectory(appDirectory);
        _databasePath = Path.Combine(appDirectory, "insurance.db");

        Loaded += MainWindow_Loaded;
        Closed += (_, _) => _importCancellation?.Cancel();
    }

    private InsuranceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        return new InsuranceDbContext(options);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await using var db = CreateDbContext();
            await db.Database.EnsureCreatedAsync();
            DatabasePathTextBlock.Text = $"База даних SQLite: {_databasePath}";
            await RefreshAnalyticsAsync();
        }
        catch (Exception ex)
        {
            ShowError("Не вдалося ініціалізувати базу даних", ex);
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Оберіть JSON-файл страхових полісів",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true
        };

        string samplePath = Path.Combine(
            AppContext.BaseDirectory, "SampleData", "insurance-sample.json");
        if (File.Exists(samplePath))
            dialog.FileName = samplePath;

        if (dialog.ShowDialog(this) == true)
            JsonPathTextBox.Text = dialog.FileName;
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        string jsonPath = JsonPathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            MessageBox.Show("Спочатку оберіть JSON-файл.", "Немає файлу",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _importCancellation = new CancellationTokenSource();
        SetImportState(isImporting: true);

        try
        {
            await using var db = CreateDbContext();
            var importService = new InsuranceImportService(
                db, new InsuranceJsonReader(), new InsuranceDataValidator());

            int imported = await importService.ImportAsync(
                jsonPath, _importCancellation.Token);

            StatusTextBlock.Text = $"Імпорт завершено. Додано власників полісів: {imported}.";
            await RefreshAnalyticsAsync();
        }
        catch (OperationCanceledException)
        {
            StatusTextBlock.Text = "Імпорт скасовано. Транзакцію не збережено.";
        }
        catch (Exception ex)
        {
            ShowError("Помилка імпорту JSON у базу даних", ex);
            StatusTextBlock.Text = "Імпорт не виконано.";
        }
        finally
        {
            SetImportState(isImporting: false);
            _importCancellation.Dispose();
            _importCancellation = null;
        }
    }

    private void CancelImportButton_Click(object sender, RoutedEventArgs e)
    {
        _importCancellation?.Cancel();
        StatusTextBlock.Text = "Надіслано запит на скасування імпорту...";
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshAnalyticsAsync();
            StatusTextBlock.Text = "Аналітику оновлено.";
        }
        catch (Exception ex)
        {
            ShowError("Не вдалося оновити аналітику", ex);
        }
    }

    private async Task RefreshAnalyticsAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        var holders = await db.PolicyHolders
            .AsNoTracking()
            .Include(holder => holder.Policies)
                .ThenInclude(policy => policy.Claims)
            .OrderBy(holder => holder.PolicyHolderName)
            .ToListAsync();

        CoverageBreachesGrid.ItemsSource =
            InsuranceAnalytics.FindCoverageBreaches(holders);
        InconsistentClaimsGrid.ItemsSource =
            InsuranceAnalytics.FindInconsistentClaims(holders);
        StatusStatisticsGrid.ItemsSource =
            InsuranceAnalytics.GetStatusStatistics(holders);
        MonthlyStatisticsGrid.ItemsSource =
            InsuranceAnalytics.GetMonthlyPayoutStatistics(holders);

        int policyCount = holders.Sum(holder => holder.Policies.Count);
        int claimCount = holders.Sum(holder => holder.Policies.Sum(policy => policy.Claims.Count));
        decimal approvedTotal = holders
            .SelectMany(holder => holder.Policies)
            .SelectMany(policy => policy.Claims)
            .Where(claim => string.Equals(claim.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            .Sum(claim => claim.PayoutAmount);

        SummaryTextBlock.Text =
            $"Власників: {holders.Count}  |  Полісів: {policyCount}  |  " +
            $"Страхових випадків: {claimCount}  |  Схвалені виплати: {approvedTotal:N2} грн";
    }

    private void SetImportState(bool isImporting)
    {
        ImportButton.IsEnabled = !isImporting;
        CancelImportButton.IsEnabled = isImporting;
    }

    private static void ShowError(string title, Exception exception)
    {
        MessageBox.Show(exception.Message, title,
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}