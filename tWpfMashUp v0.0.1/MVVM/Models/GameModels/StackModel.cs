using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels
{
    public class StackModel
    {
        private Brush tColor;
        public static bool HasFirstSelected;

        //a triangle in the background,
        public Polygon Triangle { get; set; }

        // a stackpanel in foreground
        public StackPanel UiStack { get; set; }

        // a stack for data(no ui connection)
        public Stack<SoliderModel> SoliderStack { get; private set; }

        //a grid matrix location indicator
        public MatrixLocation Location { get; set; }

        public int Count { get => SoliderStack.Count; }
        public event EventHandler OnClicked;
        public event EventHandler OnSelected;
        public bool IsOption { get; private set; }
        public StackModel(MatrixLocation location) => Location = location;

        public void Clear()
        {

            UiStack.Children.Clear();
            SoliderStack.Clear();
        }

        public StackModel Build()
        {
            SoliderStack = new Stack<SoliderModel>();
            if (UiStack != null)
            {
            UiStack.IsHitTestVisible = true;
            UiStack.MouseDown += OnMouseDown;
            }

            if (Triangle != null)
            {
                tColor = Triangle.Fill;
                Triangle.IsHitTestVisible = true;
                Triangle.MouseDown += OnMouseDown;
            }
            return this;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HasFirstSelected)
            {
                OnSelected?.Invoke(this, new EventArgs());
            }
            else
            {
                OnClicked?.Invoke(this, new EventArgs());
            }
        }

        public void Push(SoliderModel solider)
        {
            if (solider == null) return;
            SoliderStack.Push(solider);
            solider.SetLocation(Location);
            if (Location.Row == 1) try { UiStack.Children.Insert(0, solider.Soldier); } finally { }
            else UiStack.Children.Add(solider.Soldier);
        }

        public SoliderModel Pop()
        {
            //validate not empty or null
            if (SoliderStack.Count > 0)
            {
                var solider = SoliderStack.Pop();

                if (UiStack != null)
                    UiStack.Children.Remove(solider.Soldier);

                return solider;
            }
            else return null;
        }

        internal bool HasMineSoliders() => SoliderStack.Any() ? SoliderStack.Peek().IsOwnSolider : false;

        public bool CanStepInto() => HasMineSoliders() || UiStack.Children.Count <= 1;

        public SoliderModel Peek() => SoliderStack.Peek();

        public void MarkSoliderAsActive(bool isActive)
        {
            var solider = SoliderStack.Peek();
            byte c = solider.IsOwnSolider ? (byte)255 : (byte)0;
            solider.Soldier.Fill = new SolidColorBrush(Color.FromArgb(isActive ? (byte)125 : (byte)255, c, c, c));
        }

        public void MarkStackAsOption(bool isOption)
        {
            this.IsOption = isOption;
            Triangle.Fill = isOption ? new SolidColorBrush(Colors.White) : tColor;
        }
        
        internal bool HasEnemyNoHouse() => SoliderStack.Count == 1 && !SoliderStack.Peek().IsOwnSolider;
    }
}
