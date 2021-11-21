using System;
using System.Windows.Controls;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.Extensions;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels.Interfaces;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels
{
    public class Dices : IDicesRoller
    {
        public Grid Grid;
        private readonly Random rnd = new();
        public List<int> RollsResultsValue { get; private set; }
        public List<Border> RollsResults { get; private set; }

        public Dices(Grid panel)
        {
            Grid = panel;
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        public List<int> Roll()
        {
            Grid.Children.Clear();
            RollsResultsValue = new();

            for (int i = 0; i < 2; i++)
            {
                var res = rnd.Next(1, 7);

                RollsResultsValue.Add(res);
            }
            if (RollsResultsValue[0] == RollsResultsValue[1])
            {
                RollsResultsValue.Add(RollsResultsValue[0]);
                RollsResultsValue.Add(RollsResultsValue[0]);
            }
            DisplayResult();
            return RollsResultsValue;
        }

        public int DisplayResult()
        {
            for (int i = 0; i < RollsResultsValue.Count; i++)
            {
                var brdr = new Border();
                Grid.AddToGrid(brdr, i);
                brdr.BuildElipses(RollsResultsValue[i]);
            }
            return RollsResultsValue.Count;
        }

        public bool HasValue() => RollsResultsValue != null && RollsResultsValue.Count < 0;

        public void ClearDices()
        {
            RollsResultsValue?.Clear();
            Grid.Children?.Clear();
        }
    }
}
