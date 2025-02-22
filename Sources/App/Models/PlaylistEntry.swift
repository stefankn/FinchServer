//
//  PlaylistEntry.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 22/02/2025.
//

import Vapor
import Fluent

final class PlaylistEntry: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "playlist_entries"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "index")
    var index: Int
    
    @Parent(key: "playlist_id")
    var playlist: Playlist
    
    @Field(key: "item_id")
    var itemId: Int
    
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    
    
    // MARK: - Construction
    
    init() {}
    
    init(item: Item, index: Int) throws {
        itemId = try item.requireID()
        self.index = index
    }
}
