//
//  AlbumsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 25/01/2025.
//

import Vapor
import Fluent
import SwiftGD

struct AlbumsController: RouteCollection {
    
    // MARK: - Functions
    
    @Sendable func index(req: Request) async throws -> Page<AlbumDTO> {
        var query = Album.query(on: req.db(.beets)).with(\.$attributes)
        
        let sorting: Sorting = req.query[Sorting.self, at: "sort"] ?? .added
        let sortOrder = req.query[SortingDirection.self, at: "direction"] ?? .ascending
        
        switch sorting {
        case .added:
            query = query.sort(\.$addedAt, sortOrder.direction)
        case .title:
            query = query.sort(\.$title, sortOrder.direction)
        case .artist:
            query = query.sort(\.$albumArtistSortKey, sortOrder.direction)
        }
        
        return try await query.paginate(for: req).map{ AlbumDTO($0) }
    }
    
    @Sendable func show(req: Request) async throws -> AlbumDTO {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        // Eager load the items and attributes
        _ = try await album.$attributes.get(on: req.db(.beets))
        let items = try await album.$items.get(on: req.db(.beets))
        
        return AlbumDTO(album, items: items)
    }
    
    @Sendable func artwork(req: Request) async throws -> Response {
        guard
            let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)),
            let artworkPath = album.artworkPath else {
            
            throw Abort(.notFound)
        }
        
        return req.fileio.streamFile(at: artworkPath)
    }
    
    @Sendable func artworkThumbnail(req: Request) async throws -> Response {
        guard
            let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)),
            let artworkThumbnailPath = album.artworkThumbnailPath(req: req) else {
            
            throw Abort(.notFound)
        }
        
        return req.fileio.streamFile(at: artworkThumbnailPath)
    }
    
    @Sendable func items(req: Request) async throws -> [ItemDTO] {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        return try await album.$items.get(on: req.db(.beets)).map{ ItemDTO($0, includeAlbumId: true) }
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
