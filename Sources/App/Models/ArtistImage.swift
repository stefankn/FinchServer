//
//  ArtistImage.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 10/06/2025.
//

import Vapor
import Fluent

final class ArtistImage: Model, @unchecked Sendable {
    
    // MARK: - Types
    
    enum ImageType: String, Codable {
        case background
        case image
        case thumbnail
        case musicLogo
    }
    
    
    
    // MARK: - Constants
    
    static let schema = "artist_images"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "filename")
    var filename: String
    
    @Enum(key: "image_type")
    var type: ImageType
    
    @Parent(key: "artist_id")
    var artist: Artist
    
    
    
    // MARK: - Construction
    
    init(_ type: ImageType, filename: String) {
        self.type = type
        self.filename = filename
    }
    
    init() {}
}
