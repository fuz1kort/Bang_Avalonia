namespace XProtocol;

public enum XPacketType
{
    Unknown,
    Connection,
    PlayersList,
    UpdatedPlayerProperty,
    CardToTable,
    CardToReset,
    CardToPlayer,
    RemoveCard,
    BangToPlayer,
    Turn,
    Win,
    Lose
}