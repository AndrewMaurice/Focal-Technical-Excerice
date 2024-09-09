using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<CsvSettings>(builder.Configuration.GetSection("CsvSettings"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register CsvDataService
builder.Services.AddSingleton<IDataService, CsvDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Focal technical Task");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.MapPost("/metadata", (Movie movie, IDataService dataService) => {
    dataService.WriteMovieToDatabase(movie);
    return Results.Ok(new { message = "Movie has been addedd successfully" });
})
.WithName("PostMetadata")
.WithOpenApi();

app.MapGet("/metadata/{movieId:int}", (int movieId, IDataService dataService) => {
    var records = dataService.GetMetadataRecords()
    .OrderByDescending(m => m.Id)
    .Where(m => m.MovieId == movieId)
    .DistinctBy(m => m.Language);
    
    if(!records.Any()) {
        return Results.NotFound();
    }

    return Results.Ok(
        records
        .Select(m => new {MovieId = m.MovieId, Title = m.Title, Language = m.Language, Duration = m.Duration, ReleaseYear = m.ReleaseYear})
        .OrderBy(m => m.Language)
        );
})
.WithName("GetMetadataByMovieId")
.WithOpenApi();


app.MapGet("/movies/stats", (IDataService dataService) => {
    return Results.Ok(dataService.GetMovieStats().DistinctBy(m => m.MovieId).OrderByDescending(m => m.Watches));
    
})
.WithName("GetMoviewsStats")
.WithOpenApi();



app.Run();
