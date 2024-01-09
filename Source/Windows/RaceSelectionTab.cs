using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    public class RaceSelectionTab : TFWindowTab
    {
        public Rect viewRect;
        private Vector2 scrollPos;
        public override string Name => "Race";
        const float boxHeight=60;
        public override void Draw(Rect inRect, Pawn pawn, AmphiShifter shifter)
        {
            viewRect=new Rect(inRect.position, new Vector2(inRect.width-30, inRect.height*shifter.knownSpecies.Count));
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
           
            float xPos = inRect.position.x + 60;
            float textureX = inRect.position.x + 10;
            List<ThingDef> species = shifter.knownSpecies.Keys.ToList();
            int length = 1;
            for (int i = 0; i < shifter.knownSpecies.Count; i++)
            {
                List<StoredRace> storedRaces =shifter.knownSpecies[species[i]];
                if (storedRaces.NullOrEmpty()) continue;
                Texture2D tex = species[i].uiIcon;
                Widgets.DrawTextureFitted(new Rect(new Vector2(textureX,boxHeight*length+20),new Vector2(boxHeight,boxHeight)),tex,1);
                for(int a =0; a < storedRaces.Count; a++)
                {
                    StoredRace race = storedRaces[a];
                    if (a == 0)
                    {
                        Widgets.Label(new Rect(new Vector2(xPos, boxHeight * length + 20), new Vector2(inRect.width - 90, boxHeight)), (race.XenotypeDef != null ? race.XenotypeDef.defName : race.ThingDef.defName));
                        continue;
                    }
                    if (a > 0)
                    {
                        Texture2D icon = tex;
                        if (race.XenotypeDef != null&&race.XenotypeDef.Icon!=null) icon = race.XenotypeDef.Icon;
                        Widgets.DrawTextureFitted(new Rect(new Vector2(textureX, boxHeight * length + 20), new Vector2(boxHeight, boxHeight)), icon, 1);
                       
                    }
                    Widgets.Label(new Rect(new Vector2(xPos, boxHeight * length + 20 + boxHeight), new Vector2(inRect.width - 90 - boxHeight, boxHeight)), (race.XenotypeDef != null ? race.XenotypeDef.defName : race.ThingDef.defName));
                    length++;
                }
                length++;


            }
            Widgets.EndScrollView();
        }
    }
}
