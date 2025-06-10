//
//  ArtistResponse.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 10/06/2025.
//

import Foundation

struct ArtistResponse: Decodable {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case name
        case mbId = "mbid_id"
        case backgrounds = "artistbackground"
        case images = "artistthumb"
        case musicLogos = "musiclogo"
        case hdMusicLogos = "hdmusiclogo"
    }
    
    
    
    // MARK: - Properties
    
    let name: String
    let mbId: String
    let backgrounds: [Image]?
    let images: [Image]?
    let musicLogos: [Image]?
    let hdMusicLogos: [Image]?
    
    var isImageAvailable: Bool {
        backgrounds?.isEmpty == false || images?.isEmpty == false || musicLogos?.isEmpty == false || hdMusicLogos?.isEmpty == false
    }
}
