namespace FinchServer.Utilities;

public static class DotEnv {
    
    // - Functions
    
    public static void Load() {
        var path = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(path)) {
            return;
        }
            

        foreach (var line in File.ReadAllLines(path)) {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}