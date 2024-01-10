using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public class RimimorphoSettings : ModSettings
    {
        public static bool useRimworldTabs = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref useRimworldTabs, nameof(useRimworldTabs));
            base.ExposeData();
        }
    }
}
