//
//  Item.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor
import Fluent

final class Item: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "items"
    
    
    
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
    
    @Field(key: "encoder")
    var encoder: String?
    
    @Field(key: "added")
    var addedAt: Date
    
    var path: String? {
        String(data: pathData, encoding: .utf8)
    }
}
