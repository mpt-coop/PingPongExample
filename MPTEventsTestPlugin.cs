using BepInEx;
using Comfort.Common;
using EFT.UI;
using LiteNetLib;
using LiteNetLib.Utils;
using MPT.Plugin.Modding;
using MPT.Plugin.Modding.Events;
using MPT.Plugin.Networking;
using System;

namespace MPTEventsTest
{
    [BepInPlugin("com.pandahhcorp.mpteventstest", "MPTEventsTest", "1.0.0")]
    public class MPTEventsTestPlugin : BaseUnityPlugin
    {
        private NetDataWriter _writer;
        
        // Prioritize reusing one NetDataWriter instance over creating a new one every time
        private NetDataWriter GetNetDataWriter()
        {
            if (_writer == null)
            {
                _writer = new NetDataWriter();
            }
            else
            {
                _writer.Reset();
            }

            return _writer;
        }

        private void Awake()
        {
            // Adds a console command that will send a packet to all clients, which they will then send one back to us
            ConsoleScreen.Processor.RegisterCommand("ping", OnPingCommand);
        }

        // Gets called whenever we type ping into console
        private void OnPingCommand()
        {
            if (!Singleton<MPTServer>.Instantiated)
            {
                throw new Exception("Only the server can execute the ping command");
            }

            // This is our buffer, the data we write into here will be sent over the network
            NetDataWriter writer = GetNetDataWriter();

            // This is the packet that we want to send, empty initializer as it has no data (I prefer initializers on packets over constructors)
            PingPacket packet = new PingPacket { };

            // MPT handles serializing the packet into the buffer and sending the data for us, so we don't have to communicate with the NetManager ourselves
            Singleton<MPTServer>.Instance.SendDataToAll(writer, ref packet, DeliveryMethod.Unreliable);
        }

        private void OnEnable()
        {
            // Register client events
            MPTEventDispatcher.SubscribeEvent<MPTClientCreatedEvent>(OnMPTClientCreatedEvent);
            MPTEventDispatcher.SubscribeEvent<MPTClientDestroyedEvent>(OnMPTClientDestroyedEvent);

            // Register server events
            MPTEventDispatcher.SubscribeEvent<MPTServerCreatedEvent>(OnMPTServerCreatedEvent);
            MPTEventDispatcher.SubscribeEvent<MPTServerDestroyedEvent>(OnMPTServerDestroyedEvent);
        }

        private void OnDisable()
        {
            // Unregister client events
            MPTEventDispatcher.UnsubscribeEvent<MPTClientCreatedEvent>(OnMPTClientCreatedEvent);
            MPTEventDispatcher.UnsubscribeEvent<MPTClientDestroyedEvent>(OnMPTClientDestroyedEvent);

            // Unregister server events
            MPTEventDispatcher.UnsubscribeEvent<MPTServerCreatedEvent>(OnMPTServerCreatedEvent);
            MPTEventDispatcher.UnsubscribeEvent<MPTServerDestroyedEvent>(OnMPTServerDestroyedEvent);
        }

        // Client events
        private void OnMPTClientCreatedEvent(MPTClientCreatedEvent clientCreatedEvent)
        {
            // Start listening for the packet that we will be receiving from the server
            clientCreatedEvent.Client._packetProcessor.SubscribeNetSerializable<PingPacket>(OnPingPacket);
        }

        private void OnMPTClientDestroyedEvent(MPTClientDestroyedEvent clientDestroyedEvent)
        {
            // Remove the listener from the client
            clientDestroyedEvent.Client._packetProcessor.RemoveSubscription<PingPacket>();
        }

        // Server events
        private void OnMPTServerCreatedEvent(MPTServerCreatedEvent serverCreatedEvent)
        {
            // Start listening for the packet that we will be receiving from clients
            serverCreatedEvent.Server._packetProcessor.SubscribeNetSerializable<PongPacket>(OnPongPacket);
        }

        private void OnMPTServerDestroyedEvent(MPTServerDestroyedEvent serverDestroyedEvent)
        {
            // Remove the listener from the server
            serverDestroyedEvent.Server._packetProcessor.RemoveSubscription<PongPacket>();
        }

        private void OnPingPacket(PingPacket packet)
        {
            if (!Singleton<MPTClient>.Instantiated)
            {
                throw new Exception("Server sent ping but MPTClient is not instantiated");
            }

            ConsoleScreen.Log("Server sent ping, sending pong...");

            // Create our buffer
            NetDataWriter writer = GetNetDataWriter();

            // Create our packet
            PongPacket responsePacket = new PongPacket { };

            // Send data to the server
            Singleton<MPTClient>.Instance.SendData(writer, ref responsePacket, DeliveryMethod.Unreliable);
        }

        // Gets called after the clients receive the PingPacket sent by the server (us) and sends the PongPacket back
        private void OnPongPacket(PongPacket packet)
        {
            ConsoleScreen.Log("Client sent pong");
        }
    }
}
