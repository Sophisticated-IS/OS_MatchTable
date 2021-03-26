using Messages.Base;

namespace MessageHandler
{
    public static class MessageConverter
    {
        public static byte[] PackMessage(Message message)
        {
            var serializedMessage = MessageSerializer.Serialize(message);
            var compressedMessage = ByteCompressor.Compress(serializedMessage);

            return compressedMessage;
        }

        public static Message UnPackMessage(byte[] data)
        {
            var decompressData = ByteCompressor.DeCompress(data);
            var deserializedMessage = MessageSerializer.DeSerialize(decompressData);
            
            return deserializedMessage;
        }
    }
}