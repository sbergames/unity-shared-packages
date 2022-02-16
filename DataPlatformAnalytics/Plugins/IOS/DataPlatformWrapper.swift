//
//  DataPlatformWrapper.swift
//  SensorDemo
//
//  Created by Alexandr on 11.01.2022.
//

import Foundation
import CoreData

@objc (DataPlatformWrapper)
public final class DataPlatformWrapper: NSObject  {
    
    @objc public static let shared: DataPlatformWrapper = {
        let instance = DataPlatformWrapper()
        return instance
    }()
    
    private let apiService: APIService = APIService()
 
    private let dbService: DBService = DBService()
    
    private let queue: DispatchQueue = DispatchQueue.main
    private let resendData: DispatchTimeInterval = .seconds(5)
    
    private override init() {}
    
    @objc public func setup(key: String, host: String) {
        apiService.host = host
        apiService.key = key
//        print("setup key:", key)
//        print("setup host:", host)
    }
    
    @objc public func addEventItem(name: String, value: String, date: Date, uuid: UUID) {
        dbService.addEventItem(name: name, value: value, date: date, uuid: uuid)
    }
    
    @objc public func sendAllData() {
        
        guard let items = dbService.dataEventList(fetchLimit: 20) else {
            toLog(error: .database) // "Ошибка получения данных из базы."
            return
        }
        
        if items.isEmpty {
            print("Список пуст. Отправка закончилась")
            return
        }
        
        apiService.sendData(itemsList: items.toDictionary()) { [weak self] response in
            self?.response(items: items, response: response)
        }
    }
    
    private func response(items: [EventItem], response: APIResponse) {
        if case .success(_) = response {
            dbService.deleteObjects(items: items)
            runResendData()
        } else if case let .error(error) = response  {
            toLog(error: error)
            runResendData()
        } else if case let .validationError(error) = response  {
            toLog(message: error.detail)
        }
    }
    
    public func runResendData() {
        queue.asyncAfter(deadline: .now() + resendData) { [weak self] in
            self?.sendAllData()
        }
    }
    
}

extension DataPlatformWrapper {
    
    private func toLog(message: Any) {
        print(message)
    }
    
    private func toLog(error: DataPlatformError) {
        print(error)
    }
    
}
