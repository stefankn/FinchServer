//
//  DeleteAlbumDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 28/02/2025.
//

import Vapor

struct DeleteAlbumDTO: Content {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case deleteFiles = "delete_files"
    }
    
    
    
    // MARK: - Properties
    
    let deleteFiles: Bool
}
