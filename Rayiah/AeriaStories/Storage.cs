using System;
using System.Collections.Generic;
using System.Text;

namespace Rayiah.AeriaStories
{
    public static class Storage
    {
        public static List<GamePanel> Panels = new List<GamePanel>();
 
        public static void Cycle()
        {
            foreach (GamePanel panel in Panels)
            {
                if (panel.Character.CheckTimer()) Panels.Remove(panel);
            }
        }
    }
}