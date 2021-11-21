using System.Collections.Generic;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels.Interfaces
{
    public interface IDicesRoller
    {
        List<int> Roll();
        int DisplayResult();
    }
}
