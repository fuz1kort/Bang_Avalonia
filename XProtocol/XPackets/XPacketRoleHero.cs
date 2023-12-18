using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketRoleHero
{
    [XField(1)] public byte RoleType;

    [XField(2)] public string? HeroName;

    public XPacketRoleHero()
    {
    }

    public XPacketRoleHero(byte roleType, string? heroName)
    {
        RoleType = roleType;
        HeroName = heroName;
    }
}