//
//  BeetsConfiguration.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 25/01/2025.
//

import Vapor

struct BeetsConfiguration {
    
    // MARK: - Properties
    
    let executablePath: String
    let configLocation: URL
    let databaseLocation: URL
}

struct BeetsConfigurationKey: StorageKey {
    
    // MARK: - Types
    
    typealias Value = BeetsConfiguration
}

extension Application {
    
    // MARK: - Properties
    
    var beetsConfiguration: BeetsConfiguration {
        get { storage[BeetsConfigurationKey.self]! }
        set { storage[BeetsConfigurationKey.self] = newValue }
    }
}
