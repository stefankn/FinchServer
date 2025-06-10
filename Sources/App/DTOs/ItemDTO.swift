//
//  ItemDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor

struct ItemDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case track
        case disc
        case title
        case artist
        case artists
        case length
        case format
        case bitrate
        case sampleRate = "sample_rate"
        case genre
        case lyricist
        case composer
        case comments
        case musicBrainzId = "music_brainz_id"
        case albumId = "album_id"
        case albumType = "album_type"
        case discogsArtistId = "discogs_artist_id"
        case musicBrainzArtistId = "music_brainz_artist_id"
    }
    
    
    
    // MARK: - Properties
    
    let id: Int?
    let track: Int?
    let disc: Int?
    let title: String
    let artist: String
    let artists: String
    let length: Double
    let format: String
    let bitrate: Int
    let sampleRate: Int
    let genre: String?
    let lyricist: String?
    let composer: String?
    let comments: String?
    let musicBrainzId: String?
    let albumId: Int?
    let albumType: String?
    let discogsArtistId: Int?
    let musicBrainzArtistId: String?
    
    
    
    // MARK: - Construction
    
    init(_ item: Item, includeAlbumId: Bool = false) {
        id = item.id
        track = item.track
        disc = item.disc
        title = item.title
        artist = item.artist
        artists = item.artists
        length = item.length
        format = item.format
        bitrate = item.bitrate
        sampleRate = item.sampleRate
        genre = item.genre.nilIfEmpty
        lyricist = item.lyricist.nilIfEmpty
        composer = item.composer.nilIfEmpty
        comments = item.comments.nilIfEmpty
        musicBrainzId = item.musicBrainzId.nilIfEmpty
        albumType = item.albumType
        discogsArtistId = item.discogsArtistId
        musicBrainzArtistId = item.musicBrainzArtistId
        
        if includeAlbumId {
            albumId = item.$album.id
        } else {
            albumId = nil
        }
    }
}
