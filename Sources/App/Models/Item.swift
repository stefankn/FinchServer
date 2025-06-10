//
//  Item.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor
import Fluent

final class Item: Model, @unchecked Sendable, Comparable {
    
    // MARK: - Constants
    
    static let schema = "items"
    
    
    // MARK: - Private Properties
    
    private var orderIndex: Int {
        let disc = disc ?? 1
        return disc * 1000 + (track ?? 0)
    }
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "track")
    var track: Int?
    
    @Field(key: "disc")
    var disc: Int?
    
    @Field(key: "title")
    var title: String
    
    @Field(key: "artist")
    var artist: String
    
    @Field(key: "artists")
    var artists: String
    
    @Field(key: "path")
    var pathData: Data
    
    @Field(key: "length")
    var length: Double
    
    @Field(key: "format")
    var format: String
    
    @Field(key: "bitrate")
    var bitrate: Int
    
    @Field(key: "samplerate")
    var sampleRate: Int
    
    @Field(key: "album_id")
    var albumId: Int?
    
    @Field(key: "albumtype")
    var albumType: String?
    
    @OptionalParent(key: "album_id")
    var album: Album?
    
    @Field(key: "genre")
    var genre: String?
    
    @Field(key: "lyricist")
    var lyricist: String?
    
    @Field(key: "composer")
    var composer: String?
    
    @Field(key: "comments")
    var comments: String?
    
    @Field(key: "mb_trackid")
    var musicBrainzId: String?
    
    @Field(key: "mb_artistid")
    var musicBrainzArtistId: String?
    
    @Field(key: "encoder")
    var encoder: String?
    
    @Field(key: "discogs_artistid")
    var discogsArtistId: Int?
    
    @Field(key: "added")
    var addedAt: Date
    
    @Children(for: \.$item)
    var attributes: [ItemAttribute]
    
    var path: String? {
        String(data: pathData, encoding: .utf8)
    }
    
    
    
    // MARK: - Functions
    
    // MARK: Comparable Functions
    
    static func < (lhs: Item, rhs: Item) -> Bool {
        lhs.orderIndex < rhs.orderIndex
    }
    
    
    // MARK: Equatable Functions
    
    static func == (lhs: Item, rhs: Item) -> Bool {
        lhs.id == rhs.id
    }
}
