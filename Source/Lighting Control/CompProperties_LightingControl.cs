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
        public bool PawnsInroom =>
#if v1_4
            CurRoom.ContainedAndAdjacentThings.Any(thing => thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Awake());
#else
            CurRoom.ContainedThings<Pawn>().Any(pawn => pawn.RaceProps.Humanlike && pawn.Awake());
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

        public bool AlwaysOn = false;

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

        public override void CompTickRare()
        {
            if (AlwaysOn)
            {
                Allowed = true;
                return;
            }
            if (LightingControl.settings.alwaysOnInRoomWithPawns && !CurRoom.PsychologicallyOutdoors)
            {
                Allowed = PawnsInroom;
                return;
            }
            var glowGrid = parent.Map.glowGrid;
            if (Glower.Glows)
            {
                glowGrid.DeRegisterGlower(Glower);
                glowGrid.GlowGridUpdate_First();
            }
#if v1_4
            var glowLevel = glowGrid.GameGlowAt(GlowPosition);
#else
            var glowLevel = glowGrid.GroundGlowAt(GlowPosition);
#endif
            if (Glower.Glows)
            {
                glowGrid.RegisterGlower(Glower);
                glowGrid.GlowGridUpdate_First();
            }
            var result = glowLevel < Props.threshold;
            if (result && !CurRoom.PsychologicallyOutdoors)
                result = PawnsInroom;
            Allowed = result;
        }

        public override string CompInspectStringExtra() => Allowed ? null : Props.offMessage;
    }
}
