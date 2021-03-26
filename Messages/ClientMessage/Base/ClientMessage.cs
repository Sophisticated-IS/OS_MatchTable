using Messages.Base;
using ProtoBuf;

namespace Messages.ClientMessage.Base
{
    [ProtoContract]
    [ProtoInclude(3,typeof(WhoIsServerMessage))]
    public abstract class ClientMessage : Message
    {
        
    }
}