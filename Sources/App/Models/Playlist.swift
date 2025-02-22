//
//  Playlist.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 16/02/2025.
//

import Vapor
import Fluent

final class Playlist: Model, @unchecked Sendable {

    // MARK: - Constants
    
    static let schema = "playlists"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "name")
    var name: String
    
    @Field(key: "description")
    var description: String?
    
    @Field(key: "image")
    var image: String?
    
    @Children(for: \.$playlist)
    var entries: [PlaylistEntry]
    
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?
    
    
    
    // MARK: - Construction
    
    init(_ playlist: CreatePlaylistDTO) {
        name = playlist.name
        description = playlist.description
        image = nil
    }
    
    init() {}
}
