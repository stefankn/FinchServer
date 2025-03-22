//
//  Album.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 25/01/2025.
//

import Vapor
import Fluent
import SwiftGD

final class Album: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "albums"
    
    
    // MARK: - Private Properties
    
    @Field(key: "albumtypes")
    private var typesValue: String
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "albumartist")
    var albumArtist: String
    
    @Field(key: "albumartists_sort")
    var albumArtistSortKey: String
    
    @Field(key: "album")
    var title: String
    
    @Field(key: "release_group_title")
    var releaseGroupTitle: String
    
    @Field(key: "albumtype")
    var albumType: String

    @Field(key: "added")
    var addedAt: Date
    
    @Field(key: "genre")
    var genre: String?
    
    @Field(key: "style")
    var style: String?
    
    @Field(key: "year")
    var year: Int
    
    @Field(key: "disctotal")
    var discCount: Int
    
    @Field(key: "discogs_albumid")
    var discogsAlbumId: Int?
    
    @Field(key: "discogs_artistid")
    var discogsArtistId: Int?
    
    @Field(key: "discogs_labelid")
    var discogsLabelId: Int?
    
    @Field(key: "label")
    var label: String?
    
    @Field(key: "barcode")
    var barcode: String?
    
    @Field(key: "asin")
    var asin: String?
    
    @Field(key: "catalognum")
    var catalogNumber: String?
    
    @Field(key: "country")
    var country: String?
    
    @Field(key: "artpath")
    var artworkPathData: Data?
    
    @Children(for: \.$album)
    var attributes: [AlbumAttribute]
    
    @Children(for: \.$album)
    var items: [Item]
    
    var types: [AlbumType] {
        typesValue
            .split(separator: ";")
            .compactMap{ AlbumType(rawValue: $0.trimmingCharacters(in: .whitespaces)) }
    }
    
    var artworkPath: String? {
        get {
            guard isArtworkAvailable, let artworkPathData else { return nil }
            
            return String(data: artworkPathData, encoding: .utf8)
        }
        set {
            artworkPathData = newValue?.data(using: .utf8)
        }
    }
    
    var artworkURL: URL? {
        guard let artworkPath else { return nil }
        
        return URL(fileURLWithPath: artworkPath)
    }
    
    var artwork: Image? {
        guard let artworkURL else { return nil }
        
        return Image(url: artworkURL)
    }
    
    var isArtworkAvailable: Bool {
        if let artworkPathData, !artworkPathData.isEmpty {
            return true
        }
        
        return false
    }
    
    var artworkThumbnailFilename: String? {
        guard let discogsAlbumId, let fileExtension = artworkURL?.pathExtension else { return nil }
        
        return "album_\(discogsAlbumId)_thumbnail.\(fileExtension)"
    }
    
    
    
    // MARK: - Functions
    
    func artworkThumbnailPath(req: Request) -> String? {
        guard let artwork, let artworkThumbnailFilename else { return nil }
        
        let thumbnailPath = req.application.directory.thumbnailsDirectory + artworkThumbnailFilename
        
        if FileManager.default.fileExists(atPath: thumbnailPath) {
            return thumbnailPath
        }
        
        guard let thumbnail = artwork.resizedTo(width: 300) else { return nil }
        
        let thumbnailURL = URL(fileURLWithPath: thumbnailPath)
        thumbnail.write(to: thumbnailURL)
        
        return thumbnailPath
    }
}
