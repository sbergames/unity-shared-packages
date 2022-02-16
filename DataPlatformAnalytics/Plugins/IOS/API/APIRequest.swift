//
//  APIRequest.swift
//  UnityFramework
//
//  Created by Alexandr on 15.02.2022.
//

import Foundation

struct APIRequest {
    
    private let remove = "?api_key={API_KEY}"
    private let apiKey = "api_key"
    private let httpMethod = "POST"
    private let headerFields = ["accept":"application/json", "Content-Type": "application/json"]
    private let encoding: String.Encoding = .utf8
    private let timeoutInterval = 10.0
    private let url: URL
    
    init?(with key: String?, host: String?) {
        
        guard let cleanKey = key else {
            return nil
        }
        
        guard let cleanHost = host?.replacingOccurrences(of: remove, with: "") else {
            return nil
        }
        
        guard var components = URLComponents(string: cleanHost) else {
            return nil
        }
        
        components.queryItems = [URLQueryItem(name: apiKey, value: cleanKey)]
        
        guard let cleanURL = components.url else {
            return nil
        }
        
        self.url = cleanURL
    }
    
    public func request(base: [[String : Any]]) -> URLRequest? {
        guard let body = requestBoby(base: base) else { return nil }
        var request = URLRequest(url: url, cachePolicy: .reloadIgnoringCacheData, timeoutInterval: timeoutInterval)
        request.httpMethod = httpMethod
        request.allHTTPHeaderFields = headerFields
        request.httpBody = body.data(using: encoding)
        return request
    }
    
    private func requestBoby(base: [[String : Any]]) -> String? {
        do {
            let jsonData = try JSONSerialization.data(withJSONObject: base, options: .prettyPrinted)
            return String(data: jsonData, encoding: encoding)
        } catch {
            return nil
        }
    }
}
