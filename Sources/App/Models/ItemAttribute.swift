//
//  ItemAttribute.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 28/02/2025.
//

import Vapor
import Fluent

final class ItemAttribute: Model, @unchecked Sendable {
    
    // MARK: - Constants
    
    static let schema = "item_attributes"
    
    
    
    // MARK: - Properties
    
    @ID(custom: .id, generatedBy: .database)
    var id: Int?
    
    @Parent(key: "entity_id")
    var item: Item
}
