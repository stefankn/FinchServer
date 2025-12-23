using System.Diagnostics;
using System.Text;

namespace FinchServer.Beets;

public class InteractiveCommand {
    
    // - Types
    
    public event EventHandler<string>? OutputReceived;
    public event EventHandler<int>? ProcessExited;
    
    
    // - Private Properties
    
    private readonly StringBuilder _output = new();
    private readonly Process _process;
    
    
    // - Properties
    
    public string Command { get; init; }
    public bool IsRunning => !_process.HasExited;
    
    
    // - Construction

    public InteractiveCommand(string executablePath, string arguments) {
        Command = $"{executablePath} {arguments}";
        
        var process = new Process();
        process.StartInfo.FileName = executablePath;
        process.StartInfo.Arguments = arguments;
        
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        
        _process = process;
        
        // Monitor when process exits
        process.EnableRaisingEvents = true;
        process.Exited += (sender, e) => {
            Console.WriteLine("process exited, code: {0}", process.ExitCode);
            ProcessExited?.Invoke(this, process.ExitCode);
        };
        process.Start();

        // Read stdout character by character
        Task.Run(async () => {
            var buffer = new char[1];
            var lineBuffer = new StringBuilder();
            
            while (true) {
                try {
                    var count = await process.StandardOutput.ReadAsync(buffer, 0, 1);
                    if (count == 0) break;
            
                    var ch = buffer[0];
            
                    if (ch == '\n') {
                        var line = lineBuffer.ToString().TrimEnd('\r');
                        _output.AppendLine(line);
                        OutputReceived?.Invoke(this, line);
                        lineBuffer.Clear();
                    } else if (ch == '\r') {
                        continue; // Skip carriage returns
                    } else {
                        lineBuffer.Append(ch);
                
                        // Only check for '?' (prompts typically end with '?')
                        if (ch == '?') {
                            var line = lineBuffer.ToString();
                            _output.Append(line);
                            OutputReceived?.Invoke(this, line);
                            lineBuffer.Clear();
                        }
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"Error reading output: {ex.Message}");
                    break;
                }
            }
            
            // Flush any remaining content
            if (lineBuffer.Length > 0) {
                var line = lineBuffer.ToString();
                _output.Append(line);
                OutputReceived?.Invoke(this, line);
            }
        });
        
        Task.Run(() => {
            while (process.StandardError.ReadLine() is { } line) {
                _output.AppendLine(line);
                OutputReceived?.Invoke(this, line);
            }
        });
    }
    
    
    // - Functions
    
    public async Task SendInputAsync(string input) {
        if (_process == null || _process.HasExited) {
            throw new InvalidOperationException("Process is not running");
        }
        
        if (_process.StandardInput.BaseStream.CanWrite) {
            await _process.StandardInput.WriteLineAsync(input);
            await _process.StandardInput.FlushAsync();
        }
    }
}