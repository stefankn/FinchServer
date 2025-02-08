//
//  Sorting.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 08/02/2025.
//

import Vapor
import Fluent

enum Sorting: String, Decodable {
    case added
    case title
    case artist
}

enum SortingDirection: String, Decodable {
    case ascending
    case descending
    
    
    
    // MARK: - Properties
    
    var direction: DatabaseQuery.Sort.Direction {
        switch self {
        case .ascending:
            return .ascending
        case .descending:
            return .descending
        }
    }
}
