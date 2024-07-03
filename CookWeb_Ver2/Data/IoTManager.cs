using CookWeb_Ver2.Ultilities;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace CookWeb_Ver2.Data
{
    public class IoTManager
	{
        //Biến đếm sử dụng trong lúc gọi và trả thảnh phẩm, nguyên liệu
        //Do lúc gọi và trả cần gửi nhiều bản tin khác nhau trong thời điểm
        public static byte counterCommon = 0;

		private readonly ApplicationDbContext _context;

		public IoTManager(ApplicationDbContext context)
		{
			_context = context;
        }

        //Đây là biến dùng trong Task gửi bản tin truyền thông (Program.cs)
        public static byte[] dataMachine01 = new byte[18];
        public static byte[] dataMachine02 = new byte[18];
        public static byte[] dataMachine03 = new byte[18];

        public void CheckOrders()
		{
            if (Program.Client != null && Program.Client.Connected && Program.flagReady && checkUsingConveyor())
            {
                try
                {
                    //Check nếu có bếp rảnh và có món đang chờ nấu
                    var machines = _context.Machines.Where(m => m.IsCooking == false && m.MachineTypeId == 2 && m.Activate == true).ToList();
                    if (machines == null || machines.Count() == 0)
                    {
                        return;
                    }
                    var orders = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine != null && p.Machine.MachineTypeId == 2).ToList();
                    if (orders == null || orders.Count() == 0)
                    {
                        return;
                    }

                    var machine = machines.First();
                    
                    
                    ////Check nếu món ăn phù hợp với bếp về loại món (takeaway hoặc eatin)
                    //for(byte i=0; i < machines.Count(); i++)
                    //{
                    //    orders = orders.Where(p => p.TypeFood == machines[i].typeTray && p.Status == 0 && p.Machine != null && p.Machine.MachineTypeId == 2).ToList();

                    //    if (orders.Count() != 0)
                    //    {
                    //        machine = machines[i];
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        if(i != machines.Count()-1)
                    //            orders = _context.OrderDetails.ToList();
                    //    }
                    //}
                    //if (orders == null || orders.Count() == 0)
                    //{
                    //    return;
                    //}

                    var order = orders.First();

                    var recipe = _context.Recipes.Where(r => r.Id == order.RecipesID).FirstOrDefault();
                    
                    //Sau khi chọn được món và bếp match với nhau thì mình tiến hành update database cho món và gửi hàm nấu.
                    order.MachineId = machine.MachineId;
                    order.Status = 1;
                    order.StartTime = DateTime.Now;
                    order.ErrorCode = 0;
                    

                    _context.OrderDetails.Update(order);

                    machine.IsCooking = true;
                    _context.Machines.Update(machine);
                    _context.SaveChanges();
                    Program.flagReady = false;

                    //Đây là hàm nấu món
                    UpdateBufferToBeSent(machine, order);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
                return;
		}

        //Cờ báo trạng thái cho 3 bếp
        public static bool[] TraTPDone = new bool[3];
        public static bool[] NL_traDone = new bool[3];
        public static bool[] CalibEndDone = new bool[3];
        public static bool[] XitRuaDone = new bool[3];
        public static bool[] isCalib = new bool[3];
        public static bool[] isCallTP = new bool[3];
        public static bool[] IsNLCall = new bool[3];

        public static bool[] isUseConveyor = new bool[3];

        // Gửi lệnh nấu xuống bếp và cập nhật vào database 
        private void UpdateBufferToBeSent(Machine machine, OrderDetail order)
        {
            SetFrameDataTobeSent _frameDataTobeSent = new SetFrameDataTobeSent();
            bool IsAcpRun = true;

            //Mảng gửi và nhận data
            byte[] send_buffer_tmp = new byte[18];

            //các biến bool dùng để cập nhật trạng thái cho bếp
            bool  isUpdateAct = false, updateflagReady = false, AvailableEnd = false;
            bool IsPreparingDone = false, IsCookingDone = false, IsEndingDone = false;

            //Lấy list công thức để nấu
            List<StepsMakeRecipes> lstStep = _context.StepsMakeRecipes.Where(m => m.RecipesId == order.RecipesID).OrderBy(o => o.StepNumber).ToList();

            byte machineId = (byte) machine.MachineId;
            byte machineIndex = (byte)(machineId - 1);

            //update new status for flag of machine
            IsNLCall[machineIndex] = false;
            isCallTP[machineIndex] = false;
            isCalib[machineIndex] = false;
            XitRuaDone[machineIndex] = false;
            CalibEndDone[machineIndex] = false;
            NL_traDone[machineIndex] = false;
            TraTPDone[machineIndex] = false;

            isUseConveyor[machineIndex] = true;

            //Ban tin fix
            //Ban tin mau da duoc fix san
            
            byte[] ArrCalib =           [0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x65, 0xc7];
            byte[] ArrCallThanhPham =   [0x00, 0x00, 0x01, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x66, 0x86];
            byte[] ArrCallNgLieu =      [0x00, 0x00, 0x01, 0x00, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0xd7, 0x5c];
            byte[] ArrTraThanhPham =    [0x00, 0x00, 0x01, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x07, 0x73];
            byte[] ArrTraNguyenLieu =   [0x00, 0x00, 0x01, 0x00, 0x2b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0xf7, 0x7c];
            byte[] ArrXitRua =          [0x00, 0x00, 0x01, 0x00, 0x2e, 0x00, 0x9c, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0xb2, 0x1a];
            byte[] ArrPause =           {0x00, 0x00, 0x01, 0x00, 0x0d, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0xb2, 0x1a };

            //Bien dem so cong thuc dang duoc gui
            ushort count = 0;

            #region Send to cook
            CancellationTokenSource sentocookCancel = new CancellationTokenSource();
            Task SendToCook = Task.Run(async () =>
            {
                while (!IsEndingDone && IsAcpRun)
                {
                    if (!IsPreparingDone)
                    {
                        if (!isCallTP[machineIndex] && counterCommon == 0)
                        {
                            ArrCallThanhPham[4] = (byte)(34 + machineIndex);

                           if(order.TypeFood == "eatin")
                            {
                                ArrCallThanhPham[12] = 0x01;
                            }

                           if (order.TypeFood == "takeaway")
                            {
                                ArrCallThanhPham[12] = 0x02;
                            }
                            
                            //Sua ban tin call thanh pham
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrCallThanhPham);
                        }

                        if (!isCalib[machineIndex] && counterCommon == 1)
                        {
                            ArrCalib[4] = (byte)(01 + machineIndex);
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrCalib);
                        }

                        else if (!IsNLCall[machineIndex] && isCalib[machineIndex] && counterCommon == 2)
                        {
                            var recipe = _context.Recipes.Where(r => r.Id == order.RecipesID).FirstOrDefault();

                            ArrCallNgLieu[4] = (byte)(37 + machineIndex);   
                            
                            switch(recipe.TypeDishId)
                            {
                                case 1:
                                    ArrCallNgLieu[12] = 0x01;
                                    break;
                                case 2:
                                    ArrCallNgLieu[12] = 0x02;
                                    break;
                                case 3:
                                    ArrCallNgLieu[12] = 0x03;
                                    break;
                                case 4:
                                    ArrCallNgLieu[12] = 0x04;
                                    break;
                                case 5:
                                    ArrCallNgLieu[12] = 0x05;
                                    break;
                                case 6:
                                    ArrCallNgLieu[12] = 0x06;
                                    break;
                            }

                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrCallNgLieu);
                        }
                    }
                    if (!IsCookingDone && isUpdateAct)
                    {
                        _frameDataTobeSent.ChooseDataMachine(machineId, send_buffer_tmp);
                    }

                    if (IsCookingDone && Program.flagReady && !AvailableEnd && checkPriorityMachine(machineIndex))
                    {
                        //enable flag bang tai
                        Program.flagReady = false;

                        AvailableEnd = true;
                    }
                    

                    if (!IsEndingDone && IsCookingDone && AvailableEnd)
                    {
                        if (!CalibEndDone[machineIndex] && TraTPDone[machineIndex] && NL_traDone[machineIndex] && XitRuaDone[machineIndex])
                        {
                            ArrCalib[4] = (byte)(01 + machineIndex);
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrCalib);
                        }

                        if (!NL_traDone[machineIndex] && counterCommon == 0)
                        {
                            ArrTraNguyenLieu[4] = (byte)(43 + machineIndex);
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrTraNguyenLieu);
                        }

                        if (!TraTPDone[machineIndex] && counterCommon == 1)
                        {
                            ArrTraThanhPham[4] = (byte)(40 + machineIndex);
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrTraThanhPham);
                        }

                        if (!XitRuaDone[machineIndex] && TraTPDone[machineIndex] && NL_traDone[machineIndex])
                        {
                            ArrXitRua[4] = (byte)(46 + machineIndex);
                            _frameDataTobeSent.ChooseDataMachine(machineId, ArrXitRua);
                        }
                    }
                    if (IsEndingDone)
                    {
                        sentocookCancel.Cancel();
                    }
                    await Task.Delay(100);
                }
            });
            #endregion

            #region Xử lý sự kiện dừng bếp bằng núy bấm
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task UpdateStatus = Task.Run(async () =>
            {
                while (!IsEndingDone && IsAcpRun)
                {
                    await Task.Delay(2000);
                    // Check if the task should be canceled
                    if (Program.isPauseCook[machineIndex])
                    {
                        // Set flags to false
                        IsAcpRun = false;
                        isUpdateAct = false;

                        // Perform actions
                        _frameDataTobeSent.ChooseDataMachine(machineId, ArrPause);

                        // Signal cancellation and perform cleanup
                        cancellationTokenSource.Cancel();

                        break; // Exit the loop
                    }
                }
            }, cancellationTokenSource.Token);
            #endregion

            #region Task update bản tin từ công thức món lấy từ sql
            CancellationTokenSource cancellationTokenSource_updateAct = new CancellationTokenSource();
            Task updateAct = Task.Run(async () =>
            {
                int sumOfsteps = lstStep.Count();
                while (!IsCookingDone && IsAcpRun)
                {
                    if (isUpdateAct)
                    {
                        ushort nowactioncode = ushort.Parse(lstStep[count].ActionCode);
                        int timerStep = 0;

                        //Handle action add tray sent data
                        if (nowactioncode >= 22 && nowactioncode <= 31)
                        {
                            //Nếu action code là thêm khay: Gửi action code 11 để gật đổ. Bởi vì đã di chuyển sang vị trí khay trước đó rồi
                            _frameDataTobeSent.PushActionAddTrayToBuffer(send_buffer_tmp, lstStep[count], machineIndex);
                            timerStep = lstStep[count]._Second * 1000;
                        }
                        else if(nowactioncode == 13)
                        {
                            //Gửi action code dieu chinh nau
                            _frameDataTobeSent.SetDataUsingSMRClass(send_buffer_tmp, lstStep[count], machineIndex);
                            timerStep = lstStep[count]._Second * 1000 + 5000;
                        }
                        else
                        {
                            //Gửi action code bình thường
                            _frameDataTobeSent.SetDataUsingSMRClass(send_buffer_tmp, lstStep[count], machineIndex);
                            timerStep = lstStep[count]._Second * 1000;
                        } 

                        //Send action pre-addtray and delay timer of step
                        if (count < sumOfsteps - 1)
                        {
                            ushort nextactioncode = ushort.Parse(lstStep[count + 1].ActionCode);

                            if (nextactioncode >= 22 && nextactioncode <= 31)
                            { 
                                await Task.Delay(timerStep - 5000);
                                //Cho khay di chuyen truoc sang vi tri
                                _frameDataTobeSent.SetDataUsingActionCode(send_buffer_tmp, nextactioncode, machineIndex);
                                await Task.Delay(6000);
                            }
                            else
                                await Task.Delay(timerStep);
                        }
                        else if (count == sumOfsteps - 1)
                        {
                            await Task.Delay(timerStep - 7000);
                            //Cho khay di chuyen truoc sang vi tri 4: action code : 31
                            _frameDataTobeSent.SetDataUsingActionCode(send_buffer_tmp, 31, machineIndex);
                            await Task.Delay(7000);
                        }
                        count++;
                    }
                    if (count == sumOfsteps)
                    {
                        //handle when step is finish
                        isUpdateAct = false;
                        IsCookingDone = true;
                        isUseConveyor[machineIndex] = true;
                        cancellationTokenSource_updateAct.Cancel();
                    }
                }
            }, cancellationTokenSource_updateAct.Token);
            #endregion

            //#region read data from plc
            //while (!IsEndingDone && IsAcpRun)
            //{
            //    if (Program.Client != null && Program.Client.Connected)
            //    {
            //        NetworkStream streamRead = Program.Client.GetStream();

            //        int bytesRead = streamRead.Read(read_buffer, 0, read_buffer.Length);

            //        // Check if any bytes were read
            //        if (bytesRead > 0 && _frameDataTobeSent.VerifyChecksum(read_buffer) && read_buffer[2] == 0x01 && read_buffer[4] == 0x0c)
            //        {
            //            byte updatedMachineIndex = (byte)(read_buffer[0] - 1);
            //            //update status of each machine when a data received from plc

            //            if (checkDataReceived(read_buffer,37,updatedMachineIndex))
            //                IsNLCall[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 1, updatedMachineIndex) && !isCalib[updatedMachineIndex])
            //                isCalib[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 34, updatedMachineIndex))
            //                isCallTP[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 40, updatedMachineIndex))
            //                TraTPDone[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 43, updatedMachineIndex))
            //                NL_traDone[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 1, updatedMachineIndex) && XitRuaDone[updatedMachineIndex] && TraTPDone[updatedMachineIndex] && NL_traDone[updatedMachineIndex])
            //                CalibEndDone[updatedMachineIndex] = true;


            //            else if (checkDataReceived(read_buffer, 46, updatedMachineIndex) && TraTPDone[updatedMachineIndex])
            //                XitRuaDone[updatedMachineIndex] = true;

            //            ////Ban tin doc loi gui ve tu plc
            //            //if (read_buffer[10] != 0x00 && order.ErrorCode == 0)
            //            //{
            //            //    order.ErrorCode = read_buffer[10];
            //            //    _context.OrderDetails.Update(order);
            //            //    _context.SaveChanges();
            //            //}

            //            //if (read_buffer[10] == 0x00 && order.ErrorCode != 0)
            //            //{
            //            //    order.ErrorCode = read_buffer[10];
            //            //    _context.OrderDetails.Update(order);
            //            //    _context.SaveChanges();
            //            //}
            //        }
            //    }
            //    //update process
            //    try
            //    {
            //        if (IsNLCall[machineIndex] && isCallTP[machineIndex] && !IsPreparingDone)
            //        {
            //            Program.flagReady = true;
            //            isUpdateAct = true;
            //            IsPreparingDone = true;
            //            isUseConveyor[machineIndex] = false;
            //        }
            //        if (TraTPDone[machineIndex] && NL_traDone[machineIndex] && !updateflagReady)
            //        {
            //            Program.flagReady = true;
            //            updateflagReady = true;
            //            isUseConveyor[machineIndex] = false;
            //        }
            //        if (CalibEndDone[machineIndex] && !IsEndingDone)
            //        {
            //            machine.IsCooking = false;
            //            _context.Machines.Update(machine);

            //            order.Status = 99;
            //            order.OrdinalNumber = 0;

            //            _context.OrderDetails.Update(order);
            //            _context.SaveChanges();
            //            IsEndingDone = true;

            //            // Consider adding a comment explaining why the loop should be exited here
            //            break;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        throw;
            //    }
            //}
            //#endregion

            while (!IsEndingDone && IsAcpRun)
            {
                //update process
                try
                {
                    if (IsNLCall[machineIndex] && isCallTP[machineIndex] && !IsPreparingDone)
                    {
                        Program.flagReady = true;
                        isUpdateAct = true;
                        IsPreparingDone = true;
                        isUseConveyor[machineIndex] = false;
                    }
                    if (TraTPDone[machineIndex] && NL_traDone[machineIndex] && !updateflagReady)
                    {
                        Program.flagReady = true;
                        updateflagReady = true;
                        isUseConveyor[machineIndex] = false;
                    }
                    if (CalibEndDone[machineIndex] && !IsEndingDone)
                    {
                        machine.IsCooking = false;
                        _context.Machines.Update(machine);

                        order.Status = 99;
                        order.OrdinalNumber = 0;

                        _context.OrderDetails.Update(order);
                        _context.SaveChanges();
                        IsEndingDone = true;

                        // Consider adding a comment explaining why the loop should be exited here
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }
        }
        private bool checkDataReceived(byte[] data, int actionCodeIndex, byte machineIndex)
        {
            byte[] ref_frame = new byte[18];
            
            switch (machineIndex)
            {
                case 0:
                    ref_frame = dataMachine01;
                    break;
                case 1:
                    ref_frame = dataMachine02;
                    break;
                case 2:
                    ref_frame = dataMachine03;
                    break;
            }

            return data[6] == (actionCodeIndex + machineIndex) && ref_frame[4] == data[6] && data[8] == 0x01;
        }

        private bool checkPriorityMachine(byte machineIndex)
        {
            switch (machineIndex)
            {
                case 0:
                    return true;
                case 1:
                    if (isUseConveyor[0])
                        return false;
                    return true;
                case 2:
                    if (isUseConveyor[1] || isUseConveyor[0])
                        return false;
                    return true;
                default: 
                    return false;
            }
        }

        private bool checkUsingConveyor()
        {
            //Neu co may dang dung bang tai thi false
            foreach(var item in isUseConveyor)
            {
                if (item)
                    return false;
            }
            return true;
        }
    }
}