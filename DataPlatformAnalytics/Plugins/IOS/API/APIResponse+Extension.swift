//
//  APIResponse+Extension.swift
//  UnityFramework
//
//  Created by Alexandr on 15.02.2022.
//

import Foundation

extension APIResponse: Equatable {
    
    static func == (lhs: APIResponse, rhs: APIResponse) -> Bool {
        switch lhs {
        case let .success(lresponse):
            switch rhs {
                case let .success(rresponse): return lresponse == rresponse
                default: return false
            }
        case let .validationError(lerror):
            switch rhs {
                case let .validationError(rerror): return lerror == rerror
                default: return false
            }
        case let .error(lerror):
            switch rhs {
                case let .error(rerror): return lerror == rerror
                default: return false
            }
        }
    }
}

extension APIResponse.ResponseValidationError: Equatable {
    
    static func == (lhs: APIResponse.ResponseValidationError, rhs: APIResponse.ResponseValidationError) -> Bool {
        return lhs.detail.elementsEqual(rhs.detail)
    }
    
}
