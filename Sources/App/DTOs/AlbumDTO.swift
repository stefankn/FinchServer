//
//  AlbumDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 25/01/2025.
//

import Vapor

struct AlbumDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case title
        case releaseGroupTitle = "release_group_title"
        case artist
        case artistSortKey = "artist_sort_key"
        case type
        case genre
        case style
        case year
        case discCount = "disc_count"
        case discogsAlbumId = "discogs_album_id"
        case discogsArtistId = "discogs_artist_id"
        case discogsLabelId = "discogs_label_id"
        case label
        case barcode
        case asin
        case catalogNumber = "catalog_number"
        case country
        case isArtworkAvailable = "is_artwork_available"
        case artworkPath = "artwork_path"
        case addedAt = "added_at"
        case media
        case dataSource = "data_source"
        case items
    }
    
    
    
    // MARK: - Properties
    
    let id: Int?
    let title: String
    let releaseGroupTitle: String
    let artist: String
    let artistSortKey: String
    let type: String
    let genre: String?
    let style: String?
    let year: Int
    let discCount: Int
    let discogsAlbumId: Int?
    let discogsArtistId: Int?
    let discogsLabelId: Int?
    let label: String?
    let barcode: String?
    let asin: String?
    let catalogNumber: String?
    let country: String?
    let isArtworkAvailable: Bool
    let artworkPath: String?
    
    let addedAt: Date
    let media: String?
    let dataSource: String?
    
    let items: [ItemDTO]?
    
    
    
    // MARK: - Construction
    
    init(_ album: Album, items: [Item]? = nil) {
        id = album.id
        title = album.title
        releaseGroupTitle = album.releaseGroupTitle
        artist = album.albumArtist
        artistSortKey = album.albumArtistSortKey
        type = album.albumType
        genre = album.genre.nilIfEmpty
        style = album.style.nilIfEmpty
        year = album.year
        discCount = album.discCount
        discogsAlbumId = album.discogsAlbumId != 0 ? album.discogsAlbumId : nil
        discogsArtistId = album.discogsArtistId != 0 ? album.discogsArtistId : nil
        discogsLabelId = album.discogsLabelId != 0 ? album.discogsLabelId : nil
        label = album.label.nilIfEmpty
        barcode = album.barcode.nilIfEmpty
        asin = album.asin.nilIfEmpty
        catalogNumber = album.catalogNumber.nilIfEmpty
        country = album.country
        isArtworkAvailable = album.isArtworkAvailable
        artworkPath = album.artworkPath
        addedAt = album.addedAt
        media = album.attributes.first{ $0.key == "media" }?.value
        dataSource = album.attributes.first{ $0.key == "data_source" }?.value
        
        if let items {
            self.items = items.map{ ItemDTO($0) }
        } else {
            self.items = nil
        }
    }
}
