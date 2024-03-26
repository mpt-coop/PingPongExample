using LiteNetLib.Utils;

namespace MPTEventsTest
{
    // Packets must be INetSerializable
    public class PongPacket : INetSerializable
    {
        // Our packets contain no data, but if they did we would push out data to the writer, or pull it from the reader here
        public void Deserialize(NetDataReader reader)
        {
        }

        public void Serialize(NetDataWriter writer)
        {
        }
    }
}
