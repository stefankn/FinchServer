import NIOSSL
import Fluent
import FluentSQLiteDriver
import Vapor

public func configure(_ app: Application) async throws {

    guard let beetsExecutablePath = Environment.get("BEETS_EXECUTABLE") else {
        throw BeetsError.missingBeetsExecutableEnvironmentVariable
    }
    
    let beetsConfigLocation = try BeetsCLI.getBeetsConfigLocation(executablePath: beetsExecutablePath)
    let beetsDatabaseLocation = try BeetsCLI.getBeetsDatabaseLocation(configURL: beetsConfigLocation)
    
    app.beetsConfiguration = BeetsConfiguration(
        executablePath: beetsExecutablePath,
        configLocation: beetsConfigLocation,
        databaseLocation: beetsDatabaseLocation
    )
    
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file("db.sqlite")), as: .main)
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file(beetsDatabaseLocation.path())), as: .beets)
    
    app.middleware.use(FileMiddleware(publicDirectory: app.directory.publicDirectory))

    try routes(app)
}
