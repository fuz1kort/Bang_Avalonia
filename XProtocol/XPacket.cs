﻿using System.Text;
using Newtonsoft.Json;

namespace XProtocol;

public class XPacket
{
    public byte PacketType { get; private init; }
    public byte PacketSubtype { get; private init; }
    private List<XPacketField> Fields { get; } = new();
    private bool ChangeHeaders => false;

    private XPacket()
    {
    }

    private XPacketField GetField(byte id) => Fields.FirstOrDefault(field => field.FieldId == id)!;

    public bool HasField(byte id) => GetField(id) != null!;


    public T GetValue<T>(byte id)
    {
        var field = GetField(id);

        if (field == null)
            throw new Exception($"Field with ID {id} wasn't found.");

        var jsonString = Encoding.UTF8.GetString(field.Contents);
        return JsonConvert.DeserializeObject<T>(jsonString)!;
    }

    public void SetValue(byte id, object? obj)
    {
        var field = GetField(id);

        if (field == null!)
        {
            field = new XPacketField
            {
                FieldId = id
            };

            Fields.Add(field);
        }

        var jsonString = JsonConvert.SerializeObject(obj);

        var bytes = Encoding.UTF8.GetBytes(jsonString);

        field.FieldSize = (byte)bytes.Length;
        field.Contents = bytes;
    }


    public static XPacket Create(byte type, byte subtype)
    {
        return new XPacket
        {
            PacketType = type,
            PacketSubtype = subtype
        };
    }

    public byte[] ToPacket()
    {
        var packet = new MemoryStream();

        packet.Write(
            ChangeHeaders
                ? new byte[] { 0x95, 0xAA, 0xFF, PacketType, PacketSubtype }
                : new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

        // Сортируем поля по ID
        var fields = Fields.OrderBy(field => field.FieldId);

        // Записываем поля
        foreach (var field in fields)
        {
            packet.Write(new[] { field.FieldId, field.FieldSize }, 0, 2);
            packet.Write(field.Contents, 0, field.Contents.Length);
        }

        // Записываем конец пакета
        packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

        return packet.ToArray();
    }

    public static XPacket Parse(byte[] packet)
    {
        /*
         * Минимальный размер пакета - 7 байт
         * HEADER (3) + TYPE (1) + SUBTYPE (1) + PACKET ENDING (2)
         */
        if (packet.Length < 7)
            return null!;

        // var encrypted = false;

        // Проверяем заголовок
        if (packet[0] != 0xAF ||
            packet[1] != 0xAA ||
            packet[2] != 0xAF)
        {
            if (packet[0] != 0x95 &&
                packet[1] != 0xAA &&
                packet[2] != 0xFF)
                return null!;
        }

        var mIndex = packet.Length - 1;

        // Проверяем, что бы пакет заканчивался нужными байтами
        if (packet[mIndex - 1] != 0xFF ||
            packet[mIndex] != 0x00)
        {
            return null!;
        }

        var type = packet[3];
        var subtype = packet[4];

        var xpacket = new XPacket { PacketType = type, PacketSubtype = subtype };

        var fields = packet.Skip(5).ToArray();

        while (true)
        {
            if (fields.Length == 2) // Остались последние два байта, завершающие пакет.
                return xpacket;

            var id = fields[0];
            var size = fields[1];

            var contents = size != 0 ? fields.Skip(2).Take(size).ToArray() : null;

            xpacket.Fields.Add(new XPacketField
            {
                FieldId = id,
                FieldSize = size,
                Contents = contents!
            });

            fields = fields.Skip(2 + size).ToArray();
        }
    }
}