//
//  CreatePlaylistDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 21/02/2025.
//

import Vapor

struct CreatePlaylistDTO: Content {
    
    // MARK: - Properties
    
    let name: String
    let description: String?
    let items: [Int]?
}
