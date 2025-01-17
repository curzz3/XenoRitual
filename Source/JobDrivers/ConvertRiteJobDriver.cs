﻿using HugsLib.Settings;
using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;
using XenoRitual.Helpers;

namespace XenoRitual.JobDrivers
{
    public class ConvertRiteJobDriver : JobDriver
    {
        //public readonly TargetIndex iPawn = TargetIndex.A;
        public readonly TargetIndex iSacrificeBuilding = TargetIndex.A; // Any sacrifice building
        public readonly TargetIndex MeatIndex = TargetIndex.B;
        public readonly TargetIndex iConverting = TargetIndex.C; // Prisoner

        public int TicksLeft { get; set; } = (int)TicksMax;
        private static float TicksMax = StaticModVariables.WorkAmmount;
        public Pawn Sacrificer
        {
            get
            {
                return pawn;
            }
        }

        public Pawn ConvertingPawn
        {
            get
            {
                switch ((Thing)job.GetTarget(iConverting))
                {
                    case Pawn pawnPrisoner:
                        return pawnPrisoner;
                    default:
                        return null;
                }
            }
        }

        public Building SacrificeSpot
        {
            get
            {
                return (Building)job.GetTarget(iSacrificeBuilding);
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(ConvertingPawn, this.job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(SacrificeSpot, this.job) &&
                   job.GetTarget(MeatIndex).Thing == null || pawn.Reserve(job.GetTarget(MeatIndex).Thing, this.job);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(iSacrificeBuilding);
            this.FailOnAggroMentalState(iConverting);
            var convertingPawn = ConvertingPawn;

            foreach (Toil toil2 in JobDriver_DoBill.CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C, false, true, false))
            {
                yield return toil2;
            }
            Toil toil = ToilMaker.MakeToil("SetTargets");
            toil.AddFinishAction(() =>
            {
                job.SetTarget(iConverting, convertingPawn);
                job.SetTarget(MeatIndex, SacrificeSpot.Map.thingGrid.ThingsListAt(SacrificeSpot.InteractionCell).FirstOrDefault(x => x.def == Defs.Resource));
                this.FailOnDestroyedNullOrForbidden(MeatIndex);
            });
            yield return toil;
            yield return Toils_Reserve.Reserve(MeatIndex, 1, -1, null);
            Toil goToTakee = Toils_Goto.GotoThing(iConverting, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDespawnedNullOrForbidden(TargetIndex.C)
                .FailOn(() => !this.pawn.CanReach(this.SacrificeSpot, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                .FailOnSomeonePhysicallyInteracting(iConverting);

            yield return goToTakee;
            yield return Toils_Haul.StartCarryThing(iConverting, false, false, false, true);
            yield return Toils_Goto.GotoThing(iSacrificeBuilding, SacrificeSpot.InteractionCell);
            yield return Toils_Reserve.Release(iConverting);


            var doSacrificePrisoner = new Toil
            {
                socialMode = RandomSocialMode.Off,
            };
            doSacrificePrisoner.initAction = () =>
            {
                Sacrificer.carryTracker.TryDropCarriedThing(SacrificeSpot.InteractionCell, ThingPlaceMode.Direct, out var thing, null);
                ConvertingPawn.jobs.StartJob(JobMaker.MakeJob(Defs.PrisonerWait));
            };
            doSacrificePrisoner.AddFailCondition(() => ConvertingPawn.Dead);
            doSacrificePrisoner.defaultCompleteMode = ToilCompleteMode.Never;
            doSacrificePrisoner.AddPreTickAction(() =>
            {
                --TicksLeft;
                if (TicksLeft <= 0)
                {
                    ReadyForNextToil();
                }
            });
            yield return doSacrificePrisoner.WithProgressBar(iConverting, () => (TicksMax - (float)TicksLeft) / TicksMax);

            var afterSacrificePrisoner = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = () =>
                {
                    ReimplantXenogerm(Sacrificer, ConvertingPawn);
                    (ConvertingPawn.health.AddHediff(HediffDef.Named("CatatonicBreakdown")) as HediffWithComps).TryGetComp<HediffComp_Disappears>().ticksToDisappear = 100_000;
                    ConvertingPawn.jobs.StopAll();
                    if (job.GetTarget(TargetIndex.B).Thing.stackCount <= StaticModVariables.ResourceAmmount)
                        job.GetTarget(TargetIndex.B).Thing.Destroy(DestroyMode.Vanish);
                    else job.GetTarget(TargetIndex.B).Thing.stackCount -= StaticModVariables.ResourceAmmount;
                }
            };
            yield return afterSacrificePrisoner;


        }
        public static void ReimplantXenogerm(Pawn caster, Pawn recipient)
        {
            if (!ModLister.CheckBiotech("xenogerm reimplantation"))
            {
                return;
            }

            QuestUtility.SendQuestTargetSignals(caster.questTags, "XenogermReimplanted", caster.Named("SUBJECT"));
            recipient.genes.SetXenotype(caster.genes.Xenotype);
            recipient.genes.xenotypeName = caster.genes.xenotypeName;
            recipient.genes.xenotypeName = caster.genes.xenotypeName;
            recipient.genes.iconDef = caster.genes.iconDef;
            recipient.genes.ClearXenogenes();
            recipient.genes.Endogenes.Clear();
            foreach (Gene xenogene in caster.genes.GenesListForReading)
            {
                recipient.genes.AddGene(xenogene.def, xenogene: true);
            }

            if (!caster.genes.Xenotype.soundDefOnImplant.NullOrUndefined())
            {
                caster.genes.Xenotype.soundDefOnImplant.PlayOneShot(SoundInfo.InMap(recipient));
            }
            recipient.health.AddHediff(HediffDefOf.XenogerminationComa);
            if (StaticModVariables.isSicknessEnabled)
            {
                GeneUtility.ExtractXenogerm(caster);
            }
            GeneUtility.UpdateXenogermReplication(recipient);
        }
    }
}
