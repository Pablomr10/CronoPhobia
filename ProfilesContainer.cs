using System.Collections.Generic;
using System.Linq;

namespace Cronophobia
{
    public class ProfilesContainer
    {
        public List<ProfileSettings> Profiles { get; set; } = new();
        public string ActiveProfileName { get; set; } = "New profile";

        public void EnsureDefaultProfile()
        {
            // eliminar defaults duplicados
            var defaults = Profiles.Where(p => p.IsDefault).ToList();
            if (defaults.Count > 1)
            {
                var keep = defaults[0];
                foreach (var extra in defaults.Skip(1))
                    Profiles.Remove(extra);
            }

            // crear default si no existe
            if (!Profiles.Any(p => p.IsDefault))
            {
                Profiles.Insert(0, new ProfileSettings
                {
                    ProfileName = "New profile",
                    IsDefault = true
                });
            }

            // forzar nombre correcto
            var def = Profiles.First(p => p.IsDefault);
            def.ProfileName = "New profile";
        }

        public ProfileSettings GetActiveProfile()
        {
            return Profiles.FirstOrDefault(p => p.ProfileName == ActiveProfileName)
                   ?? Profiles.First();
        }
    }
}
