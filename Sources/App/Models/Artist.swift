//
//  Artist.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Vapor
import Fluent

final class Artist: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "artists"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .user)
    var id: Int?
    
    @Field(key: "name")
    var name: String
    
    @Field(key: "real_name")
    var realName: String?
    
    @Field(key: "image")
    var image: String?
    
    @Field(key: "profile")
    var profile: String?
    
    @Field(key: "urls")
    var urls: [String]?
    
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?
    
    var thumbnailImage: String? {
        guard let image else { return nil }
        
        return "thumb_\(image)";
    }
    
    
    
    // MARK: - Construction
    
    init(_ response: ArtistResponse, imageFilename: String?) {
        id = response.id
        name = response.name
        realName = response.realName
        image = imageFilename
        profile = response.profile
        urls = response.urls
    }
    
    init() {}
    
    
    
    // MARK: - Functions
    
    func artworkPath(req: Request) -> String? {
        guard let image, let artistDirectory = artistDirectory(req: req) else { return nil }
        
        return "\(artistDirectory)/\(image)"
    }
    
    func artworkThumbnailPath(req: Request) -> String? {
        guard let thumbnailImage, let artistDirectory = artistDirectory(req: req) else { return nil }
        
        return "\(artistDirectory)/\(thumbnailImage)"
    }
    
    
    
    // MARK: - Private Functions
    
    private func artistDirectory(req: Request) -> String? {
        guard let id else { return nil }
        
        return "\(req.application.directory.artistImagesDirectory)\(id)"
    }
}
