
using ProtoBuf;

namespace Messages.Base
{
   [ProtoContract]
   [ProtoInclude(1,typeof(ClientMessage.Base.ClientMessage))]
   [ProtoInclude(2,typeof(ServerMessage.Base.ServerMessage))]
    public abstract class Message
    {
        
    }
}