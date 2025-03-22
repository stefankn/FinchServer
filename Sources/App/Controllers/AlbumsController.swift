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
        
        if let type = req.query[AlbumType.self, at: "type"] {
            query = query.filter(\.$albumType =~ type.rawValue)
        }
        
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
    
    @Sendable func path(req: Request) async throws -> AlbumPathDTO {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        if let path = try await resolvePath(for: album, db: req.db(.beets))?.path {
            return AlbumPathDTO(path: path)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func items(req: Request) async throws -> [ItemDTO] {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        return try await album.$items.get(on: req.db(.beets)).map{ ItemDTO($0, includeAlbumId: true) }
    }
    
    @Sendable func update(req: Request) async throws -> AlbumDTO {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        let body = try req.content.decode(UpdateAlbumDTO.self)
        
        var thumbnailFilename: String?
        if album.artworkPath != body.artworkPath {
            thumbnailFilename = album.artworkThumbnailFilename
        }
        
        album.albumArtist = body.artist
        album.title = body.title
        album.artworkPath = body.artworkPath
        
        if let thumbnailFilename {
            let thumbnailPath = req.application.directory.thumbnailsDirectory + thumbnailFilename
            if FileManager.default.fileExists(atPath: thumbnailPath) {
                try FileManager.default.removeItem(atPath: thumbnailPath)
            }
        }
        
        try await album.save(on: req.db(.beets))
        _ = try await album.$attributes.get(on: req.db(.beets))
        
        return AlbumDTO(album)
    }
    
    @Sendable func delete(req: Request) async throws -> HTTPStatus {
        guard let album = try await Album.find(req.parameters.get("id"), on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        let body = try req.content.decode(DeleteAlbumDTO.self)
        
        var thumbnailPath: String?
        if let artworkThumbnailFilename = album.artworkThumbnailFilename {
            thumbnailPath = req.application.directory.thumbnailsDirectory + artworkThumbnailFilename
        }
        
        let albumPath = try await resolvePath(for: album, db: req.db(.beets))
        
        try await req.db(.beets).transaction { db in
            let items = try await album.$items.get(on: db)
            
            for item in items.sorted() {
                try await item.$attributes.get(on: db).delete(on: db)
            }
            
            try await items.delete(on: db)
            try await album.$attributes.get(on: db).delete(on: db)
            try await album.delete(on: db)
        }
        
        let fileManager = FileManager.default
        
        if let thumbnailPath, fileManager.fileExists(atPath: thumbnailPath) {
            try fileManager.removeItem(atPath: thumbnailPath)
        }
        
        if body.deleteFiles, let albumPath {
            try fileManager.removeItem(at: albumPath)
        }
        
        return .noContent
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
            
            album.get("path", use: path)
                .openAPI(
                    summary: "Get the file path of an album",
                    query: .type(String.self),
                    response: .type(AlbumPathDTO.self)
                )
            
            album.put(use: update)
                .openAPI(
                    summary: "Update an album by id",
                    query: .type(String.self),
                    body: .type(UpdateAlbumDTO.self),
                    response: .type(AlbumDTO.self)
                )
            
            album.delete(use: delete)
                .openAPI(
                    summary: "Delete an album by id",
                    query: .type(String.self),
                    statusCode: .noContent
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
    
    
    
    // MARK: - Private Functions
    
    private func resolvePath(for album: Album, db: any Database) async throws -> URL? {
        var url: URL?
        
        if let artworkURL = album.artworkURL {
            url = artworkURL
        }
        
        if let item = try await album.$items.get(on: db).sorted().first, let path = item.path {
            url = URL(fileURLWithPath: path)
        }
        
        return url?.deletingLastPathComponent()
    }
}
