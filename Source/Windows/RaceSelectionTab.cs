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

        public override int TabIndex => 0;

        const float boxHeight=60;
        int size = 0;
        public override void Draw(Rect inRect, Pawn pawn, AmphiShifter shifter)
        {
            size = shifter.knownSpecies.Count;
            viewRect=new Rect(inRect.position, new Vector2(inRect.width-30,(size*boxHeight)));
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
           
            float xPos = inRect.position.x + 60;
            float textureX = inRect.position.x + 10;
            
            List<ThingDef> species = shifter.knownSpecies.Keys.ToList();
            int length = 1;
           
            for (int i = 0; i < shifter.knownSpecies.Count; i++)
            {
                RaceList<StoredRace> storedRaces =shifter.knownSpecies[species[i]];
                if (storedRaces.Empty) continue;
                Texture2D tex = species[i].uiIcon;
                Widgets.DrawTextureFitted(new Rect(new Vector2(textureX,boxHeight*length+20),new Vector2(boxHeight,boxHeight)),tex,1);
                GameFont current = Text.Font;
                TextAnchor curAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Medium;
                for (int a =0; a < storedRaces.Length; a++)
                {
                    float lengthEval = boxHeight * length + 20;
                    float widthEval = inRect.width - 90;
                    StoredRace race = storedRaces[a];
                    if (a == 0)
                    {
                        Widgets.Label(new Rect(new Vector2(xPos+40,lengthEval), new Vector2(widthEval, boxHeight)), (race.XenotypeDef != null ? race.XenotypeDef.defName : race.ThingDef.defName));
                        continue;
                    }
                    if (a > 0)
                    {
                        Texture2D icon = tex;
                        if (race.XenotypeDef != null&&race.XenotypeDef.Icon!=null) icon = race.XenotypeDef.Icon;
                        Rect iconRect = new Rect(new Vector2(textureX, lengthEval), new Vector2(boxHeight, boxHeight));
                        Widgets.DrawTextureFitted(iconRect, icon, 1);
                        Widgets.DrawMenuSection(iconRect);

                    }
                    Widgets.Label(new Rect(new Vector2(xPos+40, lengthEval), new Vector2(widthEval - boxHeight, boxHeight)), (race.XenotypeDef != null ? race.XenotypeDef.defName : race.ThingDef.defName));
                    
                    length++;
                }
                Text.Anchor = curAnchor; Text.Font=current;
                length++;


            }
            Widgets.EndScrollView();
        }
    }
}
