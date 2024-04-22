using UnityEngine;
using Verse;

namespace Lighting_Control
{
    class Settings : ModSettings
    {
        public bool alwaysOnInDoorWithPawns;
        public bool alwaysOnOutDoorAtNight;
        public bool advancedLightRequired;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref alwaysOnInDoorWithPawns, "alwaysOnInDoorWithPawns");
        }

        public void DoWindowContents(Rect inRect)
        {
            var height = 28f;
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("LightingControl.AdvancedLightRequired".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(advancedLightRequired ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    advancedLightRequired = !advancedLightRequired;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("LightingControl.AlwaysOnInDoorWithPawns".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(alwaysOnInDoorWithPawns ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    alwaysOnInDoorWithPawns = !alwaysOnInDoorWithPawns;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("LightingControl.AlwaysOnOutDoorAtNight".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(alwaysOnOutDoorAtNight ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    alwaysOnOutDoorAtNight = !alwaysOnOutDoorAtNight;
            }
            ls.End();
        }
    }

    class LightingControl : Mod
    {
        public static Settings settings;

        public LightingControl(ModContentPack content)
            : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

        public override string SettingsCategory() => "LightingControl".Translate();
    }
}
