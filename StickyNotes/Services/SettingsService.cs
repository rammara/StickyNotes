using System;
using System.IO;
using System.Text.Json;
using StickyNotes.Models;

namespace StickyNotes.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;

        public SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "StickyNotes");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
        } // SettingsService

        public bool SettingsLoaded { get; private set; }
        public SettingsModel? Current { get; private set; }

        public SettingsModel LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    Current = JsonSerializer.Deserialize<SettingsModel>(json) ?? new SettingsModel();
                    SettingsLoaded = true;
                    return Current;
                }
            }
            catch (Exception)
            {
                // return defaulsts
            }

            return new SettingsModel();
        } // LoadSettings

        private readonly JsonSerializerOptions _SerializationOptions = new() { WriteIndented = true };

        public void SaveSettings(SettingsModel settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, _SerializationOptions);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                // Обработка ошибки сохранения
                System.Diagnostics.Debug.WriteLine($"Error while saving the settings: {ex.Message}");
            }
        } // SaveSettings
    } // SettingsService
}