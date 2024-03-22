using UnityEngine;
using Verse;

namespace Lighting_Control
{
    class Settings : ModSettings
    {
        public bool alwaysOnInRoomWithPawns;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref alwaysOnInRoomWithPawns, "alwaysOnInRoomWithPawns");
        }

        public void DoWindowContents(Rect inRect)
        {
            var height = 28f;
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            var rowRect = ls.GetRect(height);
            var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
            row.Label("LightingControl.AlwaysOnInRoomWithPawns".Translate());
            var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(alwaysOnInRoomWithPawns ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                alwaysOnInRoomWithPawns = !alwaysOnInRoomWithPawns;
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
