using System;
using Castle.Core;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.Sevices;
using tWpfMashUp_v0._0._1.Extensions;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels.Interfaces;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels
{
    public delegate void TurnChangedEventHandler(bool value);

    public class GameBoard : IGameBoard
    {
        int inHouseCount;
        int totalSolidersCount;
        int TotalSolidersCount
        {
            get => totalSolidersCount;
            set { totalSolidersCount = value; if (totalSolidersCount == 0) AnnounceAsWinner(); }
        }


        private bool allowTakeOuts => inHouseCount == TotalSolidersCount;
        private List<int> rollsValues;
        private TaskCompletionSource<SoliderModel> pickStackForSolider;
        Grid takeOutGrid;
        public event TurnChangedEventHandler TurnChanged;


        private readonly SignalRListenerService signalRListener;
        private readonly List<MoveOption> options;
        private readonly GameService gameService;
        private readonly StoreService store;
        public Grid GameGrid { get; private set; }
        public StackModel FocusedStack { get; set; }
        public StackModel GhostStack { get; set; }
        public StackModel[,] StacksMatrix { get; set; }
        public SoliderModel FocusedSolider { get; set; }
        public int MatrixColumnsCount { get => StacksMatrix.GetLength(0); }
        public int MatrixRowsCount { get => StacksMatrix.GetLength(1); }
        private bool isMyTurn;
        public bool IsMyTurn
        {
            get => isMyTurn;
            set
            {
                isMyTurn = value;
                TurnChanged?.Invoke(value);
            }
        }

        public GameBoard(SignalRListenerService signalRListener, GameService gameService, StoreService store)
        {
            this.store = store;
            this.gameService = gameService;
            this.signalRListener = signalRListener;
            this.signalRListener.OpponentPlayed += UpdateOpponentMove;
            this.signalRListener.OpponentFinnishedPlay += (s, e) => IsMyTurn = true; ;
            options = new List<MoveOption>();
            rollsValues = new List<int>();
            inHouseCount = 0;
        }


        public async Task UpdateRollsResult(List<int> newVals)
        {
            rollsValues = newVals;
            options.Clear();
          
            if (!HasAvailableMoves())
            {
                SkipTurn(); return;
            }
            if (GhostStack.Count > 0)
            {
                FocusedStack = GhostStack;
                FocusedSolider = GhostStack.Peek();
                StackModel.HasFirstSelected = true;
                MarkAvailableMoves(rollsValues, GhostStack.Location);
                foreach (var opt in options)
                {
                    StacksMatrix[opt.Location.Col, opt.Location.Row].MarkStackAsOption(true);
                }
                StackModel.HasFirstSelected = true;
                try
                {
                    await GetSelectionAsync();
                }
                finally { }
            }
            options.Clear();
        }

        private bool HasAvailableMoves()
        {
            if (GhostStack.Count > 0)
            {
                MarkAvailableMoves(rollsValues, new MatrixLocation { Col = 12, Row = 0 });
                return  options.Any();
                
            }
            for (int row = 0; row < MatrixRowsCount; row++)
            {
                for (int col = 0; col < MatrixColumnsCount; col++)
                {
                    if (StacksMatrix[col, row].HasMineSoliders())
                    {
                        MarkAvailableMoves(rollsValues, new MatrixLocation { Col = col, Row = row });
                        if (options.Any())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void SkipTurn()
        {
            Modal.ShowModal($"It seems like you have no available moves \n for {rollsValues[0]} {rollsValues[1]}", "Bad luck");
            rollsValues.Clear();
            gameService.UpdateTurnChangedAsync();
            isMyTurn = false;
        }

        public IGameBoard Build(Grid gameGrid) =>
                                    SetInitialVaulues(gameGrid)
                                    .Clear()
                                    .BuildGameBoardDefenitions(12, 2)
                                    .FillGameboardMatrix()
                                    .PlaceSolidersInInitialState()
                                    .BuildMatirxMovementAbility()
                                    .BuildBoardoutGrid();

        private GameBoard BuildBoardoutGrid()
        {
            takeOutGrid = new Grid { Background = new SolidColorBrush(Color.FromArgb(125, 250, 250, 250)) };
            Grid.SetRowSpan(takeOutGrid, 3);
            Grid.SetColumnSpan(takeOutGrid, 6);
            Panel.SetZIndex(takeOutGrid, 3);
            takeOutGrid.Visibility = Visibility.Collapsed;
            takeOutGrid.Height = Double.NaN; takeOutGrid.Width = Double.NaN;
            GameGrid.Children.Add(takeOutGrid);
            return this;
        }

        private void RemoveSolider(MoveOption option)
        {
            if (pickStackForSolider != null)
            {

                FocusedStack.Pop();
                foreach (var opt in options)
                {
                    if (opt.Location.Col == 12)
                    {
                        takeOutGrid.Visibility = Visibility.Collapsed;
                        continue;
                    }
                    StacksMatrix[opt.Location.Col, opt.Location.Row].MarkStackAsOption(false);
                }
                rollsValues.Remove(option.DicerollValue);
                options.Clear();
                inHouseCount--;
                TotalSolidersCount--;
                pickStackForSolider.TrySetResult(FocusedSolider);
                pickStackForSolider = null;
                var move = new Pair<MatrixLocation, MatrixLocation>(FocusedStack.Location, new MatrixLocation { Col = 12, Row = 1 });
                if (totalSolidersCount == 0)
                {
                    gameService.AnnounceAsWinnerAsync();
                }
                gameService.UpdateServerMove(move);
                if (rollsValues.Count == 0 || !HasAvailableMoves())
                {
                    IsMyTurn = false;
                    gameService.UpdateTurnChangedAsync();
                }
            }
        }

        private GameBoard SetInitialVaulues(Grid grid)
        {
            IsMyTurn = store.Get(CommonKeys.IsMyTurn.ToString());
            GameGrid = grid;
            inHouseCount = 5;
            TotalSolidersCount = 15;
            GhostStack = new StackModel(new MatrixLocation { Row = 0, Col = 12 }).Build();
            GhostStack.UiStack = new StackPanel();
            return this;
        }

        public void AddStackToGameGridAndMatrix(StackModel stck, int row, int col)
        {
            StacksMatrix[stck.Location.Col, stck.Location.Row] = stck;
            GameGrid.AddToGrid(stck.Triangle, col, row);
            GameGrid.AddToGrid(stck.UiStack, col, row);
            Panel.SetZIndex(stck.Triangle, 1);
            Panel.SetZIndex(stck.UiStack, 2);
        }

        private GameBoard BuildMatirxMovementAbility()
        {
            foreach (var stack in StacksMatrix)
            {
                stack.OnClicked += Stack_OnClicked;
                stack.OnSelected += (s, e) => Stack_OnSelected(s, e);
            }
            return this;
        }

        private async void Stack_OnClicked(object sender, EventArgs e)
        {
            if (IsMyTurn &&
                ((StackModel)sender).Count > 0 &&
                ((StackModel)sender).HasMineSoliders() &&
                rollsValues.Count > 0)
            {
                FocusedStack = (StackModel)sender;
                FocusedSolider = FocusedStack.Peek();
                StackModel.HasFirstSelected = true;
                FocusedStack.MarkSoliderAsActive(true);

                MarkAvailableMoves(rollsValues, FocusedSolider.Location);
                foreach (var opt in options)
                {
                    if (opt.Location.Col == 12)
                    {
                        takeOutGrid.Visibility = Visibility.Visible;
                        MouseButtonEventHandler handler = (s, e) => { };
                        handler = (s, e) => { RemoveSolider(opt); try { takeOutGrid.MouseDown -= handler; } finally { } };
                        takeOutGrid.MouseDown += handler;
                    }
                    else
                        StacksMatrix[opt.Location.Col, opt.Location.Row].MarkStackAsOption(true);
                }
                try
                {
                    await GetSelectionAsync();
                }
                finally { }
            }
            else { }
        }

        private async Task<SoliderModel> GetSelectionAsync()
        {
            pickStackForSolider = new TaskCompletionSource<SoliderModel>();
            try
            {
                return await pickStackForSolider.Task;
            }
            finally
            {
                StackModel.HasFirstSelected = false;
                FocusedSolider = null;
                FocusedStack = null;
            }
        }

        private void Stack_OnSelected(object sender, EventArgs e)
        {
            //De-selecting
            if ((StackModel)sender == FocusedStack)
            {
                if (pickStackForSolider != null)
                {
                    pickStackForSolider.TrySetResult(FocusedSolider);
                    pickStackForSolider = null;
                }
                try { FocusedStack.MarkSoliderAsActive(false); } finally { FocusedStack = null; }

                foreach (var opt in options)
                {
                    if (opt.Location.Col == 12)
                    {
                        takeOutGrid.Visibility = Visibility.Collapsed;
                        continue;
                    }
                    StacksMatrix[opt.Location.Col, opt.Location.Row].MarkStackAsOption(false);
                }
                options.Clear();
            }
            //Selecting
            else if (((StackModel)sender).IsOption)
            {
                if (pickStackForSolider != null)
                {
                    if (((StackModel)sender).CanStepInto())
                    {
                        FocusedStack.MarkSoliderAsActive(false);
                        //make location switches
                        var newStack = (StackModel)sender;
                        if (newStack.HasEnemyNoHouse()) newStack.Pop();
                        FocusedStack.Pop();
                        newStack.Push(FocusedSolider);
                        //demark options
                        foreach (var opt in options)
                        {
                            if (opt.Location.Col == 12)
                            {
                                takeOutGrid.Visibility = Visibility.Collapsed;
                                continue;
                            }
                            StacksMatrix[opt.Location.Col, opt.Location.Row].MarkStackAsOption(false);
                        }

                        var toRemove = options.FirstOrDefault(o => o.Location.Equals(newStack.Location));
                        if (toRemove != null) rollsValues.Remove(toRemove.DicerollValue);
                        options.Clear();

                        if (newStack.Location.Row == 1 && newStack.Location.Col >= 6 && FocusedStack.Location.Col < 6)
                            inHouseCount++;

                        pickStackForSolider.TrySetResult(FocusedSolider);
                        pickStackForSolider = null;

                        var move = new Pair<MatrixLocation, MatrixLocation>(FocusedStack.Location, newStack.Location);
                        gameService.UpdateServerMove(move);

                        if (rollsValues.Count == 0 || !HasAvailableMoves())
                        {
                            IsMyTurn = false;
                            gameService.UpdateTurnChangedAsync();
                        }
                    }
                }
            }
        }

        private void UpdateOpponentMove(object sender, OpponentPlayedEventArgs e)
        {
            SoliderModel solider;
            if (e.Source.Col == 12)
            {
                solider = new SoliderModel();
                solider.IsOwnSolider = false;
                solider.Soldier = new System.Windows.Shapes.Ellipse
                {
                    Stretch = Stretch.UniformToFill,
                    Width = double.NaN,
                    Fill = new SolidColorBrush(Colors.Black)
                };
            }
            else
            {
                solider = StacksMatrix[e.Source.Col, e.Source.Row].Pop();
            }

            if (e.Destenation.Col < 12 && StacksMatrix[e.Destenation.Col, e.Destenation.Row].HasMineSoliders() && StacksMatrix[e.Destenation.Col, e.Destenation.Row].Count == 1)
            {
                GhostStack.Push(StacksMatrix[e.Destenation.Col, e.Destenation.Row].Pop());
                if (e.Destenation.Row == 1 && e.Destenation.Col >= 6)
                {
                    inHouseCount--;
                }
            }
            if (e.Destenation.Col < 12)
                StacksMatrix[e.Destenation.Col, e.Destenation.Row].Push(solider);
        }

        public void MarkAvailableMoves(List<int> rollRes, MatrixLocation selectedLocation)
        {
            options.Clear();

            foreach (var res in rollRes)
            {
                if (allowTakeOuts)
                {
                    if (selectedLocation.Row == 0) continue;
                    if (selectedLocation.Col + res == MatrixColumnsCount)
                    {
                        options.Add(new MoveOption { Location = new MatrixLocation { Col = 12, Row = 1 }, DicerollValue = res });
                        continue;
                    }
                    if (selectedLocation.Col + res > MatrixColumnsCount)
                    {
                        bool hasOneBefore = false;
                        for (int i = 6; i < selectedLocation.Col; i++)
                        {
                            if (StacksMatrix[i, 1].HasMineSoliders())
                            { hasOneBefore = true; break; }
                        }
                        if (hasOneBefore) continue;
                        options.Add(new MoveOption { Location = new MatrixLocation { Col = 12, Row = 1 }, DicerollValue = res });
                        continue;
                    }
                }
                if (selectedLocation.Row == 1)
                {
                    if (selectedLocation.Col + res < MatrixColumnsCount)
                    {
                        var op = res + selectedLocation.Col;
                        if (StacksMatrix[op, 1].CanStepInto())
                        {
                            options.Add(new MoveOption { Location = new MatrixLocation { Col = op, Row = 1 }, DicerollValue = res });
                            continue;
                        }
                    }
                }
                if (selectedLocation.Row == 0)
                {
                    if (selectedLocation.Col >= res)
                    {
                        var op = selectedLocation.Col - res;
                        if (StacksMatrix[op, 0].CanStepInto())
                        {
                            options.Add(new MoveOption { Location = new MatrixLocation { Col = op, Row = 0 }, DicerollValue = res });
                            continue;
                        }
                    }
                    if (selectedLocation.Col < res)
                    {
                        var op = res - (selectedLocation.Col + 1);
                        if (StacksMatrix[op, 1].CanStepInto())
                        {
                            options.Add(new MoveOption { Location = new MatrixLocation { Col = op, Row = 1 }, DicerollValue = res });
                            continue;
                        }
                    }
                }
            }

            if (options.Count == 0)
            {
                StackModel.HasFirstSelected = false;
                if (FocusedStack != null && FocusedStack.HasMineSoliders())
                    FocusedStack.MarkSoliderAsActive(false);
                if (pickStackForSolider != null)
                {
                    pickStackForSolider.TrySetResult(FocusedSolider);
                    pickStackForSolider = null;
                }
                FocusedStack = null; FocusedSolider = null;
            }
        }

        private void AnnounceAsWinner()
        {
            Modal.ShowModal("Winnerwinner chicken dinner", "Congrats");
            gameService.AnnounceAsWinnerAsync();
        }
    }
}