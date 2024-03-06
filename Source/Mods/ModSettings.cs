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
        public static bool pawnsCanGetInfected = true;
        public static bool pawnsCanGetInfectedViaContact = true;
        public static bool pawnsCanGetInfectedViaBite = true;
        public static bool somePawnsAreAmphimorpho = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref useRimworldTabs, nameof(useRimworldTabs));
            Scribe_Values.Look(ref pawnsCanGetInfected, nameof(pawnsCanGetInfected));
            Scribe_Values.Look(ref pawnsCanGetInfectedViaContact, nameof(pawnsCanGetInfectedViaContact));
            Scribe_Values.Look(ref pawnsCanGetInfectedViaBite, nameof(pawnsCanGetInfectedViaBite));
            base.ExposeData();
        }
    }
}
