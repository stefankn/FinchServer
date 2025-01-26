//
//  Optional+Utilities.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

extension Optional where Wrapped == String {
    
    // MARK: - Properties
    
    var nilIfEmpty: String? {
        switch self {
        case .none:
            return nil
        case .some(let wrapped):
            return wrapped.isEmpty ? nil : wrapped
        }
    }
}
