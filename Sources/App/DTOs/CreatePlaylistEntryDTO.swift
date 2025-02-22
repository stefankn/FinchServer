//
//  CreatePlaylistEntryDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 22/02/2025.
//

import Vapor

struct CreatePlaylistEntryDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case itemId = "item_id"
    }
    
    
    
    // MARK: - Properties
    
    let itemId: Int
}
