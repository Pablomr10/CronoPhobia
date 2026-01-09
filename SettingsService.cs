using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Cronophobia
{
    public static class SettingsService
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json");

        public static ProfilesContainer Load()
        {
            ProfilesContainer container;

            if (!File.Exists(FilePath))
            {
                container = new ProfilesContainer();
                container.EnsureDefaultProfile();
                Save(container);
                return container;
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                container = JsonSerializer.Deserialize<ProfilesContainer>(json)
                            ?? new ProfilesContainer();
            }
            catch
            {
                container = new ProfilesContainer();
            }

            container.EnsureDefaultProfile();
            container.ActiveProfileName = container.GetActiveProfile().ProfileName;

            return container;
        }


        public static void Save(ProfilesContainer container)
        {
            SanitizeProfiles(container);

            var json = JsonSerializer.Serialize(
                container,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(FilePath, json);
        }

        // ================= DEFAULT / TEMPLATE =================
        private static ProfilesContainer CreateInitialContainer()
        {
            var template = new ProfileSettings
            {
                ProfileName = "New profile",
                IsDefault = true,

                // Valores base SOLO visuales
                ShowTitle = true,
                ShowIcons = true,
                TitleFontSize = 48,
                TimerFontSize = 120,
                TextColor = "White",

                // Posición neutra (NO importa, no se guarda)
                WindowLeft = 0,
                WindowTop = 0
            };

            return new ProfilesContainer
            {
                ActiveProfileName = "New profile",
                Profiles = new List<ProfileSettings> { template }
            };
        }

        // ================= SANITIZE =================
        private static void SanitizeProfiles(ProfilesContainer container)
        {
            // 1. Solo UN perfil default
            var defaults = container.Profiles.Where(p => p.IsDefault).ToList();
            if (defaults.Count == 0)
            {
                container.Profiles.Insert(0, CreateInitialContainer().Profiles[0]);
            }
            else if (defaults.Count > 1)
            {
                var keep = defaults.First();
                foreach (var extra in defaults.Skip(1))
                    container.Profiles.Remove(extra);
            }

            // 2. Forzar nombre y flags del default
            var def = container.Profiles.First(p => p.IsDefault);
            def.ProfileName = "New profile";
            def.IsDefault = true;

            // 3. Máximo 4 perfiles (sin contar el default)
            var nonDefault = container.Profiles.Where(p => !p.IsDefault).ToList();
            if (nonDefault.Count > 4)
            {
                foreach (var extra in nonDefault.Skip(4))
                    container.Profiles.Remove(extra);
            }
        }
    }
}
