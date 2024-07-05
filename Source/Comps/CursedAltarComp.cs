using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;
using XenoRitual.Helpers;

namespace XenoRitual.Comps
{
    public class CursedAltarComp : CompUsable
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.CanReserve(parent))
            {
                yield break;
            }
            else if (pawn.Map != null && pawn.Map == Find.CurrentMap)
            {
                var convertTargets = pawn.Map.mapPawns.AllPawns.Where(p => IsValidConvertOption(p, pawn)).Except(pawn).ToList();
                if (convertTargets.Count == 0)
                {
                    var caption = "No valid target. Everyone are the same Xenotype... I think.";
                    yield return new FloatMenuOption(caption, null, MenuOptionPriority.DisabledOption);
                }
                
                List<Thing> MeatStacks = new List<Thing>();
                int needed = StaticModVariables.ResourceAmmount;

                var MeatsOnSpot = parent.Map.thingGrid.ThingsListAt(parent.InteractionCell).FirstOrDefault(x => x.def == Defs.Resource);
                if (MeatsOnSpot != null && MeatsOnSpot.def == Defs.Resource)
                {
                    needed -= MeatsOnSpot.stackCount;
                }
                if (needed > 0)
                    MeatStacks.AddRange(parent.Map.listerThings.ThingsOfDef(Defs.Resource).Except(MeatsOnSpot));
                var thingCountList = new List<ThingCount>();
                if (TryGetClosestMeat(pawn.Position, MeatStacks, thingCountList, needed))
                    foreach (var convertable in convertTargets)
                    {
                        yield return CreateConvertOption(pawn, convertable, thingCountList, MeatsOnSpot);
                    }
                else
                {
                    var caption = $"We have no more gifts to sacrifice. We need at least {needed}.";
                    yield return new FloatMenuOption(caption, null, MenuOptionPriority.DisabledOption);
                }
            }
        }
        private static bool TryGetClosestMeat(IntVec3 rootCell, List<Thing> availableThings, List<ThingCount> chosen,int needed)
        {
            if (needed == 0)
                return true;
            Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
            {
                float num5 = (float)(t1.PositionHeld - rootCell).LengthHorizontalSquared;
                float value = (float)(t2.PositionHeld - rootCell).LengthHorizontalSquared;
                return num5.CompareTo(value);
            };
            availableThings.Sort(comparison);
            while (availableThings.Count != 0)
            {
                chosen.Add(new ThingCount(availableThings[0], Math.Min(availableThings[0].stackCount, needed)));
                needed -= chosen.Last().Count;
                availableThings.RemoveAt(0);
                if (needed <= 0)
                    return true;
            }
            return false;

        }
        private bool IsValidConvertOption(Pawn converting, Pawn ritualist)
        {

            return converting.IsHumanlike() && converting.IsColonist
                && (converting.genes.XenotypeLabel != ritualist.genes.XenotypeLabel);

        }
        protected FloatMenuOption CreateConvertOption(Pawn sacrificer, Pawn converting, List<ThingCount> Meats,Thing MeatsOnSpot =null)
        {
            var caption = $"convert {converting.Name} to {sacrificer.genes.XenotypeLabel}";
            return new FloatMenuOption(caption, () =>
            {
                var job = JobMaker.MakeJob(Defs.ConvertXenotype, parent, MeatsOnSpot, converting);
                job.targetQueueB = new List<LocalTargetInfo>(Meats.Count);
                job.countQueue = new List<int>(Meats.Count);
                for (int i = 0; i < Meats.Count; i++)
                {
                    job.targetQueueB.Add(Meats[i].Thing);
                    job.countQueue.Add(Meats[i].Count);
                }
                job.count = 1;
                job.haulMode = HaulMode.ToCellNonStorage;
                sacrificer.jobs.TryTakeOrderedJob(job);
            });
        }
    }
}
