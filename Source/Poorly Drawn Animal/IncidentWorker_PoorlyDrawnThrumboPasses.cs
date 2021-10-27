using System;
using UnityEngine;
using RimWorld;
using Verse;

namespace PoorlyDrawnAnimal
{
    public class IncidentWorker_PoorlyDrawnThrumboPasses : IncidentWorker
    {
        int MaxCount = 4;
        int MinCount = 1;

        // look into making a thingdef class for this mod
        public static ThingDef Thing_PThrumbo = DefDatabase<ThingDef>.GetNamed("Poorly_Drawn_Thrumbo");
        public static PawnKindDef Pawn_PThrumbo = DefDatabase<PawnKindDef>.GetNamed("Poorly_Drawn_Thrumbo");

        // details when the incident can and cannot happen
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
                return false;

            if (!map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(Thing_PThrumbo))
                return false;

            // see if there is a spot to spawn
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 intVec, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null))
                return false;

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 VecSpawn;
            IntVec3 VecWalk = IntVec3.Invalid;
            // randomly generate number of pthrumbo spawns
            int SpawnNum = Mathf.Clamp(GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow(map) / Pawn_PThrumbo.combatPower), MinCount, MaxCount);
            int TicksToLeave = Rand.RangeInclusive(90000, 150000);

            // get spawn location
            if (!RCellFinder.TryFindRandomPawnEntryCell(out VecSpawn, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null))
                return false;

            // check if it is possible to walk to center of map from spawn location
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(VecSpawn, map, 10f, out VecWalk))
                VecWalk = IntVec3.Invalid; // need to reset walkvec

            Pawn pt = null;
            for (int i = 0; i < SpawnNum; i++)
            {
                // generate and spawn pawn
                pt = PawnGenerator.GeneratePawn(Pawn_PThrumbo, null);
                IntVec3 VecSpreadTo = CellFinder.RandomClosewalkCellNear(VecSpawn, map, 10, null); // wear to spread out to
                GenSpawn.Spawn(pt, VecSpreadTo, map, Rot4.Random); // note: Rot4 value is the rotation of the spawned pawn.
                pt.mindState.exitMapAfterTick = Find.TickManager.TicksGame + TicksToLeave;

                // walk to center if possible
                if (VecWalk.IsValid)
                    pt.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(VecWalk, map, 10, null);
            }

            base.SendStandardLetter("Poorly Drawn Thrumbo Passes", "Poorly Drawn Thrumbo Passes", LetterDefOf.PositiveEvent, parms, pt, Array.Empty<NamedArgument>());

            return true;
        }
    }
}
