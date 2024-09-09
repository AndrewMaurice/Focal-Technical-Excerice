public interface IDataService
{
    void ReadDataFromCsvFiles();
    List<Metadata> GetMetadataRecords();
    List<Stats> GetStatsRecords();
    void WriteMovieToDatabase(Movie movie);
    List<MovieStats> GetMovieStats();
}
