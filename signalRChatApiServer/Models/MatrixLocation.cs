namespace signalRChatApiServer.Models
{
    public struct MatrixLocation
    {
        public int Row;
        public int Col;

        public MatrixLocation InverteLocation() => new MatrixLocation
        {
            Col = this.Col,
            Row = this.Row == 1 ? 0 : 1
        };
    }
}
