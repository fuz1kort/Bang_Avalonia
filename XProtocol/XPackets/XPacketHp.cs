using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketHp
{
    [XField(1)] public byte Hp;
    
    public XPacketHp() {}

    public XPacketHp(byte hp) => Hp = hp;
}