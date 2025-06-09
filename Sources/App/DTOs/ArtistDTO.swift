//
//  ArtistDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Vapor

struct ArtistDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case name
        case realName = "real_name"
        case profile
        case image
        case thumbnail
        case urls
    }
    
    
    
    // MARK: - Properties
    
    let id: Int?
    let name: String
    let realName: String?
    let profile: String?
    let image: String?
    let thumbnail: String?
    let urls: [String]
    
    
    
    // MARK: - Construction
    
    init(_ artist: Artist) {
        id = artist.id
        name = artist.name
        realName = artist.realName
        profile = artist.profile
        image = artist.image
        thumbnail = artist.image
        urls = artist.urls ?? []
    }
}
