using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Biblioteka
{
    public class Class1
    {
        private bool searchSubdirectories;

        public void SetSearchSubdirectories(bool value)
        {
            searchSubdirectories = value;
        }

        public List<string> CatalogFiles(string directoryPath, string searchPattern = "*")
        {
            List<string> fileList = new List<string>();

            if (Directory.Exists(directoryPath))
            {
                SearchOption searchOption = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                fileList.AddRange(Directory.GetFiles(directoryPath, searchPattern, searchOption));
            }
            else
            {
                throw new DirectoryNotFoundException("Podana ścieżka katalogu jest nieprawidłowa lub nie istnieje.");
            }

            return fileList;
        }

        public Dictionary<string, List<string>> GroupFilesByExtension(List<string> files)
        {
            return files.GroupBy(file => Path.GetExtension(file).ToLower())
                        .ToDictionary(group => group.Key, group => group.OrderBy(file => file).ToList());
        }

        public void SaveToFile(Dictionary<string, List<string>> groupedFiles, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var group in groupedFiles)
                {
                    writer.WriteLine($"Liczba plików z rozszerzeniem {group.Key}: {group.Value.Count}");
                    writer.WriteLine($"Pliki z rozszerzeniem {group.Key}:");
                    foreach (var file in group.Value)
                    {
                        writer.WriteLine(file);
                    }
                    writer.WriteLine();
                }
            }
        }

        public void AddFile(string sourceFilePath, string destinationDirectory)
        {
            string fileName = Path.GetFileName(sourceFilePath);
            string destinationPath = Path.Combine(destinationDirectory, fileName);

            if (File.Exists(destinationPath))
            {
                throw new IOException("Plik o tej nazwie już istnieje w katalogu docelowym.");
            }

            File.Copy(sourceFilePath, destinationPath);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }
        public void CreateDirectory(string parentDirectory, string newDirectoryName)
        {
            string path = Path.Combine(parentDirectory, newDirectoryName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"Utworzono katalog: {path}");
            }
            else
            {
                Console.WriteLine($"Katalog już istnieje: {path}");
            }
        }

        public void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Ścieżka {directoryPath} nie istnieje.");
            }

            Directory.Delete(directoryPath, true);
        }

        public List<string> FilterFilesByName(List<string> files, string namePattern)
        {
            return files.Where(file => Path.GetFileName(file).Contains(namePattern)).ToList();
        }


        public List<string> FilterFilesBySize(List<string> files, long? minSize, long? maxSize)
        {
            if (minSize.HasValue && maxSize.HasValue)
            {
                return files.Where(file =>
                {
                    long fileSize = new FileInfo(file).Length;
                    return fileSize >= minSize && fileSize <= maxSize;
                }).ToList();
            }
            else if (minSize.HasValue)
            {
                return files.Where(file =>
                {
                    long fileSize = new FileInfo(file).Length;
                    return fileSize >= minSize;
                }).ToList();
            }
            else if (maxSize.HasValue)
            {
                return files.Where(file =>
                {
                    long fileSize = new FileInfo(file).Length;
                    return fileSize <= maxSize;
                }).ToList();
            }
            else
            {
                return files; // Zwróć wszystkie pliki jeśli nie podano ograniczeń rozmiaru
            }
        }

        public List<string> FilterFilesByExactModifiedDate(List<string> files, DateTime exactModifiedDate)
        {
            // Filtruj pliki na podstawie dokładnej daty modyfikacji
            return files.Where(file => File.GetLastWriteTime(file).Date == exactModifiedDate.Date).ToList();
        }

    }
}
