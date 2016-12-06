using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace madafakathresh 
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += delegate
            {
                if (Player.Instance.Hero != Champion.Thresh)
                {
                    return;
                }

                #region Menu Stuff
                //mtavari meniu
                var menu = MainMenu.AddMenu("=MadafakaThresh=", "=MadafakaThresh=");

                menu.AddGroupLabel("=Hitchance=");
                var hitchances = new List<HitChance>();
                for (var i = (int)HitChance.Medium; i <= (int)HitChance.Immobile; i++)
                {
                    hitchances.Add((HitChance)i);
                }
                //samizne zona
                var slider = new Slider(hitchances[0].ToString(), 0, 0, hitchances.Count - 1);
                slider.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs) { slider.DisplayName = hitchances[changeArgs.NewValue].ToString(); };
                menu.Add("=hitchance=", slider);

                if (EntityManager.Heroes.Enemies.Count > 0)
                {
                    //chempioenbis amorcheva
                    menu.AddSeparator();
                    menu.AddGroupLabel("=Enabled targets=");
                    var addedChamps = new List<string>();
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => !addedChamps.Contains(enemy.ChampionName)))
                    {
                        addedChamps.Add(enemy.ChampionName);
                        menu.Add(enemy.ChampionName, new CheckBox(string.Format("{0} ({1})", enemy.ChampionName, enemy.Name)));
                    }
                }
                // nazakzebis gaketeba amis dedasvheci 
                menu.AddSeparator();
                menu.AddGroupLabel("=Drawings=");
               
                var predictions = menu.Add("=soon=", new CheckBox("=soon="));

                #endregion

                var Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1800, 70);
                var predictedPositions = new Dictionary<int, Tuple<int, PredictionResult>>();

                Game.OnTick += delegate
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Q.IsReady())
                    {
                        foreach (
                            var enemy in
                                EntityManager.Heroes.Enemies.Where(
                                    enemy => ((TargetSelector.SeletedEnabled && TargetSelector.SelectedTarget == enemy) || menu[enemy.ChampionName].Cast<CheckBox>().CurrentValue) &&
                                             enemy.IsValidTarget(Q.Range + 150) &&
                                             !enemy.HasBuffOfType(BuffType.SpellShield)))
                        {
                            var prediction = Q.GetPrediction(enemy);
                            if (prediction.HitChance >= hitchances[0])
                            {
                                predictedPositions[enemy.NetworkId] = new Tuple<int, PredictionResult>(Environment.TickCount, prediction);

                                // yvelaze magali shedegi 
                                if (prediction.HitChance >= hitchances[slider.CurrentValue])
                                {
                                    Q.Cast(prediction.CastPosition);
                                }
                            }
                        }
                    }
                };

            };
        }
    }
}