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
    var id: String?
    
    @Field(key: "name")
    var name: String
    
    @Children(for: \.$artist)
    var images: [ArtistImage]
    
    @Timestamp(key: "created_at", on: .create)
    var createdAt: Date?
    
    @Timestamp(key: "updated_at", on: .update)
    var updatedAt: Date?
    
    
    
    // MARK: - Construction
    
    init(_ response: ArtistResponse) {
        id = response.mbId
        name = response.name
    }
    
    init() {}
    
    
    
    // MARK: - Functions
    
    func imagePath(for type: ArtistImage.ImageType, req: Request) async throws -> String? {
        guard
            let image = try await $images.query(on: req.db(.main)).filter(\.$type == type).first(),
            let artistDirectory = artistDirectory(req: req) else { return nil }
        
        return "\(artistDirectory)/\(image.filename)"
    }
    
    
    
    // MARK: - Private Functions
    
    private func artistDirectory(req: Request) -> String? {
        guard let id else { return nil }
        
        return "\(req.application.directory.artistImagesDirectory)\(id)"
    }
}
