using System;
using System.Collections.Generic;
using System.Text;

namespace Rayiah.Ekumena
{
    public static class EkumenaResearch
    {
        public delegate void ResearchDelegate(ref EkumenaPlayer player, ref Research res);

        public static Dictionary<string, ResearchDelegate> Events = new Dictionary<string, ResearchDelegate>();

        public static IReadOnlyDictionary<string, Research> Research => research;
        static Dictionary<string, Research> research = new Dictionary<string, Research>();

        public static void CallOnResearch(ref EkumenaPlayer player, ref Research res)
        {
            
        }

        public static void CallEveryTurn(EkumenaPlayer player, Research res)
        {
            
        }
    }

    public abstract class Research
    {
        public int Possibility;
        public int CostInTours;
        public int Stage; // od 1 do któregoś tam.
        public string Name;
        public ResearchType Type = ResearchType.Option;

        public DynamicResearchStruct OnResearchStr;
        public DynamicResearchStruct EveryTurnStr;
    }

    public struct DynamicResearchStruct
    {
        public string DelegateStr;
        public object Var;
    }
}