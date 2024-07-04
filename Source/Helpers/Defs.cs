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
        internal static ThingDef Resource => _Resource ?? DefDatabase<ThingDef>.GetNamed("XenoRiteRes");
        private static ThingDef _Resource;

        internal static RecipeDef ChangeXenotypeRiteRecipe => _ChangeXenotypeRiteRecipe ?? DefDatabase<RecipeDef>.GetNamed("ChangeXenotypeRiteRecipe");
        private static RecipeDef _ChangeXenotypeRiteRecipe;

        internal static JobDef ConvertXenotype => _convertXenotype ?? DefDatabase<JobDef>.GetNamed("XenoRiteXenotypeConvertion");
        private static JobDef _convertXenotype;

        internal static JobDef PrisonerWait => _prisonerWait ?? DefDatabase<JobDef>.GetNamed("XenoRiteXenotypeConvertionPrisonerWait");
        private static JobDef _prisonerWait;
    }
}
