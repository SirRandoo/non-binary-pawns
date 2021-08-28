using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace SirRandoo.NonBinary
{
    [UsedImplicitly]
    public class GenderlessMod : Mod
    {
        public GenderlessMod(ModContentPack content) : base(content)
        {
            GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public override void DoSettingsWindowContents(Rect region)
        {
            Settings.Draw(region);
        }
    }
}
