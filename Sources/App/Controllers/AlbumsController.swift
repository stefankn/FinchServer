//
//  AlbumsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 25/01/2025.
//

import Vapor
import SwiftGD

struct AlbumsController: RouteCollection {
    
    // MARK: - Functions
    
    @Sendable func index(req: Request) async throws -> [AlbumDTO] {
        try await Album.query(on: req.db(.beets)).with(\.$attributes).all().map{ AlbumDTO($0) }
    }
    
    @Sendable func show(req: Request) async throws -> AlbumDTO {
        if let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) {
            
            // Eager load the items and attributes
            _ = try await album.$attributes.get(on: req.db(.beets))
            let items = try await album.$items.get(on: req.db(.beets))
            
            return AlbumDTO(album, items: items)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func artwork(req: Request) async throws -> Response {
        if
            let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)),
            let artworkPath = album.artworkPath {
            
            return req.fileio.streamFile(at: artworkPath)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func artworkThumbnail(req: Request) async throws -> Response {
        if
            let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)),
            let artworkThumbnailPath = album.artworkThumbnailPath(req: req) {
            
            return req.fileio.streamFile(at: artworkThumbnailPath)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func items(req: Request) async throws -> [ItemDTO] {
        if let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) {
            return try await album.$items.get(on: req.db(.beets)).map{ ItemDTO($0) }
        }
        
        throw Abort(.notFound)
    }
    
    
    // MARK: RouteCollection Functions
    
    func boot(routes: any RoutesBuilder) throws {
        let albums = routes.grouped("api", "v1", "albums")
        
        albums
            .get(use: index)
            .openAPI(
                summary: "List all albums",
                response: .type([AlbumDTO].self)
            )
        
        albums.group(":id") { album in
            album.get(use: show)
                .openAPI(
                    summary: "Get an album by id",
                    query: .type(String.self),
                    response: .type(AlbumDTO.self)
                )
            
            album.group("artwork") { artwork in
                artwork.get(use: self.artwork)
                    .openAPI(
                        summary: "Get artwork for an album",
                        query: .type(String.self)
                    )
                
                artwork.get("thumbnail", use: artworkThumbnail)
                    .openAPI(
                        summary: "Get a thumbnail version of the artwork for an album",
                        query: .type(String.self)
                    )
            }
            
            album.group("items") { items in
                items.get(use: self.items)
                    .openAPI(
                        summary: "Get the items for an album",
                        query: .type(String.self),
                        response: .type(ItemDTO.self)
                    )
            }
        }
    }
}
