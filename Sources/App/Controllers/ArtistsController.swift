//
//  ArtistsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Vapor

struct ArtistsController: RouteCollection {
    
    // MARK: - Private Properties
    
    private let discogsURL = "https://api.discogs.com"
    
    
    
    // MARK: - Functions
    
    @Sendable func show(req: Request) async throws -> ArtistDTO {
        if let artist = try await Artist.find(req.parameters.get("id"), on: req.db(.main)) {
            return ArtistDTO(artist)
        }
        
        guard let artistId = req.parameters.get("id") else { throw Abort(.badRequest) }
        guard let discogsToken = Environment.get("DISCOGS_TOKEN") else { throw Abort(.internalServerError) }

        let response = try await req.client.get("\(discogsURL)/artists/\(artistId)") { req in
            req.headers.contentType = .json
            req.headers.add(name: "User-Agent", value: "Finch/1.0")
            req.headers.add(name: "Authorization", value: "Discogs token=\(discogsToken)")
        }
        
        let artistResponse = try response.content.decode(ArtistResponse.self)
        var imageFilename: String?
        
        if let primaryImage = artistResponse.images.first(where: { $0.type == .primary }), let url = URL(string: primaryImage.uri) {
            let filename = "primary_\(url.lastPathComponent)"
            
            let artistDirectory = "\(req.application.directory.artistImagesDirectory)\(artistResponse.id)"
            try FileManager.default.createDirectory(atPath: artistDirectory, withIntermediateDirectories: true)
            
            let artistImagePath = "\(artistDirectory)/\(filename)";
            let artistThumbnailPath = "\(artistDirectory)/thumb_\(filename)";
            
            if !FileManager.default.fileExists(atPath: artistImagePath), let data = try await req.client.get("\(primaryImage.uri)").body {
                try await req.fileio.writeFile(data, at: artistImagePath)
            }
            
            if !FileManager.default.fileExists(atPath: artistThumbnailPath), let data = try await req.client.get("\(primaryImage.uri150)").body  {
                try await req.fileio.writeFile(data, at: artistThumbnailPath)
            }
            
            imageFilename = filename
        }
        
        let artist = Artist(artistResponse, imageFilename: imageFilename)
        try await artist.save(on: req.db(.main))
      
        return ArtistDTO(artist)
    }
    
    @Sendable func artwork(req: Request) async throws -> Response {
        guard
            let artist = try await Artist.find(req.parameters.get("id"), on: req.db(.main)),
            let artworkPath = artist.artworkPath(req: req) else {
            
            throw Abort(.notFound)
        }
        
        return req.fileio.streamFile(at: artworkPath)
    }
    
    @Sendable func artworkThumbnail(req: Request) async throws -> Response {
        guard
            let artist = try await Artist.find(req.parameters.get("id"), on: req.db(.main)),
            let artworkThumbnailPath = artist.artworkThumbnailPath(req: req) else {
            
            throw Abort(.notFound)
        }
        
        return req.fileio.streamFile(at: artworkThumbnailPath)
    }
    
    
    // MARK: RouteCollection Functions
    
    func boot(routes: any RoutesBuilder) throws {
        let artists = routes.grouped("api", "v1", "artists")
        
        artists.group(":id") { artist in
            artist.get(use: show)
                .openAPI(
                    summary: "Get an artist by id",
                    query: .type(String.self)
                )
            
            artist.group("artwork") { artwork in
                artwork.get(use: self.artwork)
                    .openAPI(
                        summary: "Get artwork for an artist",
                        query: .type(String.self)
                    )
                
                artwork.get("thumbnail", use: artworkThumbnail)
                    .openAPI(
                        summary: "Get a thumbnail version of the artwork for an artist",
                        query: .type(String.self)
                    )
            }
        }
    }
}
