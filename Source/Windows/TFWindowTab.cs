using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    public abstract class TFWindowTab
    {
        public abstract void Draw(Rect inRect, Pawn pawn, AmphiShifter shifter);
        public abstract string Name { get; }
    }
}