namespace Barista.WebSocket.Protocol.FramePartReader
{
  internal interface IDataFramePartReader
  {
    int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);
  }
}