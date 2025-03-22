//
//  AlbumType.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 04/03/2025.
//

import Vapor

enum AlbumType: String, Codable {
    case album = "Album"
    case compilation = "Compilation"
    case djmix = "Mix"
    case ep = "EP"
    case maxiSingle = "Maxi-Single"
    case single = "Single"
}
