using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Biblioteka;
using System.Text;
using System.Windows.Controls;
using System.Globalization;
using System.ServiceProcess;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Threading;
using System.Timers;
using System.Threading;


namespace KatalogowaniePlikow
{
    public partial class MainWindow : Window
    {
        private readonly Class1 cataloger = new Class1();
        public ServiceController serviceController;
        public static string sciezka = ConfigurationManager.AppSettings["SetupSource"];
        public static string usluga = ConfigurationManager.AppSettings["ServiceName"];
        public static string dziennik = ConfigurationManager.AppSettings["LogName"];
        public static string sciezkaDziennika = ConfigurationManager.AppSettings["LogSource"];
        public static string sciezkaZapisu = ConfigurationManager.AppSettings["SaveSource"];
        public static string sciezkaWpisu = ConfigurationManager.AppSettings["SearchSource"];
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateButtonsAndStatusLabel();
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "Wybierz folder";
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Folder z plikami";

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = Path.GetDirectoryName(dialog.FileName);
                txtDirectoryPath.Text = selectedPath;

                // Zapisz wybraną ścieżkę do pliku tekstowego
                File.WriteAllText(sciezkaZapisu, selectedPath);
            }
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = txtDirectoryPath.Text;
            string searchPattern = txtSearchPattern.Text;
            string namePattern = txtNamePattern.Text;
            bool searchSubdirectories = chkSearchSubdirectories.IsChecked ?? false;

            try
            {
                cataloger.SetSearchSubdirectories(searchSubdirectories);

                List<string> files = cataloger.CatalogFiles(directoryPath, searchPattern);
                
                // Filtruj pliki na podstawie wzorca nazwy
                files = cataloger.FilterFilesByName(files, namePattern);

                long? minSize = null;
                long? maxSize = null;

                // Sprawdź, czy jakiekolwiek pole dotyczące rozmiaru plików zostało wypełnione
                if (!string.IsNullOrWhiteSpace(txtMinSize.Text) || !string.IsNullOrWhiteSpace(txtMaxSize.Text))
                {
                    // Parsuj minimalny i maksymalny rozmiar plików
                    if (!long.TryParse(txtMinSize.Text, out long minSizeValue))
                    {
                        MessageBox.Show("Podano nieprawidłową wartość dla minimalnego rozmiaru pliku.", "Błąd");
                        return;
                    }
                    minSize = minSizeValue;

                    if (!long.TryParse(txtMaxSize.Text, out long maxSizeValue))
                    {
                        MessageBox.Show("Podano nieprawidłową wartość dla maksymalnego rozmiaru pliku.", "Błąd");
                        return;
                    }
                    maxSize = maxSizeValue;

                    // Wybierz jednostkę rozmiaru, jeśli została wybrana
                    string unit = ((ComboBoxItem)cmbSizeUnit.SelectedItem)?.Content.ToString();
                    if (unit != null)
                    {
                        // Konwertuj minimalny i maksymalny rozmiar do bajtów
                        minSize = ConvertToBytes(minSize.Value, unit);
                        maxSize = ConvertToBytes(maxSize.Value, unit);
                    }
                }

                // Filtruj pliki na podstawie rozmiaru, jeśli jakiekolwiek pole dotyczące rozmiaru plików zostało wypełnione
                if (minSize != null || maxSize != null)
                {
                    files = cataloger.FilterFilesBySize(files, minSize, maxSize);
                }
                else
                {
                    // Wyświetl ostrzeżenie, że nie wprowadzono kryteriów filtrowania
                    MessageBox.Show("Nie wprowadzono żadnych kryteriów filtrowania. Wyświetlam wszystkie pliki.", "Ostrzeżenie");
                }

                string exactDate = txtModifiedDate.Text;

                // Jeśli użytkownik wprowadził datę modyfikacji, sprawdź jej poprawność
                if (!string.IsNullOrWhiteSpace(exactDate))
                {
                    // Spróbuj przekonwertować ciąg znaków na datę w odpowiednim formacie
                    if (!DateTime.TryParseExact(exactDate, txtDateFormat.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        MessageBox.Show("Podano nieprawidłową datę.", "Błąd");
                        return;
                    }

                    // Jeśli udało się przekonwertować, zastosuj filtrowanie
                    files = cataloger.FilterFilesByExactModifiedDate(files, parsedDate);
                }

                Dictionary<string, List<string>> groupedFiles = cataloger.GroupFilesByExtension(files);

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), sciezkaWpisu);
                cataloger.SaveToFile(groupedFiles, filePath);

                MessageBox.Show($"Wyniki wyszukiwania zostały zapisane do pliku: {filePath}", "Informacja");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas wyszukiwania plików: {ex.Message}", "Błąd");
            }
        }

        public void AddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wszystkie pliki (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    string destinationDirectory = txtDirectoryPath.Text;
                    cataloger.AddFile(filePath, destinationDirectory);
                    MessageBox.Show($"Plik został przeniesiony do katalogu: {destinationDirectory}", "Informacja");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas przenoszenia pliku: {ex.Message}", "Błąd");
                }
            }
        }

        public void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wszystkie pliki (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    cataloger.DeleteFile(filePath);
                    MessageBox.Show($"Plik został usunięty.", "Informacja");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas usuwania pliku: {ex.Message}", "Błąd");
                }
            }
        }
        public void CreateDirectory_Click(object sender, RoutedEventArgs e)
        {
            string parentDirectory = txtDirectoryPath.Text;
            string newDirectoryName = txtNewDirectoryName.Text;

            try
            {
                cataloger.CreateDirectory(parentDirectory, newDirectoryName);
                MessageBox.Show($"Katalog został utworzony: {Path.Combine(parentDirectory, newDirectoryName)}", "Informacja");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas tworzenia katalogu: {ex.Message}", "Błąd");
            }
        }

        public void DeleteDirectory_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = txtDirectoryPath.Text;

            try
            {
                cataloger.DeleteDirectory(directoryPath);
                MessageBox.Show($"Katalog został usunięty wraz z zawartością.", "Informacja");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas usuwania katalogu: {ex.Message}", "Błąd");
            }
        }


        private void FilterByName_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = txtDirectoryPath.Text;
            string searchPattern = txtSearchPattern.Text;

            try
            {
                List<string> files = cataloger.CatalogFiles(directoryPath, searchPattern);
                List<string> filteredFiles = cataloger.FilterFilesByName(files, txtNamePattern.Text);

                // Wyświetl listę przefiltrowanych plików
                StringBuilder sb = new StringBuilder();
                foreach (string file in filteredFiles)
                {
                    sb.AppendLine(file);
                }
                MessageBox.Show(sb.ToString(), "Przefiltrowane pliki", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas filtrowania plików: {ex.Message}", "Błąd");
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateButtonsAndStatusLabel();
        }

        private void UpdateButtonsAndStatusLabel()
        {
            serviceController = new ServiceController(usluga);
            serviceController.Refresh();
            if (serviceController != null)
            {
                ServiceControllerStatus status = serviceController.Status;
                switch (status)
                {
                    case ServiceControllerStatus.Running:
                        Stan_uslugi.Text = "Stan usługi: uruchomiono";
                        Uruchom_usluge.IsEnabled = false;
                        Zatrzymaj_usluge.IsEnabled = true;
                        break;
                    case ServiceControllerStatus.Stopped:
                        Stan_uslugi.Text = "Stan usługi: zatrzymano";
                        Uruchom_usluge.IsEnabled = true;
                        Zatrzymaj_usluge.IsEnabled = false;
                        break;
                    default:
                        break;
                }
            }
        }

        private void StartService_Click(object sender, RoutedEventArgs e)
        {
            serviceController = new ServiceController(usluga);
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);
            UpdateButtonsAndStatusLabel();
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            UpdateButtonsAndStatusLabel(); 
        }
        public static long ConvertToBytes(long size, string unit)
        {
            switch (unit)
            {
                case "B":
                    return size;
                case "KB":
                    return size * 1024;
                case "MB":
                    return size * 1024 * 1024;
                case "GB":
                    return size * 1024 * 1024 * 1024;
                default:
                    throw new ArgumentException("Nieprawidłowa jednostka rozmiaru pliku.");
            }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // Wyczyść zawartość pól tekstowych i listy rozwijanej
            txtDirectoryPath.Text = string.Empty;
            txtSearchPattern.Text = "*.*";
            txtNamePattern.Text = string.Empty;
            txtMinSize.Text = string.Empty;
            txtMaxSize.Text = string.Empty;
            chkSearchSubdirectories.IsChecked = false;
            txtNewDirectoryName.Text = string.Empty;
            txtModifiedDate.Text = string.Empty;

            // Wyczyść wybrane elementy w liście rozwijanej (jeśli istnieją)
            cmbSizeUnit.SelectedIndex = -1;
        }
        private static bool CzyZainstalowana(string serviceName)
        {
            return ServiceController.GetServices().Any(service => service.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
