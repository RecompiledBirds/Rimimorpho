using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    internal class TransformationSelectionWindow : Window
    {
        private const float margin = 5f;

        private static readonly Vector2 initSize = new Vector2(300, 600);

        private static readonly Rect window = new Rect(new Vector2(), initSize);
        private static readonly Rect windowMargin = window.ContractedBy(5f);
        private static readonly Rect topBarRect = new Rect(windowMargin.position, new Vector2(windowMargin.width, 26f));
        private static readonly Rect titleRect = new Rect(topBarRect.x, topBarRect.y, topBarRect.width - topBarRect.height - 4f, topBarRect.height);
        private static readonly Rect closBttnRect = new Rect(topBarRect.xMax - topBarRect.height, topBarRect.y, topBarRect.height, topBarRect.height + 1f);

        public new bool draggable = true;
        public new bool onlyOneOfTypeAllowed = true;

        private readonly Pawn pawn;
        private readonly Color originalColor;
        private readonly GameFont originalFont;

        public TransformationSelectionWindow(Pawn pawn) 
        {
            this.pawn = pawn;
            
            originalFont = Text.Font;
            originalColor = GUI.color;
        }

        public override Vector2 InitialSize => initSize;

        protected override float Margin => 0f;

        private void ResetFontAndColor()
        {
            Text.Font = originalFont;
            GUI.color = originalColor;
        }

        //TODO: Translations strings
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, $"Select Transformation:");
            ResetFontAndColor();
            if (Widgets.CloseButtonFor(closBttnRect)) Close();
        }
    }
}
