using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Lighting_Control
{
    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup()
        {
            new Harmony("localghost.lightingcontrol").PatchAll();
            PatchLamps();
        }

        static void PatchLamps()
        {
            var lamps = DefDatabase<ThingDef>.AllDefsListForReading.Where(
                def =>
                    (def.defName.Contains("Lamp") || def.defName.Contains("Light") || def.defName.Contains("Dark") || def.defName.Contains("CeilingFan"))
                    && def.HasComp(typeof(CompPowerTrader))
                    && def.HasComp(typeof(CompGlower))
                    && !def.HasComp(typeof(CompSchedule))
            );
            foreach (var lamp in lamps)
            {
                if (lamp.tickerType == TickerType.Never)
                    lamp.tickerType = TickerType.Rare;
                lamp.comps.Add(new CompProperties_LightingControl());
            }
            Log.Message($"Patched:\n\t{string.Join("\n\t", lamps)}");
        }
    }

    [HarmonyPatch("RimWorld.FlickUtility", "WantsToBeOn")]
    class WantsToBeOnPatch
    {
        static void Postfix(ref bool __result, Thing t)
        {
            if (!(t.TryGetComp<CompLightingControl>()?.Allowed ?? true))
                __result = false;
        }
    }
}
