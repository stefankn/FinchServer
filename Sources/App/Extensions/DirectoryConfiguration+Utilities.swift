//
//  DirectoryConfiguration+Utilities.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 28/02/2025.
//

import Vapor

extension DirectoryConfiguration {
    
    // MARK: - Properties
    
    var thumbnailsDirectory: String {
        resourcesDirectory + "/Thumbnails/"
    }
}
