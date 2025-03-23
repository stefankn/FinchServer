//
//  AlbumFilter.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 23/03/2025.
//

import Vapor

enum AlbumFilter: String, Decodable {
    case album
    case compilation
    case single
    case ep
    
    
    
    // MARK: - Properties
    
    var types: [String] {
        switch self {
        case .album:
            return ["Album", "LP"]
        case .compilation:
            return ["Compilation", "Mixed"]
        case .single:
            return ["Single", "Maxi-Single"]
        case .ep:
            return ["EP"]
        }
    }
}
