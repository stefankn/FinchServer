//
//  UpdateAlbumDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 14/03/2025.
//

import Vapor

struct UpdateAlbumDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case artist
        case title
        case artworkPath = "artwork_path"
    }
    
    
    
    // MARK: - Properties
    
    let artist: String
    let title: String
    let artworkPath: String?
}
