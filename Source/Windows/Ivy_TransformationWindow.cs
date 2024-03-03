using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho.Windows
{
    public class Ivy_TransformationWindow : Window
    {

        private static List<TFWindowTab> tabs = new List<TFWindowTab>();
        public Ivy_TransformationWindow(Pawn pawn) { 
            this.pawn= pawn;
            this.shifter=pawn.TryGetComp<AmphiShifter>();
            if (tabs.Count == 0)
            {
                foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach(Type type in asm.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(TFWindowTab)))
                        {
                            TFWindowTab tab = (TFWindowTab)Activator.CreateInstance(type);
                            tabs.Add(tab);
                            
                        }
                    }
                }
                tabs.SortBy(x=>x.TabIndex);
                foreach(TFWindowTab tab in tabs)
                {
                    tabRecords.Add(new TFTabRecord(tab.Name, delegate ()
                    {
                        currentTab = tab;
                    },()=> currentTab == tab, tab));
                }
            }
            
        }
        private List<TFTabRecord> tabRecords = new List<TFTabRecord>();
        private TFWindowTab currentTab;
        private Rect tabRect = new Rect(new Vector2(20,11), new Vector2(720,60));
        private Rect areaRect = new Rect(new Vector2(20,70), new Vector2(800,430));
        private Rect closeButtonRect = new Rect(new Vector2(720,0), new Vector2(80, 80));
        private Pawn pawn;
        private AmphiShifter shifter;
        public override Vector2 InitialSize => new Vector2(860, 540);


        public override void DoWindowContents(Rect inRect)
        {
            // Widgets.DrawBox(tabRect);
            if (RimimorphoSettings.useRimworldTabs)
            {
                TabDrawer.DrawTabs(areaRect, tabRecords);
                Widgets.DrawMenuSection(areaRect);
            }
            else
            {
                for (int i = 0; i < tabs.Count; i++)
                {

                    TFWindowTab tab = tabs[i];
                    Vector2 rectSize = new Vector2(200, 60);
                    Vector2 rectPosition = new Vector2((rectSize.x * i) + 20, 11);
                    Rect rect = new Rect(rectPosition, rectSize);
                    if (Widgets.ButtonText(rect, tab.Name)) currentTab = tab;
                }
            }

           // Widgets.DrawBox(areaRect);
            currentTab?.Draw(areaRect,pawn,shifter);
            if (Widgets.CloseButtonFor(closeButtonRect)) Close();
        }

        private class TFTabRecord : TabRecord
        {
            private TFWindowTab tab;
            public TFWindowTab Tab { get { return tab; } }
            public TFTabRecord(string label, Action clickedAction, Func<bool> selected, TFWindowTab tab) : base(label, clickedAction, selected)
            {
                this.tab=tab;
            }
        }
    }
}
