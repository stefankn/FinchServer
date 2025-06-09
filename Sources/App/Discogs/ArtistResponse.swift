//
//  ArtistResponse.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Foundation

struct ArtistResponse: Decodable {
    
    // MARK: - Types
    
    enum CodingKeys: String, CodingKey {
        case id
        case name
        case realName = "realname"
        case profile
        case urls
        case images
    }
    
    
    
    // MARK: - Properties
    
    let id: Int
    let name: String
    let realName: String?
    let profile: String?
    let urls: [String]?
    let images: [Image]
}
