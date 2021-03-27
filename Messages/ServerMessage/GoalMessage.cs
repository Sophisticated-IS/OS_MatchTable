using ProtoBuf;

namespace Messages.ServerMessage
{
    [ProtoContract]
    public sealed class GoalMessage : Base.ServerMessage
    {
        [ProtoMember(1)]
        public string Team { get; set; }
        [ProtoMember(2)]
        public string Player { get; set; }
    }
}