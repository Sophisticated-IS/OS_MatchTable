using ProtoBuf;

namespace Messages.ClientMessage
{
    [ProtoContract]
    public sealed class WhoIsServerMessage : Base.ClientMessage
    {
    }
}