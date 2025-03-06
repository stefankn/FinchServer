//
//  UpdateItemDTO.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 06/03/2025.
//

import Vapor

struct UpdateItemDTO: Content {
    
    // MARK: - Properties
    
    let artist: String
    let artists: String
    let title: String
}
