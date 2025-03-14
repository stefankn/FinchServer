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
    
    @Sendable func index(req: Request) async throws -> Page<ItemDTO> {
        var query = Item.query(on: req.db(.beets)).filter(\.$albumId == nil)
        
        if let albumType = req.query[AlbumType.self, at: "type"] {
            query = query.filter(\.$albumType == albumType.rawValue)
        } else {
            query = query.filter(\.$albumType == "")
        }
        
        let sorting: Sorting = req.query[Sorting.self, at: "sort"] ?? .added
        let sortOrder = req.query[SortingDirection.self, at: "direction"] ?? .ascending
        
        switch sorting {
        case .added:
            query = query.sort(\.$addedAt, sortOrder.direction)
        case .title:
            query = query.sort(\.$title, sortOrder.direction)
        case .artist:
            query = query.sort(\.$artist, sortOrder.direction)
        }
        
        return try await query.paginate(for: req).map{ ItemDTO($0) }
    }
    
    @Sendable func show(req: Request) async throws -> ItemDTO {
        guard let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        return ItemDTO(item, includeAlbumId: true)
    }
    
    @Sendable func update(req: Request) async throws -> ItemDTO {
        guard let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        let body = try req.content.decode(UpdateItemDTO.self)
        
        item.artist = body.artist
        item.artists = body.artists
        item.title = body.title
        try await item.save(on: req.db(.beets))
        
        return ItemDTO(item)
    }
    
    @Sendable func delete(req: Request) async throws -> HTTPStatus {
        guard let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        let path = item.path
        
        try await req.db(.beets).transaction { db in
            try await item.$attributes.get(on: db).delete(on: db)
            try await item.delete(on: db)
        }
        
        if let path, FileManager.default.fileExists(atPath: path) {
            try FileManager.default.removeItem(atPath: path)
        }
        
        return .noContent
    }
    
    @Sendable func stream(req: Request) async throws -> Response {
        guard
            let item = try await Item.find(req.parameters.get("id"), on: req.db(.beets)),
            let path = item.path else {
            
            throw Abort(.notFound)
        }
        
        return req.fileio.streamFile(at: path, mediaType: .audio)
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
            
            item.put(use: update)
                .openAPI(
                    summary: "Update an item by id",
                    query: .type(String.self),
                    body: .type(UpdateItemDTO.self),
                    response: .type(ItemDTO.self)
                )
            
            item.delete(use: delete)
                .openAPI(
                    summary: "Delete an item by id",
                    query: .type(String.self),
                    statusCode: .noContent
                )
            
            item.get("stream", use: stream)
                .openAPI(
                    summary: "Stream the audio of an item",
                    query: .type(String.self)
                )
        }
    }
}
