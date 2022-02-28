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
    private let queue: DispatchQueue = DispatchQueue.global(qos: .default)
    private let queueName = "DataPlatformQueue"
    private let timeoutBetweenResend: Int = 10_000
    private var sendingErrorCountFromLastSuccess: Int = 0
    
    private var resendFunc: DispatchWorkItem? = nil
    
    private lazy var operationQueue: OperationQueue = {
        var queue = OperationQueue()
        queue.name = queueName
        queue.maxConcurrentOperationCount = 1
        return queue
      }()
    
    private override init() {}
    
    @objc public func setup(key: String, host: String) {
        apiService.host = host
        apiService.key = key
    }
    
    @objc public func addEventItem(name: String, value: String, date: Date, uuid: UUID) {
        dbService.addEventItem(name: name, value: value, date: date, uuid: uuid)
    }
    
    @objc public func sendAllData() {
        
        guard let items = dbService.dataEventList() else {
            toLog(error: .database) // "Ошибка получения данных из базы."
            return
        }

        if items.isEmpty { return }

        apiService.sendData(itemsList: items.toDictionary()) { [weak self] response in
            self?.response(response: response, items: items)
        }
    }
    
    private func sendDataComplete() {
        if dbService.countEventList() == 0 {
            removeResend()
            sendingErrorCountFromLastSuccess = 0
        } else {
            resendAllData()
        }
    }
    
    private func response(response: APIResponse, items: [EventItem]) {
        
        // 1. Если данные отправились штатно, то мы проверяем сколько записей осталось в базе и если их там нет прекращаем цикл
        // 2. Если записи остались, то реализуем цикл отправки через 1 секунду
        // 3. Если данные не отправились то мы запускаем процедуру добавления таска переотправки
        // 4. Если данные н отправились но пришли новые данные, которые тоже не отправились и при этом задача повторной отправки уже есть, то мы НЕ добавляем новую задачу переотправки, т.к. таковая уже есть
        
        if case .success(_) = response {
            dbService.deleteObjects(items: items)
            sendDataComplete()
        } else {
            if case let .error(error) = response  {toLog(error: error) }
            if case let .validationError(error) = response  { toLog(message: error.detail) }
            reSendData()
        }
    }
    
    
    private func reSendData() {
        
        removeResend()
        
        let resend = DispatchWorkItem(block: { [weak self] in
            
            if self?.dbService.countEventList() == 0 { return }
            
            guard let items = self?.dbService.dataEventList() else {
                self?.toLog(error: .database)
                return
            }

            if items.isEmpty { return }

            self?.apiService.sendData(itemsList: items.toDictionary()) { [weak self] response in
                self?.response(response: response, items: items)
            }
        })
        
        sendingErrorCountFromLastSuccess += 1
        resendFunc = resend
        let time: DispatchTime = .now() + .milliseconds(currentTimeout())
        queue.asyncAfter(deadline: time, execute: resend)

    }
    
    private func resendAllData() {
        let time: DispatchTime = .now() + .milliseconds(1000)
        
        let resend = DispatchWorkItem(block: { [weak self] in
            guard let count = self?.dbService.countEventList() else { return }
            if count > 0 {
                self?.sendAllData()
            }
        })
        
        queue.asyncAfter(deadline: time, execute: resend)
    }
    
    private func removeResend() {
        guard resendFunc == nil else { return }
        resendFunc?.cancel()
        resendFunc = nil
    }
    
    private func currentTimeout() -> Int {
        let sending = sendingErrorCountFromLastSuccess
        return timeoutBetweenResend * (sending < 3 ? sending + sending + 1 : 6)
    }
    
}

extension DataPlatformWrapper {
    
    private func toLog(message: Any) {
//        print(message)
    }
    
    private func toLog(error: DataPlatformError) {
//        print(error)
    }
    
}
