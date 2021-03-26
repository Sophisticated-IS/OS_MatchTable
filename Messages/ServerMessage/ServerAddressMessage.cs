using ProtoBuf;

namespace Messages.ServerMessage
{
    [ProtoContract]
    public sealed class ServerAddressMessage : Base.ServerMessage
    {
        [ProtoMember(1)]
        public string IP { get; set; }

        [ProtoMember(2)]
        public int Port { get; set; }
    }
}