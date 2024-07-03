using System;

public class CRC16Calculator
{
    private const ushort polynomial = 0x8005;
    private const ushort init = 0xFFFF;
    private const ushort xorout = 0x0000;
    private ushort[] table = new ushort[256];

    public CRC16Calculator()
    {
    }
    public UInt16 ModRTU_CRC(byte[] buf, int len)
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
}
