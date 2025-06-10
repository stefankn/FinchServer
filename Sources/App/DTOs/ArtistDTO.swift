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
    }
    
    
    
    // MARK: - Properties
    
    let id: String?
    let name: String
    
    
    
    // MARK: - Construction
    
    init(_ artist: Artist) {
        id = artist.id
        name = artist.name
    }
}
