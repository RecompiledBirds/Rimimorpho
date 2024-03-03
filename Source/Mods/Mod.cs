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
            listing_Standard.CheckboxLabeled("Use vanilla rimworld tabs: ", ref RimimorphoSettings.useRimworldTabs);
            listing_Standard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Rimimorpho Settings";
        }
    }
}
