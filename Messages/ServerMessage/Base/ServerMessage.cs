using Messages.Base;
using ProtoBuf;

namespace Messages.ServerMessage.Base
{
    [ProtoContract]
    [ProtoInclude(4,typeof(ServerAddressMessage))]
    public abstract class ServerMessage : Message
    {
        
    }
}