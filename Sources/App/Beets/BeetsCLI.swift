//
//  BeetsCLI.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor

struct BeetsCLI {
    
    // MARK: - Properties
    
    let configuration: BeetsConfiguration
    
    
    
    // MARK: - Functions
    
    func getStats() throws -> StatsDTO {
        guard let output = try Self.command("\(configuration.executablePath) stats") else { throw BeetsError.commandFailure }
        
        let lines = output.split(separator: "\n")
        let entries: [String: String] = lines.reduce(into: [:]) { partialResult, line in
            let splitted = line.split(separator: ":")
            return partialResult[splitted[0].trimmingCharacters(in: .whitespaces)] = splitted[1].trimmingCharacters(in: .whitespaces)
        }
        
        return StatsDTO(
            trackCount: Int(entries["Tracks"] ?? "") ?? 0,
            totalTime: entries["Total time"] ?? "",
            approximateTotalSize: entries["Approximate total size"] ?? "",
            artistCount: Int(entries["Artists"] ?? "") ?? 0,
            albumCount: Int(entries["Albums"] ?? "") ?? 0,
            albumArtistCount: Int(entries["Album artists"] ?? "") ?? 0
        )
    }
    
    static func getBeetsConfigLocation(executablePath: String) throws -> URL {
        if
            let output = try command("\(executablePath) config -p"),
            let firstEntry = output.split(separator: "\n").first,
            let url = URL(string: firstEntry.trimmingCharacters(in: .whitespacesAndNewlines)) {
            
            return url
        } else {
            throw BeetsError.resolveConfigLocationFailure
        }
    }
    
    static func getBeetsDatabaseLocation(configURL: URL) throws -> URL {
        if
            let output = try command("cat \(configURL)"),
            let databaseLocationLine = output.split(separator: "\n").first(where: { $0.starts(with: "library:") }) {
            
            var databaseLocation = databaseLocationLine
                .trimmingPrefix("library:")
                .trimmingCharacters(in: .whitespacesAndNewlines)
            
            if databaseLocation.starts(with: "~") {
                let homeDirectoryPath = try getUserHomeDirectoryPath()
                databaseLocation = "\(homeDirectoryPath)\(databaseLocation.dropFirst())"
            }
            
            if let url = URL(string: databaseLocation) {
                return url
            }
        }
        
        throw BeetsError.readConfigFailure
    }
    
    
    
    // MARK: - Private Functions
    
    private static func getUserHomeDirectoryPath() throws -> String {
        if let userHomeDirectoryPath = try command("cd ~ && pwd") {
            return userHomeDirectoryPath.trimmingCharacters(in: .whitespacesAndNewlines)
        }
        
        throw BeetsError.readConfigFailure
    }
    
    private static func command(_ command: String) throws -> String? {
        let task = Process()
        let pipe = Pipe()
        
        task.standardOutput = pipe
        task.standardError = pipe
        task.arguments = ["-c", command]
        task.executableURL = URL(fileURLWithPath: "/bin/zsh")
        task.standardInput = nil
        
        try task.run()
        
        if let data = try pipe.fileHandleForReading.readToEnd() {
            return String(data: data, encoding: .utf8)
        }
        
        return nil
    }

}

extension Request {
    
    // MARK: - Properties
    
    var beetsCLI: BeetsCLI { .init(configuration: application.beetsConfiguration) }
}
