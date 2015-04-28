using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace ImmortalSerials
{
    class Program
    {
        
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Champion champion;
            switch (ObjectManager.Player.ChampionName)
            {
                case "Katarina":
                    champion = new Katarina();
                    break;
            }
            Game.PrintChat(ObjectManager.Player.ChampionName);
        }
    }
}
