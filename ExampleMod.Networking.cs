using System.IO;

namespace SyntheticEvolution
{
    // This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
    partial class SyntheticEvolution
    {
        internal enum MessageType : byte
        {
            ExamplePlayerSyncPlayer,
            ExampleTeleportToStatue
        }

        // Override this method to handle network packets sent for this mod.
        //TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msgType = (MessageType)reader.ReadByte();

            switch (msgType)
            {
                default:
                    Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
                    break;
            }
        }
    }
}