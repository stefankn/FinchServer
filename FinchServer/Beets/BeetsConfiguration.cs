using System.Diagnostics;
using FinchServer.Controllers.DTO;

namespace FinchServer.Beets;

public class BeetsConfiguration {
    
    // - Properties
    
    public string ExecutablePath { get; }
    public string ConfigPath { get; }
    public string DatabasePath { get; }
    
    
    // - Construction

    public BeetsConfiguration(IConfiguration configuration) {
        var executablePath = configuration["BEETS_EXECUTABLE"];
        if (executablePath == null) {
            throw new Exception("BEETS_EXECUTABLE environment variable not set");
        }
        
        if (executablePath.StartsWith("~")) {
            executablePath = executablePath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        ExecutablePath = executablePath;
        ConfigPath = GetConfigPath();
        DatabasePath = GetDatabasePath();
    }
    
    
    // - Functions

    public StatsDto GetStats() {
        var output = RunCommand("stats");
        
        var stats = new Dictionary<string, string>();
        foreach (var line in output.Split('\n')) {
            if (line.Trim() == string.Empty) continue;
            
            var splitted = line.Split(":");
            stats[splitted[0].Trim()] = splitted[1].Trim();
        }

        return new StatsDto {
            TrackCount = int.Parse(stats["Tracks"]),
            TotalTime = stats["Total time"],
            ApproximateTotalSize = stats["Approximate total size"],
            ArtistCount = int.Parse(stats["Artists"]),
            AlbumCount = int.Parse(stats["Albums"]),
            AlbumArtistCount = int.Parse(stats["Album artists"]),
        };
    }

    public string RunCommand(string command) {
        var process = Run(ExecutablePath, command);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        return process.ExitCode != 0 ? throw new Exception(error) : output;
    }
    
    
    // - Private Functions

    private string GetConfigPath() {
        var output = RunCommand("config -p");
        return output.Trim();
    }

    private string GetDatabasePath() {
        var config = File.ReadAllText(ConfigPath);
        var libraryPath = config
            .Split('\n')
            .FirstOrDefault(l => l.StartsWith("library:"))?
            .Replace("library:", "")
            .Trim();
        
        if (libraryPath == null) {
            throw new Exception("Could not find library path in configuration file");
        }

        if (libraryPath.StartsWith("~")) {
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            libraryPath = libraryPath.Replace("~", homeDirectory);
        }

        return libraryPath;
    }

    private static Process Run(string executable, string arguments = "") {
        var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.Arguments = arguments;
        
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.WaitForExit();

        return process;
    }
}