public enum Crc16Mode : ushort
{
    // ... (other modes omitted for brevity)
    IBM_NORMAL = 0x8005,
    // ...
}

public class Crc16
{
    readonly ushort[] table = new ushort[256];

    public ushort ComputeChecksum(params byte[] bytes)
    {
        ushort crc = 0xffff;
        for (int i = 0; i < bytes.Length; ++i)
        {
            byte index = (byte)(crc ^ bytes[i]);
            crc = (ushort)((crc >> 8) ^ table[index]);
        }
        return crc;
    }

    public byte[] ComputeChecksumBytes(params byte[] bytes)
    {
        ushort crc = ComputeChecksum(bytes);
        return BitConverter.GetBytes(crc);
    }

    public Crc16(Crc16Mode mode)
    {
        ushort polynomial = (ushort)mode;
        ushort value;
        ushort temp;
        for (ushort i = 0; i < table.Length; ++i)
        {
            value = 0;
            temp = i;
            for (byte j = 0; j < 8; ++j)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            table[i] = value;
        }
    }
}
public static class Program
{
    static UInt16 ModRTU_CRC(byte[] buf, int len)
    {
        UInt16 crc = 0xFFFF;

        for (int pos = 0; pos < len; pos++)
        {
            crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

            for (int i = 8; i != 0; i--)
            {    // Loop over each bit
                if ((crc & 0x0001) != 0)
                {      // If the LSB is set
                    crc >>= 1;                    // Shift right and XOR 0xA001
                    crc ^= 0xa001;
                }
                else                            // Else LSB is not set
                    crc >>= 1;                    // Just shift right
            }
        }
        // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
        return crc;
    }

    public static ushort CalculateCRC(byte[] data)
    {
        Crc16 crcCalc = new Crc16(Crc16Mode.IBM_NORMAL);
        ushort crc = crcCalc.ComputeChecksum(data);
        return crc;
    }

    static void Main(string[] args)
    {
        // Example usage: 01 06 2000 0012
        byte[] dataBytes = new byte[] { 0x00,0x00,0x01,0x00,0x02,0x00,0x03,0x00,0x04,0x00,0x05,0x00,0x06,0x00,0x07,0x00};
        ushort calculatedCrc = ModRTU_CRC(dataBytes,16);
        Console.WriteLine($"Calculated CRC-16: 0x{calculatedCrc:X4}");
    }
}


