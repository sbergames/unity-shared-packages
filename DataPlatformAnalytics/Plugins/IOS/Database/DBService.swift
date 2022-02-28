//
//  DBService.swift
//  SensorDemo
//
//  Created by Alexandr on 14.12.2021.
//

import Foundation
import CoreData

final class DBService {
    
    private let modelName = "DBModel"
    private let entityName = "Event"
    private let maxEventAtOnce: Int = 10
    
    private lazy var managedObjectModel: NSManagedObjectModel = {
        let  managedObjectModel = NSManagedObjectModel()
        
        let entity = NSEntityDescription()
        entity.name = entityName
        entity.managedObjectClassName = NSStringFromClass(EventItem.self)

        entity.properties = [
            NSAttributeDescription(name: "date", attributeType: .dateAttributeType),
            NSAttributeDescription(name: "key", attributeType: .stringAttributeType),
            NSAttributeDescription(name: "uuid", attributeType: .UUIDAttributeType),
            NSAttributeDescription(name: "value", attributeType: .stringAttributeType),
        ]
        
        managedObjectModel.entities = [entity]
        return managedObjectModel
    }()
        
    private lazy var persistentStoreCoordinator: NSPersistentStoreCoordinator? = {
        let coordinator = NSPersistentStoreCoordinator(managedObjectModel: managedObjectModel)
        let docDirectory = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)[0]
        let storeURL = docDirectory.appendingPathComponent("\(modelName).sqlite")
        
        do {
            try coordinator.addPersistentStore(ofType: NSSQLiteStoreType, configurationName: nil, at: storeURL, options: nil)
        } catch let error {
            toLog(error: .notAddPersistentStore(error))
        }
        
        return coordinator
    }()
    

    private lazy var context: NSManagedObjectContext = {
        let type = NSManagedObjectContextConcurrencyType.mainQueueConcurrencyType
        let managedObjectContext = NSManagedObjectContext(concurrencyType: type)
        managedObjectContext.persistentStoreCoordinator = persistentStoreCoordinator
        return managedObjectContext
    }()
    
    private func saveContext () {
        guard context.hasChanges else { return }
        do {
            try context.save()
        } catch let error {
            toLog(error: .contextSave(error))
        }
    }
    
    // MARK: - Core Data Saving support

    public func addEventItem(name: String, value: String, date: Date, uuid: UUID) {
        let object = EventItem(context: context)
        object.key = name
        object.date = date
        object.value = value
        object.uuid = uuid
        saveContext()
    }
    
    public func deleteObjects(items: [EventItem]) {
        items.forEach { item in
            context.delete(item)
        }
        saveContext()
    }
    
    
    public func dataEventList() -> [EventItem]? {
        let request: NSFetchRequest<NSFetchRequestResult> = NSFetchRequest(entityName: entityName)
        request.entity = NSEntityDescription.entity(forEntityName: entityName, in: context)
        let departmentSort = NSSortDescriptor(key: "date", ascending: true)
        request.sortDescriptors = [departmentSort]
        request.fetchLimit = maxEventAtOnce
        
        var array: [EventItem] = []
        
        do {
            let objects = try context.execute(request)
            if let result = objects as? NSAsynchronousFetchResult<NSFetchRequestResult> {
                if let res = result.finalResult as? [EventItem] {
                    array = res
                }
            }
        } catch let error {
            toLog(error: .contextExecute(error))
            return nil
        }
        
        return array
    }
    
    public func countEventList() -> Int {
        let fetchRequest: NSFetchRequest<NSFetchRequestResult> = NSFetchRequest(entityName: entityName)
        var count: Int = 0
        do {
            count = try context.count(for: fetchRequest)
        } catch let error {
            toLog(error: .contextExecute(error))
        }
        return count
    }
    
}

extension NSAttributeDescription {
    
    convenience init(name: String, attributeType: NSAttributeType) {
        self.init()
        self.name = name
        self.attributeType = attributeType
        self.isOptional = true
    }
}

extension DBService {
    
    private func toLog(error: DataPlatformError.DBError) {
        print(error)
    }
    
}
