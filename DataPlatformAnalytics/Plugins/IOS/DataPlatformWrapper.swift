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
    
    private override init() {}
    
    @objc public func setup(key: String, host: String) {}
    
    @objc public func addEventItem(name: String, value: String, date: Date, uuid: UUID) {}
    
    @objc public func sendAllData() {}
    
}
