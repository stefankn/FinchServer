//
//  AlbumType.swift
//  FinchServer
//
//  Created by Stefan Klein Nulent on 04/03/2025.
//

import Vapor

enum AlbumType: String, Codable {
    case album
    case compilation
    case djmix = "dj-mix"
    case live
    case ep
    case soundtrack
    case single
}
