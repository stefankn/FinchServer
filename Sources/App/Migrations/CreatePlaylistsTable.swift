//
//  CreatePlaylistsTable.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 21/02/2025.
//

import Fluent
import FluentSQLiteDriver

struct CreatePlaylistsTable: Migration {
    
    // MARK: - Functions
    
    // MARK: Migration Functions
    
    func prepare(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(Playlist.schema)
            .field("id", .int64, .identifier(auto: true))
            .field("name", .string, .required)
            .field("description", .string)
            .field("image", .string)
            .field("created_at", .datetime, .required)
            .field("updated_at", .datetime, .required)
            .create()
    }
    
    func revert(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(Playlist.schema)
            .delete()
    }
}
