using CookWeb_Ver2.Data;

namespace CookWeb_Ver2.Ultilities
{
    public class SetFrameDataTobeSent
    {

        public void ChooseDataMachine(byte machineID, byte[] arrSent)
        {
            arrSent[0] = machineID;
            arrSent[14] = machineID;

            AddChecksumToFirst16Bytes(arrSent);

            switch (machineID)
            {
                case 1:
                    IoTManager.dataMachine01 = arrSent; break;
                case 2:
                    IoTManager.dataMachine02 = arrSent; break;
                case 3:
                    IoTManager.dataMachine03 = arrSent; break;
            }
        }

        //Tạo bản tin gửi các lệnh thông thường cho plc
        public void SetDataUsingSMRClass(byte[] Send_buffer, StepsMakeRecipes stepSelected, byte machineIndex)
        {
            Send_buffer[2] = 0x01; // Ma ham: nau mon 
            
            Send_buffer[4] = (byte)(int.Parse(stepSelected.ActionCode) + machineIndex);

            Send_buffer[6] = (byte)(stepSelected._Angle & 0xff);
            Send_buffer[7] = (byte)((stepSelected._Angle >> 8) & 0xff);

            Send_buffer[8] = Convert.ToByte(stepSelected._Speed);

            Send_buffer[10] = (byte)(stepSelected._FireLevel);

            if (int.Parse(stepSelected.ActionCode) == 4)
            {
                int time_bom_dau = (stepSelected._Capacity + stepSelected._Capacity/3);
                Send_buffer[12] = (byte)(time_bom_dau & 0xff);
                Send_buffer[13] = (byte)((time_bom_dau >> 8) & 0xff);
            }
            else if(int.Parse(stepSelected.ActionCode) == 7 || int.Parse(stepSelected.ActionCode) == 10)
            {
				int time_bom_nuoc = (stepSelected._Capacity+10)*10/50;
				Send_buffer[12] = (byte)(time_bom_nuoc & 0xff);
                Send_buffer[13] = (byte)((time_bom_nuoc >> 8) & 0xff);
            }
            else
            {
                int time_bom_nuoc = stepSelected._Capacity;
                Send_buffer[12] = (byte)(time_bom_nuoc & 0xff);
                Send_buffer[13] = (byte)((time_bom_nuoc >> 8) & 0xff);
            }
        }

        //Tạo bản tin gửi lệnh gật đỏ cho bếp: Bản tin ẩn
        public void PushActionAddTrayToBuffer(byte[] Send_buffer, StepsMakeRecipes stepSelected, byte machineIndex)
        {
            Send_buffer[2] = 0x01; // Ma ham: nau mon 

            Send_buffer[4] = (byte)(19+ machineIndex);

            Send_buffer[6] = (byte)(stepSelected._Angle & 0xff);
            Send_buffer[7] = (byte)((stepSelected._Angle >> 8) & 0xff);

            Send_buffer[8] = Convert.ToByte(stepSelected._Speed);

            Send_buffer[10] = (byte)(stepSelected._FireLevel);

            Send_buffer[12] = (byte)(stepSelected._Capacity & 0xff);
            Send_buffer[13] = (byte)((stepSelected._Capacity >> 8) & 0xff);
        }
        public void SetDataUsingActionCode(byte[] Send_buffer, int actionIndex, byte machineIndex)
        {
            Send_buffer[2] = 0x01; // Ma ham: nau mon 
            
            Send_buffer[4] = (byte)(actionIndex + machineIndex);
        }

        #region Compute CRC and check sum
        public void AddChecksumToFirst16Bytes(byte[] bytes)
        {
            CRC16Calculator crcCalculator = new CRC16Calculator();
            ushort crc = crcCalculator.ModRTU_CRC(bytes, 16); // Tính toán CRC chỉ cho 16 phần tử đầu tiên
            byte[] bytesCRC = BitConverter.GetBytes(crc);
            bytes[17] = (byte)(crc >> 8);  // Ghi phần thấp của CRC vào vị trí thứ 18
            bytes[16] = (byte)(crc & 0xFF); // Ghi phần cao của CRC vào vị trí thứ 17

            //bytes[16] = bytesCRC[1];  // Ghi phần thấp của CRC vào vị trí thứ 18
            //bytes[17] = bytesCRC[0]; // Ghi phần cao của CRC vào vị trí thứ 17
        }

        public bool VerifyChecksum(byte[] bytes)
        {
            CRC16Calculator crcCalculator = new CRC16Calculator();
            // Tính toán CRC cho 12 byte đầu tiên
            UInt16 crc = crcCalculator.ModRTU_CRC(bytes, 12);
            // Lấy giá trị CRC từ hai byte cuối cùng của bản tin
            UInt16 receivedCrc = (UInt16)(bytes[bytes.Length - 2] | (bytes[bytes.Length - 1] << 8));
            // So sánh giá trị tính được với giá trị nhận được
            return crc == receivedCrc;
        }
        #endregion

    }
}
