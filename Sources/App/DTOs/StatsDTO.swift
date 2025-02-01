//
//  StatsDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor

struct StatsDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case trackCount = "track_count"
        case totalTime = "total_time"
        case approximateTotalSize = "approximate_total_size"
        case artistCount = "artist_count"
        case albumCount = "album_count"
        case albumArtistCount = "album_artist_count"
    }
    
    
    
    // MARK: - Properties
    
    let trackCount: Int
    let totalTime: String
    let approximateTotalSize: String
    let artistCount: Int
    let albumCount: Int
    let albumArtistCount: Int
}
