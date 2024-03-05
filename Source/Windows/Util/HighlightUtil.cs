using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    internal class HighlightUtil
    {
        internal static void DrawHighlights(int curHeight, Rect tmpBttnRect)
        {
            if (curHeight % 2 == 0)
            {
                Widgets.DrawHighlight(tmpBttnRect);
            }
            else
            {
                Widgets.DrawLightHighlight(tmpBttnRect);
            }
        }

    }
}
