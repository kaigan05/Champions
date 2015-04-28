using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImmortalSerials.Controller;
using ImmortalSerials.Model;
using ImmortalSerials.Objects;
using LeagueSharp;
using LeagueSharp.Common;
using SpellData = ImmortalSerials.Model.SpellData;

namespace ImmortalSerials
{
    internal class Katarina : Champion
    {
        public Katarina()
        {
            KatarinaMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            new AutoLevel(new List<SpellSlot>
            { SpellSlot.Q, SpellSlot.E, SpellSlot.W, SpellSlot.Q, SpellSlot.Q, SpellSlot.R,
            SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E});
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Console.WriteLine(args.SData.Name);
            }
        }
        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            //foreach (var buff in Player.Buffs)
            //{
            //    Console.WriteLine(buff.Name);
            //}
            //ItemCrystalFlask
            //Console.WriteLine("Player.IsChannelingImportantSpell(): {0}", Player.IsChannelingImportantSpell());
            //Console.WriteLine("Buff KatarinaRSound: {0}", Player.HasBuff("katarinarsound",true));
            if (Player.HasBuff("katarinarsound",true)||Player.IsChannelingImportantSpell())
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
            }
            else
            {
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
            }
            KillSteal();
            //if ((int)Player.HealthPercentage() <= 20)
            //{
            //    if (Zhy.IsReady())
            //    {
            //        Zhy.Cast();
            //    }
            //}
            if (MainMenu.Item("AutoF").GetValue<KeyBind>().Active && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                Farm();
            }
            if (MainMenu.Item("WardJump").GetValue<KeyBind>().Active)
            {
                var warPos = Player.ServerPosition.Extend(Game.CursorPos, Ward.CastRange);
                Player.IssueOrder(GameObjectOrder.MoveTo, warPos);
                Ward.Jump(warPos);
            }

            if (!LastHiting && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                SafeUnderTurret.Safe();
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harras();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm(true);
                    break;
            }
            //if (SpellData.E.IsReady())
            //{
            
                //if (Evader.IsSafe(Player.Position.To2D())) return;
                //var minion = ObjectManager.Get<Obj_AI_Base>().Where(m => m.Distance(Player) < SpellData.E.Range && Evader.IsSafe(m.Position.To2D()) && !m.IsValid<Obj_AI_Turret>()).Aggregate((v1, v2) => v1.Distance(Game.CursorPos.To2D()) > v2.Distance(Game.CursorPos.To2D()) ? v2 : v1);
                //SpellData.E.SmartCast(minion);
                //Game.PrintChat("Ne skill");
            //}
        }
        
        private void KillSteal()
        {
            int targetNum = 0;
            foreach (var target in ChampionData.CanTarget(1375,TargetTeam.Enemy).OrderBy(c => c.Health))
            {
                targetNum++;
                float dist;
                if (!((dist = Player.Distance(target)) < 1375))
                {
                    continue;
                }
                float targetHealth = target.Health;
                float qDame, q1Dame;
                if (SpellData.Q.IsReadyKs)
                {
                    qDame = SpellData.Q.GetDamage(target);
                    q1Dame = SpellData.Q.GetDamage(target, 1);
                }
                else
                {
                    if (target.HasBuff("katarinaqmark", true))
                    {
                        targetHealth -= SpellData.Q.GetDamage(target, 1);
                    }
                    qDame = 0;
                    q1Dame = 0;
                }
                var wDame = SpellData.W.IsReadyKs ? SpellData.W.GetDamage(target) : 0;
                var eDame = SpellData.E.IsReadyKs ? SpellData.E.GetDamage(target) : 0;
                var rDame = SpellData.R.IsReadyKs ? SpellData.R.GetDamage(target, 1) : 0;
                var qewDame = qDame + eDame + q1Dame + wDame;
                if (qewDame + rDame < targetHealth)
                {
                    LastHiting = false;
                    return;
                }
                LastHiting = true;
                if (wDame > 0)
                {
                    if (wDame > targetHealth)
                    {
                        if (SpellData.W.Range > dist)
                        {
                            SpellData.W.SmartCast(target);
                        }
                        else
                        {
                            KsJump(target, SpellData.W, dist);
                        }
                        return;
                    }
                    if (eDame > 0 && SpellData.E.Range > dist)
                    {
                        if (eDame + wDame > targetHealth)
                        {
                            SpellData.E.SmartCast(target);
                            return;
                        }
                        if (qewDame > targetHealth)
                        {
                            if (SpellData.Q.Range > dist)
                            {
                                SpellData.Q.SmartCast(target);
                            }
                            else
                            {
                                SpellData.E.SmartCast(target);
                            }
                            return;
                        }
                    }
                    if (qDame + wDame + q1Dame > targetHealth)
                    {
                        if (SpellData.Q.Range > dist)
                        {
                            SpellData.Q.SmartCast(target);
                        }
                        else
                        {
                            KsJump(target, SpellData.W, dist);
                        }
                        return;
                    }
                }
                if (eDame > 0 && SpellData.E.Range > dist)
                {
                    if (eDame > targetHealth)
                    {
                        SpellData.E.SmartCast(target);
                        return;
                    }
                    if (qDame + eDame + q1Dame > targetHealth)
                    {
                        if (SpellData.Q.Range > dist)
                        {
                            SpellData.Q.SmartCast(target);
                        }
                        else
                        {
                            SpellData.E.SmartCast(target);
                        }
                        return;
                    }
                }

                if (qDame > targetHealth)
                {
                    if (SpellData.Q.Range > dist)
                    {
                        SpellData.Q.SmartCast(target);
                    }
                    else
                    {
                        KsJump(target, SpellData.Q, dist);
                    }
                    return;
                }

                if (qewDame <= 0)
                {
                    if (rDame > target.Health)
                    {
                        if (SpellData.R.Range / 2 > dist)
                        {
                            SpellData.R.SmartCast(target);
                        }
                    }
                }

                if (qewDame + rDame <= 0 && !Player.IsChannelingImportantSpell() && SpellData.Ignite.Range > dist)
                {
                    CastIgnite(target);
                }
            }
            if (targetNum > 0)
            {
                LastHiting = false;
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetSelectedTarget() ??
                         TargetSelector.GetTarget(SpellData.Q.Range, TargetSelector.DamageType.Magical);
            if (target == null || Player.IsChannelingImportantSpell()|| Player.HasBuff("katarinarsound", true))
            {
                return;
            }
            var distance = Player.Distance(target.ServerPosition);
            if (SpellData.Q.IsReady() && distance <= SpellData.Q.Range + target.BoundingRadius)
            {
                SpellData.Q.SmartCast(target);
            }
            if (SpellData.E.IsReady() && distance < SpellData.E.Range + target.BoundingRadius)
            {
                SpellData.E.SmartCast(target);
            }
            if (SpellData.W.IsReady() && distance < SpellData.W.Range)
            {
                SpellData.W.SmartCast(target);
            }
            if (!SpellData.Q.IsReady() && !SpellData.W.IsReady() && !SpellData.E.IsReady() && SpellData.R.IsReady() && distance < SpellData.R.Range)
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
                SpellData.R.SmartCast(target);
            }
            if (!SpellData.Q.IsReady() && (!SpellData.W.IsReady() || distance > SpellData.W.Range) && !SpellData.E.IsReady() &&
                !Player.IsChannelingImportantSpell() && SpellData.Ignite.Range > distance)
                CastIgnite(target);
        }

        private void Harras()
        {
            var target = TargetSelector.GetSelectedTarget() ??
                         TargetSelector.GetTarget(SpellData.Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            var dis = Player.Distance(target.ServerPosition);
            if (SpellData.Q.IsReadyHarass && dis <= SpellData.Q.Range + target.BoundingRadius)
            {
                SpellData.Q.CastOnUnit(target, UsePacket);
            }
            if (SpellData.E.IsReadyHarass && dis < SpellData.E.Range + target.BoundingRadius && !target.UnderTurret(true) &&
                (SpellData.Q.IsReadyHarass || SpellData.W.IsReadyHarass))
            {
                SpellData.E.CastOnUnit(target, UsePacket);
            }
            if (SpellData.W.IsReadyHarass && dis < SpellData.W.Range)
            {
                SpellData.W.Cast();
            }
        }

        public void Farm(bool dondep = false)
        {
            var targets = MinionManager.GetMinions(Player.ServerPosition, SpellData.Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (targets.Count <= 0)
                return;
            var target = targets.Where(o => Player.Distance(o) <= SpellData.Q.Range).OrderBy(o => o.Health).FirstOrDefault();
            if (target == null)
                return;
            var dist = Player.Distance(target);
            if (dondep)
            {
                if (SpellData.Q.IsReadyClear)
                {
                    SpellData.Q.SmartCast(target);
                }
                if (SpellData.W.IsReadyClear && SpellData.E.IsReadyClear)
                {
                    SpellData.E.SmartCast(target);
                }
                if (SpellData.W.IsReadyClear && dist < SpellData.W.Range)
                {
                    SpellData.W.SmartCast(target);
                }
            }
            else
            {
                float targetHealth = target.Health;
                float qDame, q1Dame;
                if (SpellData.Q.IsReadyFarm)
                {
                    qDame = GetTrueDame(target, SpellData.Q.GetDamage(target));
                    q1Dame = GetTrueDame(target, SpellData.Q.GetDamage(target, 1));
                }
                else
                {
                    if (target.HasBuff("katarinaqmark", true))
                    {
                        targetHealth -= GetTrueDame(target, SpellData.Q.GetDamage(target, 1));
                    }
                    qDame = 0;
                    q1Dame = 0;
                }
                var wDame = SpellData.W.IsReadyFarm ? GetTrueDame(target, SpellData.W.GetDamage(target)) : 0;
                float eDame = 0;
                if (!target.UnderTurret())
                {
                    eDame = SpellData.E.IsReadyFarm ? GetTrueDame(target, SpellData.E.GetDamage(target)) : 0;
                }
                var qewDame = qDame + eDame + q1Dame + wDame;
                if (qewDame < targetHealth)
                {
                    return;
                }
                if (wDame > 0 && SpellData.W.Range > dist)
                {
                    if (wDame > targetHealth)
                    {
                        SpellData.W.SmartCast(target);
                        return;
                    }
                    if (eDame > 0)
                    {
                        if (eDame + wDame > targetHealth)
                        {
                            SpellData.E.SmartCast(target);
                            return;
                        }
                        if (qewDame > targetHealth)
                        {
                            SpellData.Q.SmartCast(target);
                            return;
                        }
                    }
                    if (qDame + wDame + q1Dame > targetHealth)
                    {
                        SpellData.Q.SmartCast(target);
                        return;
                    }
                }
                if (qDame > targetHealth)
                {
                    SpellData.Q.SmartCast(target);
                    return;
                }
                if (eDame > 0)
                {
                    if (eDame > targetHealth)
                    {
                        SpellData.E.SmartCast(target);
                        return;
                    }
                    if (qDame + eDame + q1Dame > targetHealth)
                    {
                        SpellData.Q.SmartCast(target);
                        return;
                    }
                }
            }
        }

        private float GetTrueDame(Obj_AI_Base minion, float damage)
        {
            if ((int) minion.SpellBlock == 0)
            {
                damage *= 1.030f;
                if (Items.HasItem((int) ItemId.Sorcerers_Shoes))
                {
                    damage *= 1.008f;
                }
            }
            return damage;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            //var target = MinionManager.GetMinions(500, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health).OrderBy(minion => minion.Distance(Player)).FirstOrDefault();
            var pos = Drawing.WorldToScreen(Player.Position);
          //if (target != null)
          //{
          //    Drawing.DrawText(pos.X - 30, pos.Y + 35, Color.GreenYellow, String.Format("{0} == {1}", GetTrueDame(target, SpellData.Q.GetDamage(target)).ToString(), target.Health - GetTrueDame(target, SpellData.Q.GetDamage(target))));
          //    Drawing.DrawText(pos.X - 30, pos.Y + 45, Color.GreenYellow, String.Format("{0} == {1}", GetTrueDame(target, SpellData.W.GetDamage(target)).ToString(), target.Health - GetTrueDame(target, SpellData.W.GetDamage(target))));
            
            
          //}
            //Drawing.DrawText(pos.X - 30, pos.Y + 35, Color.GreenYellow, SpellData.Q.GetDamage(target, 0).ToString());
            Render.Circle.DrawCircle(Player.Position.Extend(Game.CursorPos, 599), 10, Color.Gold);
            if (MainMenu.Item("AutoF").GetValue<KeyBind>().Active)
            {
                Drawing.DrawText(pos.X - 30, pos.Y + 25, Color.GreenYellow, "LastHit: On");
            }
            else
            {
                Drawing.DrawText(pos.X - 30, pos.Y + 25, Color.Black, "LastHit: Off");
            }
            foreach (var spell in SpellData.PlayerSpells)
            {
                var item = MainMenu.Item("Draw" + spell.Slot).GetValue<Circle>();
                if (item.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, item.Color);
                }
            }
        }

        public void KatarinaMenu()
        {
            MainMenu.AddSubMenu(new Menu("Kill Steal", "KillSteal"));
            var useQKs = MainMenu.SubMenu("KillSteal").AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            var useWKs = MainMenu.SubMenu("KillSteal").AddItem(new MenuItem("UseWKS", "Use W").SetValue(true));
            var useEKs = MainMenu.SubMenu("KillSteal").AddItem(new MenuItem("UseEKS", "Use E").SetValue(true));
            //var useRKs = MainMenu.SubMenu("KillSteal").AddItem(new MenuItem("UseRKS", "Use R").SetValue(true));
            MainMenu.AddSubMenu(new Menu("Harass", "Harass"));
            var useQh = MainMenu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            var useWh = MainMenu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(true));
            var useEh = MainMenu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            MainMenu.AddSubMenu(new Menu("Farm", "Farm"));
            var useQf = MainMenu.SubMenu("Farm").AddItem(new MenuItem("UseQF", "Use Q").SetValue(true));
            var useWf = MainMenu.SubMenu("Farm").AddItem(new MenuItem("UseWF", "Use W").SetValue(true));
            var useEf = MainMenu.SubMenu("Farm").AddItem(new MenuItem("UseEF", "Use E").SetValue(false));
            MainMenu.SubMenu("Farm")
                .AddItem(new MenuItem("AutoF", "Auto").SetValue(new KeyBind(90, KeyBindType.Toggle)));
            MainMenu.AddSubMenu(new Menu("LaneClear", "Clear"));
            var useQc = MainMenu.SubMenu("Clear").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            var useWc = MainMenu.SubMenu("Clear").AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            var useEc = MainMenu.SubMenu("Clear").AddItem(new MenuItem("UseEC", "Use E").SetValue(false));
            MainMenu.AddSubMenu(new Menu("Misc", "Misc"));
            MainMenu.SubMenu("Misc").AddItem(new MenuItem("Packet", "Use Packet").SetValue(false));
            MainMenu.SubMenu("Misc")
                .AddItem(new MenuItem("WardJump", "Ward Jump").SetValue(new KeyBind(71, KeyBindType.Press)));
            MainMenu.AddSubMenu(new Menu("Drawing", "Drawing"));
            MainMenu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawQ", "Q range").SetValue(new Circle(true, Color.Yellow)));
            MainMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "W range").SetValue(new Circle(false, Color.Teal)));
            MainMenu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawE", "E range").SetValue(new Circle(false, Color.Crimson)));
            MainMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "R range").SetValue(new Circle(true, Color.Red)));
            MainMenu.AddToMainMenu();
            SpellData.Q.IsReadyKs = MainMenu.Item("UseQKS").GetValue<bool>();
            SpellData.E.IsReadyKs = MainMenu.Item("UseEKS").GetValue<bool>();
            SpellData.W.IsReadyKs = MainMenu.Item("UseWKS").GetValue<bool>();
            //SpellData.R.IsReadyKs = MainMenu.Item("UseRKS").GetValue<bool>();
            SpellData.Q.IsReadyHarass = MainMenu.Item("UseQH").GetValue<bool>();
            SpellData.W.IsReadyHarass = MainMenu.Item("UseWH").GetValue<bool>();
            SpellData.E.IsReadyHarass = MainMenu.Item("UseEH").GetValue<bool>();
            SpellData.Q.IsReadyFarm = MainMenu.Item("UseQF").GetValue<bool>();
            SpellData.W.IsReadyFarm = MainMenu.Item("UseWF").GetValue<bool>();
            SpellData.E.IsReadyFarm = MainMenu.Item("UseEF").GetValue<bool>();
            SpellData.Q.IsReadyClear = MainMenu.Item("UseQC").GetValue<bool>();
            SpellData.W.IsReadyClear = MainMenu.Item("UseWC").GetValue<bool>();
            SpellData.E.IsReadyClear = MainMenu.Item("UseEC").GetValue<bool>();
            useQKs.ValueChanged += (sender, args) => SpellData.Q.IsReadyKs = args.GetNewValue<bool>();
            useEKs.ValueChanged += (sender, args) => SpellData.E.IsReadyKs = args.GetNewValue<bool>();
            useWKs.ValueChanged += (sender, args) => SpellData.W.IsReadyKs = args.GetNewValue<bool>();
            //useRKs.ValueChanged += (sender, args) => SpellData.R.IsReadyKs = args.GetNewValue<bool>();
            useQh.ValueChanged += (sender, args) => SpellData.Q.IsReadyHarass = args.GetNewValue<bool>();
            useEh.ValueChanged += (sender, args) => SpellData.E.IsReadyHarass = args.GetNewValue<bool>();
            useWh.ValueChanged += (sender, args) => SpellData.W.IsReadyHarass = args.GetNewValue<bool>();
            useQf.ValueChanged += (sender, args) => SpellData.Q.IsReadyFarm = args.GetNewValue<bool>();
            useEf.ValueChanged += (sender, args) => SpellData.E.IsReadyFarm = args.GetNewValue<bool>();
            useWf.ValueChanged += (sender, args) => SpellData.W.IsReadyFarm = args.GetNewValue<bool>();
            useQc.ValueChanged += (sender, args) => SpellData.Q.IsReadyClear = args.GetNewValue<bool>();
            useEc.ValueChanged += (sender, args) => SpellData.E.IsReadyClear = args.GetNewValue<bool>();
            useWc.ValueChanged += (sender, args) => SpellData.W.IsReadyClear = args.GetNewValue<bool>();
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (SpellData.Q.IsReady())
                damage += SpellData.Q.GetDamage(enemy) + SpellData.Q.GetDamage(enemy, 1);

            if (SpellData.W.IsReady())
                damage += SpellData.W.GetDamage(enemy);

            if (SpellData.E.IsReady())
                damage += SpellData.E.GetDamage(enemy);

            if (SpellData.Ignite.IsReady())
                damage += SpellData.Ignite.GetDamage(enemy);

            if (SpellData.R.IsReady())
                damage += SpellData.R.GetDamage(enemy, 1) * 8;
            return (float) damage;
        }

        public bool UsePacket
        {
            get { return MainMenu.Item("Packet").GetValue<bool>(); }
        }
        private void KsJump(Obj_AI_Hero target, MySpell skillKs, float dist)
        {
            if (SpellData.E.IsReadyKs)
            {
                if (SpellData.E.Range < dist && SpellData.E.Range + Ward.CastRange > dist)
                {
                    Ward.Jump(Player.ServerPosition.Extend(target.ServerPosition, Ward.CastRange));
                }
                else if (SpellData.E.Range + skillKs.Range > dist)
                {
                    SpellData.E.SmartCast(target);
                }
            }
        }
    }
}
