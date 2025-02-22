//
//  PlaylistDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 16/02/2025.
//

import Vapor

struct PlaylistDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case name
        case description
        case image
        case createdAt = "created_at"
        case updatedAt = "updated_at"
    }
    
    
    
    // MARK: - Properties
    
    let id: Int?
    let name: String
    let description: String?
    let image: String?
    let createdAt: Date?
    let updatedAt: Date?
    
    
    
    // MARK: - Construction
    
    init(_ playlist: Playlist) {
        id = playlist.id
        name = playlist.name
        description = playlist.description
        image = playlist.image
        createdAt = playlist.createdAt
        updatedAt = playlist.updatedAt
    }
}
