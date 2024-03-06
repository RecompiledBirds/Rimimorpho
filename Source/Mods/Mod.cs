using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    public class RimimorphoMod : Mod
    {
        public RimimorphoMod(ModContentPack content) : base(content)
        {
           
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Rimimorpho_UseVanillaTabs".Translate(), ref RimimorphoSettings.useRimworldTabs);
            listing_Standard.CheckboxLabeled("Rimimorpho_PawnsGetInfected".Translate(), ref RimimorphoSettings.pawnsCanGetInfected);
            if (RimimorphoSettings.pawnsCanGetInfected)
            {
                listing_Standard.CheckboxLabeled("Rimimorpho_PawnsGetInfectedByContact".Translate(), ref RimimorphoSettings.pawnsCanGetInfectedViaContact);
                listing_Standard.CheckboxLabeled("Rimimorpho_PawnsGetInfectedByBite".Translate(), ref RimimorphoSettings.pawnsCanGetInfectedViaBite);
            }
            listing_Standard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Rimimorpho_SettingsName".Translate();
        }
    }
}
