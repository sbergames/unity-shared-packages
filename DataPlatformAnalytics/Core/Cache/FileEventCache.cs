using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SberGames.DataPlatform.Core
{
    public class FileEventCache : IEventCache
    {
        private const string CacheDirectory = "EventsCache";
        private const string EventsFile = ".events";
        private const string SentEventsFile = ".sent";
        private const char Delimiter = ':';
        
        private IFileWriter cacheWriter = null;
        private IFileWriter sentWriter = null;
        
        private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        private int currentCacheFileUnsentEventCount = 0;

        public Action OnUnsentEventsLoaded { get; set; }

        public bool IsFilesClosed => cacheWriter.IsClosed && sentWriter.IsClosed;
        
        public FileEventCache()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, CacheDirectory);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            LoadSavedData(directoryPath);

            string fileName = DateTime.UtcNow.ToString("yyyy-MM-dd") + "_" + GUIDGenerator.Generate();
            cacheWriter = new ParallelFileWriter();
            cacheWriter.Open(Path.Combine(directoryPath, fileName + EventsFile));
            
            sentWriter = new ParallelFileWriter();
            sentWriter.Open(Path.Combine(directoryPath, fileName + SentEventsFile));
        }

        public void Add(string evetnId, string eventData)
        {
            cache.Add(evetnId, eventData);
            cacheWriter.WriteLine(evetnId + Delimiter + eventData);
            currentCacheFileUnsentEventCount++;
        }

        public void Remove(string eventId)
        {
            cache.Remove(eventId);
            sentWriter.WriteLine(eventId);
            currentCacheFileUnsentEventCount--;
        }

        public IEnumerable<KeyValuePair<string, string>> UnsentEvents()
        {
            return cache;
        }
        
        public void Dispose()
        {
            if (currentCacheFileUnsentEventCount == 0)
            {
                cacheWriter.CloseAndRemove();
                sentWriter.CloseAndRemove();
            }
            else
            {
                cacheWriter.Close();
                sentWriter.Close();
            }
        }

        private async void LoadSavedData(string directoryPath)
        {
            List<KeyValuePair<string, string>> files = GetCacheFiles(directoryPath);
            
            var result = await Task.Run(() => ReadCache(files));

            foreach (var eventData in result)
            {
                cache.Add(eventData.Key, eventData.Value);
            }
            
            OnUnsentEventsLoaded?.Invoke();
        }

        private Dictionary<string, string> ReadCache(List<KeyValuePair<string, string>> files)
        {
            Dictionary<string, string> localCache = new Dictionary<string, string>();
            
            foreach (var filePair in files)
            {
                var sentEventIds = ReadSentEventIds(filePair.Key);
                
                if (!ReadUnsentEvents(filePair.Value, sentEventIds, ref localCache))
                {
                    // all events from this file have been sent
                    File.Delete(filePair.Key);
                    File.Delete(filePair.Value);
                }
            }

            return localCache;
        }

        private bool ReadUnsentEvents(string filePath, List<string> sentEventIds, ref Dictionary<string, string> localCache)
        {
            bool unsentEventsFound = false;
            var eventsReader = new StreamReader(filePath);

            while (!eventsReader.EndOfStream)
            {
                var eventLine = eventsReader.ReadLine();
                var eventId = eventLine.Substring(0, eventLine.IndexOf(Delimiter));

                if (!sentEventIds.Contains(eventId))
                {
                    var eventData = eventLine.Remove(0, eventId.Length + 1);
                    localCache.Add(eventId, eventData);
                    unsentEventsFound = true;
                }
            }

            eventsReader.Close();

            return unsentEventsFound;
        }
        
        private List<KeyValuePair<string, string>> GetCacheFiles(string directoryPath)
        {
            List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();
            
            string [] fileEntries = Directory.GetFiles(directoryPath);
            foreach (string fileName in fileEntries)
            {
                if (fileName.Contains(SentEventsFile))
                {
                    files.Add(new KeyValuePair<string, string>(fileName, fileName.Replace(SentEventsFile, EventsFile)));
                }
            }

            return files;
        }

        private List<string> ReadSentEventIds(string filePath)
        {
            var sentEventIds = new List<string>();
            var sentReader = new StreamReader(filePath);

            while (!sentReader.EndOfStream)
            {
                sentEventIds.Add(sentReader.ReadLine());
            }
            sentReader.Close();

            return sentEventIds;
        }
    }
}