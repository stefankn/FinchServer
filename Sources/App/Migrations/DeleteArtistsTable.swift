//
//  DeleteArtistsTable.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 10/06/2025.
//

import Fluent
import FluentSQLiteDriver

struct DeleteArtistsTable: Migration {
    
    // MARK: - Functions
    
    // MARK: Migration Functions
    
    func prepare(on database: Database) -> EventLoopFuture<Void> {
        database.schema(Artist.schema).delete()
    }
    
    func revert(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(Artist.schema)
            .field("id", .int64, .identifier(auto: false))
            .field("name", .string, .required)
            .field("real_name", .string)
            .field("profile", .string)
            .field("image", .string)
            .field("urls", .array(of: .string))
            .field("created_at", .datetime, .required)
            .field("updated_at", .datetime, .required)
            .create()
    }
}
