using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;


namespace XenoRitual.Helpers
{
    internal static class Defs
    {
        internal static ThingDef HumanMeat => _HumanMeat ?? DefDatabase<ThingDef>.GetNamed("Meat_Human");
        private static ThingDef _HumanMeat;

        internal static JobDef ConvertXenotype => _convertXenotype ?? DefDatabase<JobDef>.GetNamed("XenoRiteXenotypeConvertion");
        private static JobDef _convertXenotype;

        internal static JobDef PrisonerWait => _prisonerWait ?? DefDatabase<JobDef>.GetNamed("XenoRiteXenotypeConvertionPrisonerWait");
        private static JobDef _prisonerWait;
    }
}
