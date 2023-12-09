using System.Drawing;
using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketColor
{
    [XField(1)] public int Argb;
}