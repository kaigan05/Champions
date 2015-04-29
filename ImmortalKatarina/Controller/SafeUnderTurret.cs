using System;
using System.Collections.Generic;
using System.Linq;
using ImmortalSerials.Model;
using ImmortalSerials.Objects;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ImmortalSerials.Controller
{
    static class SafeUnderTurret
    {
        private static int _lastSave;
        private static readonly MySpell JumpSpell;
        public const int MinMinion = 1;
        static SafeUnderTurret()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Katarina":
                    JumpSpell = SpellDb.E;
                    break;
            }
            
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var tur = sender as Obj_AI_Turret;
            if (tur != null && (sender.IsEnemy && args.Target.IsMe))
            {
                if (!Champion.LastHiting && Champion.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                {
                    Safe(true);
                }
            }
        }

        public static float GetTimeToPos(Vector3 pos, float moveSpeed = -1)
        {
            if (moveSpeed == -1)
            {
                moveSpeed = ChampionData.Player.MoveSpeed;
            }
            var waypoints=ChampionData.Player.GetPath(pos);
            var numPoint = waypoints.Count() - 1;
            if (numPoint <= 0)
            {
                return 0;
            }
            float time=0;
            for (int index = 0; index < numPoint; index++)
            {
                var beginPoint = waypoints[index];
                var endPoint = waypoints[index + 1];
                time += beginPoint.Distance(endPoint) / moveSpeed;
            }
            return time;
        }

        
        public static void Safe(bool leaveNow = false)
        {
            if (Environment.TickCount > _lastSave)
            {
                var tur = ChampionData.Player.GetTurret();
                if (tur == null)
                {
                    return;
                }
                var objAiBases = ObjectManager.Get<Obj_AI_Base>().Where(minion => minion.IsVisible && !minion.IsDead && !minion.IsMe && minion.InRange(tur,TurretData.CastRange + 700) && !minion.IsValid<Obj_AI_Turret>());
                var minionsInRange = objAiBases as IList<Obj_AI_Base> ?? objAiBases.ToList();
                if (leaveNow || minionsInRange.Count(minion => minion.IsAlly && !minion.Name.Contains("Ward") && minion.InRange(tur, TurretData.CastRange)) <= MinMinion)
                {
                    var posDest = ChampionData.Player.ShortestPath(tur.GetSafePosCircle(TurretData.SafeDist));
                    bool flee = false;
                    if (JumpSpell.IsReady())
                    {
                        foreach (var minion in minionsInRange.Where(minion => JumpSpell.CanCast(minion)))
                            {
                                var turretOnMinion = minion.GetTurret() ?? tur;
                                var distMove = Math.Max(0, ChampionData.Player.Distance(minion) - JumpSpell.Range) + Math.Max(0, minion.ShortestPath(turretOnMinion.GetSafePosCircle(TurretData.SafeDist)).Distance);
                                if (distMove < posDest.Distance || distMove == posDest.Distance && minion.Distance(turretOnMinion) > posDest.Position.Distance(tur.Position))
                                {
                                    posDest = new Destination(minion, distMove);
                                }
                            }
                            if (posDest.ObjAiBase!=null)
                            {
                                //Game.PrintChat("Cast skill " + spell.Slot + " vi tri: " + posDest.ObjAiBase.Name);
                                if (JumpSpell.SmartCast(posDest.ObjAiBase))
                                {
                                    flee = true;
                                }
                            }
                            else if (JumpSpell.CanTarget(TargetType.Ally) && Ward.GetWardSlot() != null)
                            {
                                posDest = tur.LongestPath(ChampionData.Player.GetSafePosCircle(Ward.CastRange));
                                //Game.PrintChat("Cast skill ward: " + posDest.Distance);
                                if (Ward.Jump(posDest.Position))
                                {
                                    flee = true;
                                }
                            
                        }
                        //No target?
                        //break;
                    }
                    if (flee)
                    {
                        tur = ChampionData.Player.GetTurret();
                        if (tur == null)
                        {
                            return;
                        } 
                        //Game.PrintChat("Tiep tuc di bo khoi tam tru khi dung skill");
                        Champion.MoveTo(ChampionData.Player.ShortestPath(tur.GetSafePosCircle(TurretData.SafeDist)).Position);
                    }
                    else
                    {
                        //Game.PrintChat("Tiep tuc di bo khoi tam tru khi ko dung skill");
                        Champion.MoveTo(posDest.Position);
                    }
                }
                //Logger.Save();
                _lastSave = Environment.TickCount + 1000;
            }
        }
        //public static void Safe(bool leaveNow = false)
        //{
        //    if (Environment.TickCount > LastSave)
        //    {
        //        var tur = ChampionData.Player.Position.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy);
        //        if (tur == null)
        //        {
        //            return;
        //        }
        //        Logger.Add("Player ở dưới trụ " + tur.Name);
        //        var objAiBases =
        //            ObjectManager.Get<Obj_AI_Base>().Where(minion =>minion.IsVisible && !minion.IsDead && !minion.IsMe &&minion.Distance(tur) < (TurretData.CastRange + 350) && !minion.IsValid<Obj_AI_Turret>());
        //        var minionsInRange = objAiBases as IList<Obj_AI_Base> ?? objAiBases.ToList();
        //        if (leaveNow ||minionsInRange.Count(minion => minion.IsAlly && minion.Distance(tur) <= TurretData.CastRange) <=MinMinion)
        //        {
        //            //Duong chim bay khoi tru
        //            var posMove = tur.Position.Extend(ChampionData.Player.Position, TurretData.SafeDist);
        //            Logger.Add("Khoảng cách ra ngoài trụ theo đường chim bay: " + ChampionData.Player.Distance(posMove));
        //            if (posMove.IsWall() || (posMove.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy) != null))
        //            {
        //                //Console.WriteLine("posMove.IsWall() "+Helper.GetPosCircle(tur.Position, TurretData.SafeDist).Count());
        //                posMove =
        //                    Helper.GetPosCircle(tur.Position, TurretData.SafeDist)
        //                        .Where(
        //                            v3 =>
        //                                !v3.IsWall() &&
        //                                v3.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy) == null)
        //                        .Aggregate(
        //                            (v1, v2) =>
        //                                ChampionData.Player.Distance(v1) < ChampionData.Player.Distance(v2) ? v1 : v2);
        //                Logger.Add(
        //                    "Điểm kết thúc là bức tường hoặc trụ nên tìm điểm khác, Kc : " +ChampionData.Player.Distance(posMove));
        //            }
        //            //blink or Dash
        //            bool flee = false;
        //            foreach (
        //                var spell in
        //                    SpellData.PlayerSpells.Where(
        //                        spell =>
        //                            spell.EvadeType != EvadeType.None && spell.EvadeType != EvadeType.Shield &&
        //                            spell.IsReady()))
        //            {
        //                Logger.Add("Người chơi có skill né : " + spell.Slot);
        //                Obj_AI_Base ePos = ChampionData.Player;
        //                if (!spell.CanTarget(TargetType.None))
        //                {
        //                    Logger.Add("Skill né là targeted");
        //                    float minionToTurretMax = 0;
        //                    float minDistMove = ChampionData.Player.Distance(posMove);
        //                    foreach (var minion in spell.CanCast(minionsInRange))
        //                    {
        //                        var turretOnMinion =
        //                            minion.Position.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy);
        //                        var minionToTurretDist =
        //                            minion.Distance(turretOnMinion != null ? turretOnMinion.Position : tur.Position);
        //                        var distMove = Math.Max(0, ChampionData.Player.Distance(minion) - spell.Range) +
        //                                       Math.Max(0, TurretData.SafeDist - minionToTurretDist);
        //                        if (distMove < minDistMove)
        //                        {
        //                            flee = true;
        //                            minDistMove = distMove;
        //                            ePos = minion;
        //                            Logger.Add(
        //                                "Phát hiện điểm đến khác có khoảng cách đi gần hơn nếu dùng skill, Kc: " +
        //                                distMove + " Minion: " + minion.Name);
        //                        }
        //                        else if (distMove == minDistMove && flee && minionToTurretDist > minionToTurretMax)
        //                        {
        //                            Logger.Add(
        //                                "Phát hiện điểm đến khác có khoảng cách bằng nhưng ở xa trụ hơn nếu dùng skill");
        //                            minionToTurretMax = minionToTurretDist;
        //                            ePos = minion;
        //                        }
        //                    }
        //                    if (flee)
        //                    {
        //                        Logger.Add("Cast tại minion " + ePos.Name);
        //                        spell.SmartCast(ePos);
        //                    }
        //                    else if (spell.CanTarget(TargetType.AllyWard) && Ward.GetWardSlot() != null)
        //                    {
        //                        //Console.WriteLine(Helper.GetPosCircle(ChampionData.Player.Position, Ward.CastRange).Count(v3 => !v3.IsWall() && v3.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy) == null));
        //                        var pos =
        //                            Helper.GetPosCircle(ChampionData.Player.Position, Ward.CastRange)
        //                                .Where(
        //                                    v3 =>
        //                                        !v3.IsWall() &&
        //                                        v3.UnderTurret(TestOnAll ? TurretTeam.Ally : TurretTeam.Enemy) == null)
        //                                .Aggregate((v1, v2) => tur.Distance(v1) > tur.Distance(v2) ? v1 : v2);
        //                        Logger.Add("Không có minion nên dùng ward với khoảng cách trụ: " + tur.Distance(pos));
        //                        Ward.Jump(pos);
        //                    }
        //                }
        //                if (tur.Distance(ePos) <= TurretData.CastRange)
        //                {
        //                    Logger.Add("Tiếp tục đi bộ ra khỏi tầm trụ sau khi dùng skill");
        //                    Champion.MoveTo(tur.Position.Extend(ePos.Position, TurretData.SafeDist));
        //                }
        //                break;
        //            }
        //            if (!flee)
        //            {
        //                Logger.Add("Đi bộ ra khỏi tầm trụ sau khi không có skill");
        //                Champion.MoveTo(tur.Position.Extend(posMove, TurretData.SafeDist));
        //            }
        //        }
        //        Logger.Save();
        //        LastSave = Environment.TickCount + 5000;
        //    }
        //}
    }
}
