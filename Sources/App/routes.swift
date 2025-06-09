import Fluent
import Vapor
import VaporToOpenAPI

func routes(_ app: Application) throws {
    try app.register(collection: AlbumsController())
    try app.register(collection: ItemsController())
    try app.register(collection: PlaylistsController())
    try app.register(collection: ArtistsController())
    
    app
        .get("api", "v1", "stats") { req in
            return try req.beetsCLI.getStats()
        }
        .openAPI(
            summary: "Get statistics about the beets library",
            response: .type(StatsDTO.self)
        )
    
    app.get("api", "docs") { req in
        req.redirect(to: "/api/docs/index.html")
    }
    .excludeFromOpenAPI()
    
    app.get("openapi", "openapi.json") { req in
        req.application.routes.openAPI(
            info: .init(
                title: "Finch Server",
                description: "API for interacting with a beets library",
                version: "1.0.0"
            )
        )
    }
    .excludeFromOpenAPI()
}
