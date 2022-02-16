//
//  APIResponse.swift
//  SensorDemo
//
//  Created by Alexandr on 11.01.2022.
//

import Foundation

enum APIResponse {
    
    case success (ResponseSuccess)
    case validationError(ResponseValidationError)
    case error(DataPlatformError)
    
    init(statusCode: Int, data: Data) {
        switch statusCode {
            case 200: //"ДАННЫЕ УСПЕШНО ОТПРАВЛЕНЫ"
                self = APIResponse.responseSuccess(data: data)
            case 422: //"ОШИБКА ОТПРАВКИ ДАННЫХ"
                self = APIResponse.responseValidationError(data: data)
            default: //"НЕ УДАЛОСЬ РАСПОЗНАТЬ StatusCode"
                self = APIResponse.error(.statusCode)
        }
    }
    
    private static func responseSuccess(data: Data) -> APIResponse {
        do {
            return .success(try JSONDecoder().decode(ResponseSuccess.self, from: data))
        } catch {
            return .error(.parse(error.localizedDescription))
        }
    }
    
    private static func responseValidationError(data: Data) -> APIResponse {
        do {
            return .validationError(try JSONDecoder().decode(ResponseValidationError.self, from: data))
        } catch {
            return .error(.parse(error.localizedDescription))
        }
    }

    struct ResponseSuccess : Codable, Equatable {
        let success: Int
        let reject: Int
    }

    struct ResponseValidationError : Codable {
        
        struct Message : Codable, Equatable {
            let location: [String]
            let message: String
            let errorType: String
            
            private enum CodingKeys : String, CodingKey {
                case location = "loc", message = "msg", errorType = "type"
            }
        }
        
        let detail: [Message]
    }
}



