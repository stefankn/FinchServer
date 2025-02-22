import NIOSSL
import Fluent
import FluentSQLiteDriver
import Vapor

public func configure(_ app: Application) async throws {

    guard let beetsExecutablePath = Environment.get("BEETS_EXECUTABLE") else {
        throw BeetsError.missingBeetsExecutableEnvironmentVariable
    }
    
    app.logger.notice("Beets executable path: \(beetsExecutablePath)")
    
    let beetsConfigLocation = try BeetsCLI.getBeetsConfigLocation(executablePath: beetsExecutablePath)
    
    app.logger.notice("Beets config location: \(beetsConfigLocation)")
    
    let beetsDatabaseLocation = try BeetsCLI.getBeetsDatabaseLocation(configURL: beetsConfigLocation)
    
    app.logger.notice("Beets database location: \(beetsDatabaseLocation)")
    
    app.beetsConfiguration = BeetsConfiguration(
        executablePath: beetsExecutablePath,
        configLocation: beetsConfigLocation,
        databaseLocation: beetsDatabaseLocation
    )
    
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file("db.sqlite")), as: .main)
    app.databases.use(DatabaseConfigurationFactory.sqlite(.file(beetsDatabaseLocation.path())), as: .beets)
    
    app.migrations.add(CreatePlaylistsTable(), to: .main)
    app.migrations.add(CreatePlaylistEntriesTable(), to: .main)
    
    app.middleware.use(FileMiddleware(publicDirectory: app.directory.publicDirectory))
    app.routes.defaultMaxBodySize = "5mb"
    
    app.http.server.configuration.hostname = "0.0.0.0"
    app.http.server.configuration.port = 25520

    try routes(app)
}
