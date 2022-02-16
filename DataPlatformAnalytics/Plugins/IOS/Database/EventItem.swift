//
//  EventItem.swift
//  SensorDemo
//
//  Created by Alexandr on 10.01.2022.
//

import Foundation
import CoreData

@objc(EventItem)
public class EventItem: NSManagedObject {
    @NSManaged public var key: String
    @NSManaged public var date: Date
    @NSManaged public var value: String
    @NSManaged public var uuid: UUID
    
    public func dictionary() -> [String: Any]  {
        return convertToDictionary(text: value) ?? [:]
    }
    
    private func convertToDictionary(text: String) -> [String: Any]? {
        guard let data = text.data(using: .utf8) else { return nil }
            
        do {
            return try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any]
        } catch {
            return nil
        }

    }
}


extension Array where Element == EventItem {
    
    public func toDictionary() -> [[String : Any]] {
        var dict: [[String : Any]] = []
        
        self.forEach { item in
            let dictItem = item.dictionary()
            dict.append(dictItem)
        }
        
        return dict
    }
}
