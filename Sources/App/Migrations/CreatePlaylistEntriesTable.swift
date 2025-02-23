//
//  CreatePlaylistEntriesTable.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 22/02/2025.
//

import Fluent
import FluentSQLiteDriver

struct CreatePlaylistEntriesTable: Migration {
    
    // MARK: - Functions
    
    // MARK: Migration Functions
    
    func prepare(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(PlaylistEntry.schema)
            .field("id", .int64, .identifier(auto: true))
            .field("index", .int64, .required)
            .field("playlist_id", .int64, .required, .references("playlists", "id", onDelete: .cascade))
            .field("item_id", .int64, .required)
            .field("created_at", .datetime, .required)
            .create()
    }
    
    func revert(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(PlaylistEntry.schema)
            .delete()
    }
}
