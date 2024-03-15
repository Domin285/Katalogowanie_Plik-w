using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using Biblioteka;

namespace Usluga
{
    public partial class Service1 : ServiceBase
    {
        private FileSystemWatcher watcher;
        public static string monitoredDirectory; // Ścieżka do monitorowanego katalogu
        public static string dziennik = ConfigurationManager.AppSettings["LogName"];
        public static string sciezkaDziennika = ConfigurationManager.AppSettings["LogSource"];
        public static string usluga = ConfigurationManager.AppSettings["ServiceName"];
        public static string sciezkaZapisu = ConfigurationManager.AppSettings["SaveSource"];
        public static EventLog eventLog;

        public Service1()
        {
            this.ServiceName = usluga;

            // Odczytaj ścieżkę monitorowanego katalogu z pliku tekstowego
            monitoredDirectory = File.ReadAllText(sciezkaZapisu);
        }
        public static void UtworzDziennik()
        {
            if (!EventLog.SourceExists(sciezkaDziennika, "."))
            {
                EventLog.CreateEventSource(sciezkaDziennika, dziennik);
            }

            eventLog = new EventLog(dziennik);

            eventLog.Source = sciezkaDziennika;
       }
        protected override void OnStart(string[] args)
        {
            UtworzDziennik();

            // Inicjalizacja i konfiguracja FileSystemWatcher
            watcher = new FileSystemWatcher();
            watcher.Path = monitoredDirectory;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            // Dodanie obsługi zdarzeń
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileMoved;
        }

        protected override void OnStop()
        {
            // Zatrzymanie monitorowania i zwolnienie zasobów
            watcher.Dispose();
        }

        public void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string message = $"Utworzono katalog: {e.FullPath}";
            eventLog.WriteEntry(message, EventLogEntryType.Information);
        }

        public void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            string message = $"Usunięto plik/katalog: {e.FullPath}";
            eventLog.WriteEntry(message, EventLogEntryType.Information);
        }

        private void OnFileMoved(object sender, RenamedEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                try
                {
                    string oldPath = e.OldFullPath;
                    string newPath = e.FullPath;

                    // Sprawdź, czy ścieżki są różne, co wskazuje na przeniesienie pliku
                    if (oldPath != newPath)
                    {
                        // Przenieś plik do nowego miejsca
                        File.Move(oldPath, newPath);

                        // Usuń plik ze starej ścieżki
                        if (File.Exists(oldPath))
                        {
                            File.Delete(oldPath);
                        }

                        string message = $"Przeniesiono plik z {oldPath} do {newPath}";
                        eventLog.WriteEntry(message, EventLogEntryType.Information);
                    }
                    else
                    {
                        // Obsługa normalnego utworzenia pliku (nie przeniesienia)
                        string message = $"Utworzono katalog: {e.FullPath}";
                        eventLog.WriteEntry(message, EventLogEntryType.Information);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Wystąpił błąd podczas przenoszenia lub usuwania pliku: {ex.Message}";
                    eventLog.WriteEntry(errorMessage, EventLogEntryType.Error);
                }
            }
        }


    }
}
