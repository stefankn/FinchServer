//
//  AlbumAttribute.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Fluent

final class AlbumAttribute: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "album_attributes"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Field(key: "key")
    var key: String
    
    @Field(key: "value")
    var value: String
    
    @Parent(key: "entity_id")
    var album: Album
}
