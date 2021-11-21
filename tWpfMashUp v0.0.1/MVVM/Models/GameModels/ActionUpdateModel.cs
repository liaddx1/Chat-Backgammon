using System;

namespace tWpfMashUp_v0._0._1.MVVM.Models.GameModels
{
    [Serializable]
    public class ActionUpdateModel
    {
        public int SourceCol { get; set; }
        public int DestenationCol { get; set; }
        public int SourceRow { get; set; }
        public int DestenationRow { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
