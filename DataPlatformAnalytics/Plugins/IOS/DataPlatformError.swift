//
//  DataPlatformLogger.swift
//  UnityFramework
//
//  Created by Alexandr on 15.02.2022.
//

import Foundation

enum DataPlatformError : Error, Equatable {
    case database // ошибка базы данных (при запросе данных)
    case keyAndHosh // отсутсвут ключ либо не указан хост
    case request // ошибка в формировании запроса
    case dataTask (NSError) //ошибка которая пришла в ответе таска
    case emptyData //в ответе пришли пустые данные
    case statusCode // неизвестный statusCode
    case parse(String?) // ошибка парсинга ответа
    
    enum DBError: Error {
        case notAddPersistentStore(Error) // Not addPersistentStore"
        case contextSave(Error) // "Error context save"
        case contextExecute(Error) //"Error context execute"
    }
}

