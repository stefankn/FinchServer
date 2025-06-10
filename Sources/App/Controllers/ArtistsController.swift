//
//  ArtistsController.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Vapor
import SwiftGD

struct ArtistsController: RouteCollection {
    
    // MARK: - Private Properties
    
    private let fanarttvURL = "https://webservice.fanart.tv/v3/music"
    
    
    
    // MARK: - Functions
    
    @Sendable func show(req: Request) async throws -> ArtistDTO {
        let artist = try await fetchArtist(req: req)
        
        return ArtistDTO(artist)
    }
    
    @Sendable func background(req: Request) async throws -> Response {
        let artist = try await fetchArtist(req: req)
        
        if let path = try await artist.imagePath(for: .background, req: req) {
            return req.fileio.streamFile(at: path)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func image(req: Request) async throws -> Response {
        let artist = try await fetchArtist(req: req)
        
        if let path = try await artist.imagePath(for: .image, req: req) {
            return req.fileio.streamFile(at: path)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func thumbnail(req: Request) async throws -> Response {
        let artist = try await fetchArtist(req: req)
        
        if let path = try await artist.imagePath(for: .thumbnail, req: req) {
            return req.fileio.streamFile(at: path)
        }
        
        throw Abort(.notFound)
    }
    
    @Sendable func logo(req: Request) async throws -> Response {
        let artist = try await fetchArtist(req: req)
        
        if let path = try await artist.imagePath(for: .musicLogo, req: req) {
            return req.fileio.streamFile(at: path)
        }
        
        throw Abort(.notFound)
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
            
            artist.get("background", use: background)
                .openAPI(
                    summary: "Get a background image for an artist",
                    query: .type(String.self)
                )
            
            artist.get("image", use: image)
                .openAPI(
                    summary: "Get an image for an artist",
                    query: .type(String.self)
                )
            
            artist.get("thumbnail", use: thumbnail)
                .openAPI(
                    summary: "Get a thumbnail image for an artist",
                    query: .type(String.self)
                )
            
            artist.get("logo", use: logo)
                .openAPI(
                    summary: "Get a logo for an artist",
                    query: .type(String.self)
                )
        }
    }
    
    
    
    // MARK: - Private Functions
    
    private func fetchArtist(req: Request) async throws -> Artist {
        if let artist = try await Artist.find(req.parameters.get("id"), on: req.db(.main)) {
            return artist
        }
        
        guard let artistId = req.parameters.get("id") else { throw Abort(.badRequest) }
        guard let fanarttvApiKey = Environment.get("FANARTTV_APIKEY") else { throw Abort(.internalServerError) }

        let response = try await req.client.get("\(fanarttvURL)/\(artistId)?api_key=\(fanarttvApiKey)") { req in
            req.headers.contentType = .json
        }
        
        let artistResponse = try response.content.decode(ArtistResponse.self)
        
        let artist = Artist(artistResponse)
        try await artist.save(on: req.db(.main))
        
        if artistResponse.isImageAvailable {
            let artistDirectory = "\(req.application.directory.artistImagesDirectory)\(artistResponse.mbId)"
            try FileManager.default.createDirectory(atPath: artistDirectory, withIntermediateDirectories: true)
            
            var images: [ArtistImage] = []
            
            images += try await download(artistResponse.backgrounds?.first, type: .background, to: artistDirectory, req: req)
            images += try await download(artistResponse.images?.first, type: .image, to: artistDirectory, req: req)
            images += try await download(artistResponse.hdMusicLogos?.first ?? artistResponse.musicLogos?.first, type: .musicLogo, to: artistDirectory, req: req)
            
            if !images.compactMap({ $0 }).isEmpty {
                try await artist.$images.create(images.compactMap{ $0 }, on: req.db(.main))
            }
        }
        
        return artist
    }
    
    private func download(_ image: ArtistResponse.Image?, type: ArtistImage.ImageType, to directory: String, req: Request) async throws -> [ArtistImage] {
        guard let image, let url = URL(string: image.url) else { return [] }
        
        let filename = url.lastPathComponent
        let path = "\(directory)/\(filename)";
        var images: [ArtistImage] = []
        
        if !FileManager.default.fileExists(atPath: path), let data = try await req.client.get("\(image.url)").body {
            try await req.fileio.writeFile(data, at: path)
            
            if type == .image {
                let thumbnailFilename = "thumb_\(filename)"
                let image = Image(url: URL(fileURLWithPath: path))
                
                if let thumbnail = image?.resizedTo(width: 300) {
                    thumbnail.write(to: URL(fileURLWithPath: "\(directory)/\(thumbnailFilename)"))
                    images.append(.init(.thumbnail, filename: thumbnailFilename))
                }
            }
            
            images.append(.init(type, filename: filename))
        }
        
        return images
    }
}
