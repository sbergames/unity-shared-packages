//
//  APIService.swift
//  SensorDemo
//
//  Created by Alexandr on 13.12.2021.
//

import Foundation

///  Формат запроса отправки данных на сервер
///
///        curl -X 'POST' \
///          'https://api.sbergames.network/api/events/batch?api_key=UdVvIOmJuv-oY9OXwfXhHVLq1Hv8YwWoOkIbwAPQMSDgF1KjChPx1n63981zoWL6' \
///          -H 'accept: application/json' \
///          -H 'Content-Type: application/json' \
///          -d '[
///          {"test": 1}
///        ]'

final class APIService {
    
    /// ключ необходимый для для отправки уведомлений
    public var key: String? = nil
    
    /// для совместимости c C# кодом host должен иметь вид: "https://api.sbergames.network/api/events/batch?api_key={API_KEY}"
    public var host: String? = nil
    
    private let session = URLSession.shared
    
    
    // MARK: - sendData
    
    public func sendData(itemsList : [[String : Any]], completionHandler: @escaping (APIResponse) -> ()) {
        
        guard let apiRequest = APIRequest(with: key, host: host) else {
            completionHandler(.error(.keyAndHosh))
            return
        }
        
        guard let request = apiRequest.request(base: itemsList) else {
            completionHandler(.error(.request))
            return
        }
        
        session.dataTask(with: request) {[weak self] (data, response, error) in
            if let api = self?.dataTaskComplete(error: error, data: data, response: response) {
                completionHandler(api)
            }
        }.resume()
    }
    
    private func dataTaskComplete(error: Error?, data: Data?, response: URLResponse?) -> APIResponse {
        
        //        print("Полученный ответ:", String(data: data, encoding: .utf8)!)
        
        if let error = error {
            return .error(.dataTask(error as NSError))
        }
        
        guard let data = data else {
            return .error(.emptyData)
        }
        
        guard let statusCode = (response as? HTTPURLResponse)?.statusCode else {
            return .error(.statusCode)
        }
        
        let api = APIResponse(statusCode: statusCode, data: data)
        
        //        print("Status code:", statusCode, "API: ", api)
        
        return api
        
    }
}
