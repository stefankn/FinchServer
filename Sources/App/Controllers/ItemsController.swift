//
//  ItemsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor
import Fluent

struct ItemsController: RouteCollection {
    
    // MARK: - Functions
    
    @Sendable func index(req: Request) async throws -> [ItemDTO] {
        try await Item.query(on: req.db(.beets)).filter(\.$albumId == nil).all().map{ ItemDTO($0) }
    }
    
    @Sendable func show(req: Request) async throws -> ItemDTO {
        if let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)) {
            return ItemDTO(item, includeAlbumId: true)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func stream(req: Request) async throws -> Response {
        if
            let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)),
            let path = item.path {
            
            return req.fileio.streamFile(at: path, mediaType: .audio)
        }
        
        throw Abort(.notFound)
    }
    
    
    // MARK: RouteCollection Functions
    
    func boot(routes: any RoutesBuilder) throws {
        let items = routes.grouped("api", "v1", "items")
        
        items
            .get(use: index)
            .openAPI(
                summary: "List all singleton items",
                response: .type([ItemDTO].self)
            )
        
        items.group(":id") { item in
            item.get(use: show)
                .openAPI(
                    summary: "Get an item by id",
                    query: .type(String.self),
                    response: .type(ItemDTO.self)
                )
            
            item.get("stream", use: stream)
                .openAPI(
                    summary: "Stream the audio of an item",
                    query: .type(String.self)
                )
        }
    }
}
