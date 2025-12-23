using FinchServer.Beets;
using FinchServer.Components.Pages.Import;
using FinchServer.Database;
using FinchServer.Logging;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using ILogger = FinchServer.Logging.ILogger;
using ImportJob = FinchServer.Database.ImportJob;

namespace FinchServer.MusicImport;

public class MusicImporter(
    BeetsConfiguration beetsConfiguration,
    IWebHostEnvironment environment,
    IDbContextFactory<DataContext> dbContextFactory,
    ILogger logger
): IMusicImporter {
 
    // - Functions

    public async Task<ImportJob> Upload(IBrowserFile[] files) {
        var jobId = Guid.NewGuid().ToString();
        var jobPath = Path.Combine(environment.ContentRootPath, "Temp", "Import", jobId);
        Directory.CreateDirectory(jobPath);

        var uploadedFiles = new List<ImportFile>();

        foreach (var file in files) {
            logger.Information($"Uploading file {file.Name}...", LogCategory.Importer, consoleLog: true);
            var extension = Path.GetExtension(file.Name);
            if (!extension.Equals(".mp3", StringComparison.InvariantCultureIgnoreCase) &&
                !extension.Equals(".flac", StringComparison.InvariantCultureIgnoreCase)) {
                
                logger.Warning($"Skipping file {file.Name}, only .mp3 or .flac is supported.", LogCategory.Importer, consoleLog: true);
                uploadedFiles.Add(new ImportFile { Filename = file.Name, ErrorMessage = "File skipped, extension not supported" });
                continue;
            };
            
            var path  = Path.Combine(jobPath, file.Name);
            try {
                await using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(1024 * 1024 * 1024).CopyToAsync(fs);
                
                uploadedFiles.Add(new ImportFile { Filename = file.Name, Path = path });
                logger.Information($"File {file.Name} uploaded", LogCategory.Importer, consoleLog: true);
            } catch (Exception e) {
                logger.Error($"Error uploading file {path}", LogCategory.Importer, consoleLog: true);
                logger.Error(e.ToString(), LogCategory.Importer);
                uploadedFiles.Add(new ImportFile { Filename = file.Name, ErrorMessage = e.ToString() });
            }
        }

        var job = new ImportJob {
            Id = jobId,
            Path = jobPath,
            Files = uploadedFiles,
            CreatedAt = DateTime.UtcNow
        };
        var context = await dbContextFactory.CreateDbContextAsync();
        context.ImportJobs.Add(job);
        await context.SaveChangesAsync();

        return job;
    }

    public InteractiveCommand Import(ImportJob job, ImportConfiguration configuration) {
        if (job.Files.Count == 0) throw new Exception("No files found in job");

        // -t = Ask for confirmation when finding a metadata match
        var arguments = new List<string> {
            configuration.KeepExistingMetadata ? "-A" : "-t"
        };

        if (!configuration.KeepExistingMetadata && configuration.SearchId != null)
            arguments.Add($"--search-id {configuration.SearchId}");

        if (configuration.ImportAsSingleton)
            arguments.Add("-s");

        if (configuration.AlbumType != null) {
            arguments.Add($"--set albumtype=\"{configuration.AlbumType}\" --set albumtypes=\"{configuration.AlbumType}\"");
        }

        return beetsConfiguration.RunInteractiveCommand($"import {string.Join(" ", arguments)} {job.Path}");
    }
}