//
//  ArtistImageResponseImage.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/06/2025.
//

import Foundation

extension ArtistResponse {
    struct Image: Decodable {
        
        // MARK: - Types
        
        enum ImageType: String, Decodable {
            case primary
            case secondary
        }
        
        
        
        // MARK: - Properties
        
        let type: ImageType
        let uri: String
        let uri150: String
        let width: Int
        let height: Int
    }
}
