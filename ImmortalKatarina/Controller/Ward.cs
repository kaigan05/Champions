using System;
using System.Linq;
using ImmortalSerials.Objects;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SpellData = ImmortalSerials.Model.SpellData;

namespace ImmortalSerials.Controller
{
    public static class Ward
    {
        private static Vector3 _lastWardPos;
        private static int _lastJumpedTime;
        public const int CastRange = 599;
        private static readonly MySpell JumpSpell;
        static Ward()
        {
            foreach (var spell in SpellData.PlayerSpells.Where(spell => spell.IsFlee && spell.CanTarget(TargetType.Ally)))
            {
                JumpSpell = spell;
                GameObject.OnCreate += GameObject_OnCreate;
                break;
            }
        }
        public static InventorySlot GetWardSlot()
        {
            var wardIds = new[] { 3340, 3361, 3154, 2045, 2049, 2050, 2044, 2043 };
            return (from wardId in wardIds
                    where Items.CanUseItem(wardId)
                    select ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId))
                .FirstOrDefault();
        }

        public static bool Jump(Vector3 wardPosition)
        {
            if (JumpSpell == null || Environment.TickCount < _lastJumpedTime || !JumpSpell.IsReady())
            {
                return false;
            }
            var obj =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(minion => JumpSpell.CanCast(minion, true) && minion.Distance(wardPosition) <= 300)
                    .OrderBy(minion => minion.Distance(wardPosition))
                    .FirstOrDefault();
            if (obj != null)
            {
                JumpSpell.SmartCast(obj);
                _lastJumpedTime = Environment.TickCount + 2000;
                return true;
            }
            var slotWard = GetWardSlot();
            if (slotWard != null && Items.UseItem((int)slotWard.Id, wardPosition))
            {
                _lastWardPos = wardPosition;
                if(_lastJumpedTime > Environment.TickCount)
                    return true;
            }
            return false;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var ward = sender as Obj_AI_Minion;
            if (ward != null)
            {
                if ((ward.Name.Contains("Ward") && ward.Distance(_lastWardPos) < 100))
                {
                    JumpSpell.SmartCast(ward);
                    _lastJumpedTime = Environment.TickCount + 2000;
                }
            }
        }
    }
}
