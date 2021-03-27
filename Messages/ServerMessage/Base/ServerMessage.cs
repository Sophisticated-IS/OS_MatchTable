using Messages.Base;
using ProtoBuf;

namespace Messages.ServerMessage.Base
{
    [ProtoContract]
    [ProtoInclude(4,typeof(ServerAddressMessage))]
    [ProtoInclude(5,typeof(GoalMessage))]
    public abstract class ServerMessage : Message
    {
        
    }
}