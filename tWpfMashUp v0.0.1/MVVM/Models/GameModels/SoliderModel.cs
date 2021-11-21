using System.Windows.Shapes;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels
{
    public class SoliderModel
    {
        public Ellipse Soldier { get; set; }

        public bool IsOwnSolider { get; set; }

        public MatrixLocation Location { get; private set; }

        public SoliderModel()
        {
            Soldier = new Ellipse();
        }

        internal void SetLocation(MatrixLocation location) => Location = location;
    }
}
