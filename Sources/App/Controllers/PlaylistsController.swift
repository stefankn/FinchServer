//
//  PlaylistsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 16/02/2025.
//

import Vapor
import Fluent

struct PlaylistsController: RouteCollection {
    
    // MARK: - Functions
    
    @Sendable func index(req: Request) async throws -> [PlaylistDTO] {
        try await Playlist.query(on: req.db(.main)).all().map(PlaylistDTO.init)
    }
    
    @Sendable func show(req: Request) async throws -> PlaylistDTO {
        if let playlist = try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) {
            return PlaylistDTO(playlist)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func entries(req: Request) async throws -> [PlaylistEntryDTO] {
        guard let playlist = try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) else {
            throw Abort(.notFound)
        }
        
        let entries = try await playlist.$entries.get(on: req.db(.main))
        let items = try await Item.query(on: req.db(.beets)).filter(\.$id ~~ entries.map{ $0.itemId }).all()
        
        return entries.compactMap{ entry in PlaylistEntryDTO(entry, item: items.first{ $0.id == entry.itemId }) }
    }
    
    @Sendable func createEntry(req: Request) async throws -> PlaylistEntryDTO {
        guard let playlist = try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) else {
            throw Abort(.notFound)
        }
        
        let body = try req.content.decode(CreatePlaylistEntryDTO.self)
        
        guard let item = try await Item.find(body.itemId, on: req.db(.beets)) else {
            throw Abort(.notFound)
        }
        
        var index = 0
        if let maxIndex = try await playlist.$entries.query(on: req.db(.main)).max(\.$index){
            index = maxIndex + 1
        }
        
        let playlistEntry = try PlaylistEntry(item: item, index: index)
        try await playlist.$entries.create(playlistEntry, on: req.db(.main))
        
        return PlaylistEntryDTO(playlistEntry, item: item)
    }
    
    @Sendable func deleteEntry(req: Request) async throws -> HTTPStatus {
        guard
            try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) != nil,
            let playlistEntry = try await PlaylistEntry.find(req.parameters.get("entryId"), on: req.db(.main)) else {
            
            throw Abort(.notFound)
        }
        
        try await playlistEntry.delete(on: req.db(.main))

        return .noContent
    }
    
    @Sendable func create(req: Request) async throws -> PlaylistDTO {
        let body = try req.content.decode(CreatePlaylistDTO.self)
        let playlist = Playlist(body)

        try await playlist.save(on: req.db(.main))
        
        if let itemIds = body.items {
            let items = try await Item.query(on: req.db(.beets)).filter(\.$id ~~ itemIds).all()
            var entries: [PlaylistEntry] = []
            
            for (index, item) in items.enumerated() {
                entries.append(try .init(item: item, index: index))
            }
            
            try await playlist.$entries.create(entries, on: req.db(.main))
        }
        
        return PlaylistDTO(playlist)
    }
    
    @Sendable func uploadImage(req: Request) async throws -> PlaylistDTO {
        guard let playlist = try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) else {
            throw Abort(.notFound)
        }
        
        let id = try playlist.requireID()
        
        let filename = "playlist_\(id).jpg"
        let path = req.application.directory.publicDirectory + "images/playlist/\(filename)"
        let data = try await req.body.collect().unwrap(or: Abort(.badRequest)).get()
        try await req.fileio.writeFile(data, at: path)
        
        playlist.image = filename
        try await playlist.save(on: req.db(.main))
        
        return PlaylistDTO(playlist)
    }
    
    @Sendable func delete(req: Request) async throws -> HTTPStatus {
        guard let playlist = try await Playlist.find(req.parameters.get("id"), on: req.db(.main)) else {
            throw Abort(.notFound)
        }
        
        try await playlist.delete(on: req.db(.main))
        
        return .noContent
    }
    
    
    // MARK: RouteCollection Functions
    
    func boot(routes: any Vapor.RoutesBuilder) throws {
        let playlists = routes.grouped("api", "v1", "playlists")
        
        playlists
            .get(use: index)
            .openAPI(
                summary: "List all playlists",
                response: .type([PlaylistDTO].self)
            )
        
        playlists
            .post(use: create)
            .openAPI(
                summary: "Create a new playlist",
                body: .type(CreatePlaylistDTO.self),
                response: .type(PlaylistDTO.self)
            )
        
        playlists.group(":id") { playlist in
            playlist.get(use: show)
                .openAPI(
                    summary: "Get a playlist by id",
                    query: .type(String.self),
                    response: .type(PlaylistDTO.self)
                )
            
            playlist.post(use: uploadImage)
                .openAPI(
                    summary: "Upload playlist image",
                    query: .type(String.self),
                    response: .type(PlaylistDTO.self)
                )
            
            playlist.delete(use: delete)
                .openAPI(
                    summary: "Delete a playlist by id",
                    query: .type(String.self),
                    statusCode: .noContent
                )
            
            playlist.group("entries") { entries in
                entries.post(use: createEntry)
                    .openAPI(
                        summary: "Add an item to a playlist",
                        query: .type(String.self),
                        body: .type(CreatePlaylistEntryDTO.self),
                        response: .type(PlaylistEntryDTO.self)
                    )
                
                entries.delete(":entryId", use: deleteEntry)
                    .openAPI(
                        summary: "Delete an entry from a playlist",
                        query: .type(String.self),
                        statusCode: .noContent
                    )
                
                entries.get(use: self.entries)
                    .openAPI(
                        summary: "Get the entries for a playlist",
                        query: .type(String.self),
                        response: .type([PlaylistEntryDTO].self)
                    )
            }
        }
    }
}
