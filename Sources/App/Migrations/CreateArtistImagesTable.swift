//
//  CreateArtistImagesTable.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 10/06/2025.
//

import Fluent
import FluentSQLiteDriver

struct CreateArtistImagesTable: Migration {
    
    // MARK: - Functions
    
    // MARK: Migration Functions
    
    func prepare(on database: Database) -> EventLoopFuture<Void> {
        database.enum("image_type")
            .case("background")
            .case("image")
            .case("thumbnail")
            .case("musicLogo")
            .create()
            .flatMap { imageType in
                return database
                    .schema(ArtistImage.schema)
                    .field("id", .int64, .identifier(auto: true))
                    .field("image_type", imageType, .required)
                    .field("filename", .string, .required)
                    .field("artist_id", .int64, .required, .references("artists", "id", onDelete: .cascade))
                    .create()
            }
    }
    
    func revert(on database: Database) -> EventLoopFuture<Void> {
        database
            .schema(ArtistImage.schema)
            .delete().flatMap {
                return database.enum("image_type").delete()
            }
    }
}
