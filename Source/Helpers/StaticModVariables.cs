using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenoRitual.Helpers
{
    internal static class StaticModVariables
    {
        public static int ammount
        {
            get
            {
                var fromRecipe = (int?)Defs.ChangeXenotypeRiteRecipe.ingredients.FirstOrDefault()?.GetBaseCount();
                if (fromRecipe == null || fromRecipe <= 0)
                {
                    return 75;
                }
                return (int)fromRecipe;
            }
        }
    }
}
