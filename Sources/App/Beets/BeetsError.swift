//
//  BeetsError.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 26/01/2025.
//

import Vapor

enum BeetsError: Error {
    case missingBeetsExecutableEnvironmentVariable
    case resolveConfigLocationFailure
    case readConfigFailure
    case commandFailure
}
