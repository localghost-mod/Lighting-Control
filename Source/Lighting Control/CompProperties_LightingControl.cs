using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lighting_Control
{
    class CompProperties_LightingControl : CompProperties
    {
        public CompProperties_LightingControl() => compClass = typeof(CompLightingControl);

        public float threshold = 0.5f;
        public string offMessage => "LightingControl.OffMessage".Translate();
    }

    class CompLightingControl : ThingComp
    {
        public CompProperties_LightingControl Props
        {
            get => (CompProperties_LightingControl)props;
        }
        public CompGlower Glower
        {
            get => glower != null ? glower : (glower = parent.GetComp<CompGlower>());
        }
        CompGlower glower;
        public IntVec3 GlowPosition => parent.Position;
        public Room CurRoom => parent.GetRoom();
        public bool PawnsIndoor =>
#if v1_4
            CurRoom.ContainedAndAdjacentThings.Any(thing => thing is Pawn pawn && (pawn.RaceProps.Humanlike || pawn.RaceProps.IsMechanoid) && pawn.Awake());
#else
            CurRoom.ContainedThings<Pawn>().Any(pawn => (pawn.RaceProps.Humanlike || pawn.RaceProps.IsMechanoid) && pawn.Awake());
#endif

        public bool Allowed
        {
            get => allowed;
            set
            {
                if (allowed != value)
                {
                    allowed = value;
                    parent.BroadcastCompSignal(allowed ? "ScheduledOn" : "ScheduledOff");
                    var powerTrader = parent.GetComp<CompPowerTrader>();
                    if (powerTrader != null)
                        powerTrader.PowerOutput = Allowed ? -powerTrader.Props.PowerConsumption : 0;
                }
            }
        }
        bool allowed = true;
        float CurSkyGlow => parent.Map.skyManager.CurSkyGlow;

        public bool AlwaysOn = false;

        public override void PostExposeData() => Scribe_Values.Look(ref AlwaysOn, "alwaysOn");

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Lighting_Control.AlwaysOnLabel".Translate(),
                defaultDesc = "Lighting_Control.AwalysOnDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"),
                isActive = () => AlwaysOn,
                toggleAction = () => AlwaysOn = !AlwaysOn
            };
        }
        public override void CompTick()
        {
            if (Find.TickManager.TicksGame % 250 == 0)
                CompTickRare();
        }

        public override void CompTickRare()
        {
            if (AlwaysOn || (LightingControl.settings.advancedLightRequired && !ResearchProjectDefOf.ColoredLights.IsFinished))
            {
                Allowed = true;
                return;
            }
            if (LightingControl.settings.alwaysOnInDoorWithPawns && !CurRoom.PsychologicallyOutdoors)
            {
                Allowed = PawnsIndoor;
                return;
            }
            if (LightingControl.settings.alwaysOnOutDoorAtNight && CurRoom.PsychologicallyOutdoors)
            {
                Allowed = CurSkyGlow < Props.threshold;
                return;
            }
            var glowGrid = parent.Map.glowGrid;
            if (Glower.Glows)
            {
                glowGrid.DeRegisterGlower(Glower);
#if v1_4
                glowGrid.GlowGridUpdate_First();
#endif
            }
#if v1_4
            var glowLevel = glowGrid.GameGlowAt(GlowPosition);
#else
            var glowLevel = glowGrid.GroundGlowAt(GlowPosition);
#endif
            if (Glower.Glows)
            {
                glowGrid.RegisterGlower(Glower);
#if v1_4
                glowGrid.GlowGridUpdate_First();
#endif
            }
            var result = glowLevel < Props.threshold;
            if (result && !CurRoom.PsychologicallyOutdoors)
                result = PawnsIndoor;
            Allowed = result;
        }

        public override string CompInspectStringExtra() => Allowed ? null : Props.offMessage;
    }
}
