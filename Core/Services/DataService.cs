using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;

public class CsvDataService : IDataService
{
    private List<Metadata> _metadataList = new();
    private List<Stats> _statsList = new();
    private readonly CsvSettings _csvSettings;

    public CsvDataService(IOptions<CsvSettings> csvSettings)
    {
        _csvSettings = csvSettings.Value;
        ReadDataFromCsvFiles();
    }

    // Reads data from the CSV files at startup
    public void ReadDataFromCsvFiles()
    {
        // Read metadata.csv
        if (File.Exists(_csvSettings.MetadataFilePath))
        {
            using var reader = new StreamReader(_csvSettings.MetadataFilePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(), // Ignore case by converting headers to lowercase
                HeaderValidated = null, // Disable header validation to prevent exceptions for missing headers
                MissingFieldFound = null // Disable missing field validation to prevent exceptions for missing fields
            });
            _metadataList.AddRange(csv.GetRecords<Metadata>().ToList());
        }

        // Read stats.csv
        if (File.Exists(_csvSettings.StatsFilePath))
        {
            using var reader = new StreamReader(_csvSettings.StatsFilePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(), // Ignore case by converting headers to lowercase
                HeaderValidated = null, // Disable header validation to prevent exceptions for missing headers
                MissingFieldFound = null // Disable missing field validation to prevent exceptions for missing fields
            });
            _statsList.AddRange(csv.GetRecords<Stats>().ToList());
        }
    }

    // Retrieve metadata records
    public List<Metadata> GetMetadataRecords()
    {
        return _metadataList;
    }

    // Retrieve stats records
    public List<Stats> GetStatsRecords()
    {
        return _statsList;
    }

    public List<MovieStats> GetMovieStats()
    {
        var result = _metadataList
            .GroupJoin(
                _statsList,
                metadata => metadata.MovieId,
                stats => stats.MovieId,
                (metadata, statsGroup) => new
                {
                    Metadata = metadata,
                    StatsGroup = statsGroup
                }
            )
            .Select(x => new MovieStats
            {
                MovieId = x.Metadata.MovieId,
                Title = x.Metadata.Title,
                ReleaseYear = x.Metadata.ReleaseYear,
                AverageWatchDurationS = x.StatsGroup.Any() ? (int)x.StatsGroup.Average(s => s.WatchDurationMs / 1000) : 0,
                Watches = x.StatsGroup.Count()
            })
            .ToList();

        return result;
    }

    // Writes a Movie record to the database CSV file
    public void WriteMovieToDatabase(Movie movie)
    {
        bool fileExists = File.Exists(_csvSettings.DatabaseFilePath);

        using var writer = new StreamWriter(_csvSettings.DatabaseFilePath, append: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        if (!fileExists)
        {
            csv.WriteHeader<Movie>();
            csv.NextRecord();
        }

        csv.WriteRecord(movie);
        csv.NextRecord();
    }

    // Generic method to write data back to a CSV file
    private void WriteDataToCsv<T>(string csvFilePath, List<T> records)
    {
        using var writer = new StreamWriter(csvFilePath);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.WriteRecords(records);
    }
}
