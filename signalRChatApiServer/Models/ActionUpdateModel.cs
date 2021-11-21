using System;

namespace signalRChatApiServer.Models
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

        internal ActionUpdateModel InverseRows()
        {
            SourceRow = SourceRow == 1 ? 0 : 1;
            DestenationRow = DestenationRow == 1 ? 0 : 1;
            return this;
        }
    }
}
