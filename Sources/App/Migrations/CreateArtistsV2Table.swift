//
//  CreateArtistsV2Table.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 10/06/2025.
//

import Fluent
import FluentSQLiteDriver

struct CreateArtistsV2Table: Migration {
    
    // MARK: - Functions
    
    // MARK: Migration Functions
    
    func prepare(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(Artist.schema)
            .field("id", .string, .identifier(auto: false))
            .field("name", .string, .required)
            .field("created_at", .datetime, .required)
            .field("updated_at", .datetime, .required)
            .create()
    }
    
    func revert(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(Artist.schema)
            .delete()
    }
}
