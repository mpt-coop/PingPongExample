using LiteNetLib.Utils;

namespace MPTEventsTest
{
    // Packets must be INetSerializable
    // Prefer struct over class as structures are allocated on the stack and therefore faster
    // If your packet has data make them properties, example:
    // public int Value { get; set; }
    public struct PingPacket : INetSerializable
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
