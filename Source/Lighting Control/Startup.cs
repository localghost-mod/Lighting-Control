using HarmonyLib;
using Verse;

namespace Lighting_Control
{
    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup()
        {
            new Harmony("localghost.lightingcontrol").PatchAll();
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
}
