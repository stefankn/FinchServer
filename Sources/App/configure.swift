import NIOSSL
import Fluent
import FluentSQLiteDriver
import Vapor

enum BeetsError: Error {
    case missingBeetsExecutableEnvironmentVariable
    case resolveConfigLocationFailure
    case readConfigFailure
}

// configures your application
public func configure(_ app: Application) async throws {
    // uncomment to serve files from /Public folder
    // app.middleware.use(FileMiddleware(publicDirectory: app.directory.publicDirectory))
    
    let beetsConfigLocation = try getBeetsConfigLocation()
    let beetsDatabaseLocation = try getBeetsDatabaseLocation(configURL: beetsConfigLocation)
    
    app.beetsConfiguration = BeetsConfiguration(
        configLocation: beetsConfigLocation,
        databaseLocation: beetsDatabaseLocation
    )
    
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file("db.sqlite")), as: .main)
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file(beetsDatabaseLocation.path())), as: .beets)
    
    app.middleware.use(FileMiddleware(publicDirectory: app.directory.publicDirectory))

    // register routes
    try routes(app)
}

private func getBeetsConfigLocation() throws -> URL {
    guard let beetsExecutable = Environment.get("BEETS_EXECUTABLE") else { throw BeetsError.missingBeetsExecutableEnvironmentVariable }
    
    if
        let output = try command("\(beetsExecutable) config -p"),
        let url = URL(string: output.trimmingCharacters(in: .whitespacesAndNewlines)) {
        
        return url
    } else {
        throw BeetsError.resolveConfigLocationFailure
    }
}

private func getBeetsDatabaseLocation(configURL: URL) throws -> URL {
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

private func getUserHomeDirectoryPath() throws -> String {
    if let userHomeDirectoryPath = try command("cd ~ && pwd") {
        return userHomeDirectoryPath.trimmingCharacters(in: .whitespacesAndNewlines)
    }
    
    throw BeetsError.readConfigFailure
}

private func command(_ command: String) throws -> String? {
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
