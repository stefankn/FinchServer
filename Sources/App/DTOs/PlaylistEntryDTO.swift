//
//  PlaylistEntryDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 22/02/2025.
//

import Vapor

struct PlaylistEntryDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case index
        case item
        case createdAt = "created_at"
    }
    
    
    
    // MARK: - Properties
    
    let id: Int?
    let index: Int
    let item: ItemDTO
    let createdAt: Date?
    
    
    
    // MARK: - Construction
    
    init?(_ playlistEntry: PlaylistEntry, item: Item?) {
        guard let item else { return nil }
        
        self.init(playlistEntry, item: item)
    }
    
    init(_ playlistEntry: PlaylistEntry, item: Item) {
        id = playlistEntry.id
        index = playlistEntry.index
        self.item = ItemDTO(item)
        createdAt = playlistEntry.createdAt
    }
}
