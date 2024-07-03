using CookWeb_Ver2.Data;
using CookWeb_Ver2.Models;
using CookWeb_Ver2.Ultilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Sockets;

namespace CookWeb_Ver2.Controllers
{
    public class ManualController : Controller
	{
        private readonly ApplicationDbContext _context;
        private SetFrameDataTobeSent _setFrameDataTobeSent = new SetFrameDataTobeSent();

        //Bản tin mẫu
        private byte[] ArrSent = new byte[18] { 0x01, 0x00, 0x01, 0x00, 0x0d, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x00, 0xb2, 0x1a };

        private static byte MachineIdTobeSelected;
        private static byte machineIndex;

        public ManualController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void SendDataManual()
        {
            _setFrameDataTobeSent.ChooseDataMachine(MachineIdTobeSelected, ArrSent);
            try
            {
                if (Program.Client != null)
                {
                    Program.Client.GetStream().WriteAsync(ArrSent, 0, IoTManager.dataMachine01.Length);
                }
            }
            catch (Exception) { throw; }
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ManualViewModel();
            ViewBag.MachineName = new SelectList(_context.Machines, "Name", "Name");
            return View(model);
        }

        [HttpPost]
        public ActionResult Connect(ManualViewModel model)
        {
            ViewBag.MachineName = new SelectList(_context.Machines, "Name", "Name", model.MachineId);
            if (ModelState.IsValid)
            {
                MachineIdTobeSelected = (byte)_context.Machines.Where(m => m.Name == model.MachineName).First().MachineId;
                machineIndex = (byte)((int)MachineIdTobeSelected - 1);
            }
            return View("ControlBoard", model);
        }

        [HttpPost] //chua co chinh xong
        public IActionResult ChangeAngle(int value)
        {
            ArrSent[6] = (byte)(value & 0xff);
            ArrSent[7] = (byte)((value >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 13, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult PumpOil(int value)
        {
            ArrSent[6] = (byte)(100 & 0xff);
            ArrSent[7] = (byte)((100 >> 8) & 0xff);

            int time_bom_dau = value + value /3;
            ArrSent[12] = (byte)(time_bom_dau & 0xff);
            ArrSent[13] = (byte)((time_bom_dau >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 4, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult PumpWater(int value)
        {
            ArrSent[6] = (byte)(100 & 0xff);
            ArrSent[7] = (byte)((100 >> 8) & 0xff);

            int time_bom_dau = (value +10) * 10 / 50;
            ArrSent[12] = (byte)(time_bom_dau & 0xff);
            ArrSent[13] = (byte)((time_bom_dau >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 7, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult PumpSauce(int value)
        {
            ArrSent[6] = (byte)(100 & 0xff);
            ArrSent[7] = (byte)((100 >> 8) & 0xff);

            int time_bom_dau = (value +10) * 10/50;
            ArrSent[12] = (byte)(time_bom_dau & 0xff);
            ArrSent[13] = (byte)((time_bom_dau >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 10, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Calib()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 1, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SprayWater()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 46, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Vitri1()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 22, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Vitri2()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 25, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Vitri3()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 28, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Vitri4()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 31, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL1()
        {
            ArrSent[12] = (byte)(01 & 0xff);
            ArrSent[13] = (byte)((01 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL2()
        {
            ArrSent[12] = (byte)(02 & 0xff);
            ArrSent[13] = (byte)((02 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL3()
        {
            ArrSent[12] = (byte)(03 & 0xff);
            ArrSent[13] = (byte)((03 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL4()
        {
            ArrSent[12] = (byte)(04 & 0xff);
            ArrSent[13] = (byte)((04 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL5()
        {
            ArrSent[12] = (byte)(05 & 0xff);
            ArrSent[13] = (byte)((05 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiNL6()
        {
            ArrSent[12] = (byte)(06 & 0xff);
            ArrSent[13] = (byte)((06 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 37, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TraNL()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 43, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiTPGN()
        {

            ArrSent[12] = (byte)(01 & 0xff);
            ArrSent[13] = (byte)((01 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 34, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GoiTPGiay()
        {
            ArrSent[12] = (byte)(02 & 0xff);
            ArrSent[13] = (byte)((02 >> 8) & 0xff);
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 34, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult TraTP()
        {
            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 40, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult FireLevel(int value)
        {
            ArrSent[6] = (byte)(25 & 0xff);
            ArrSent[7] = (byte)((25 >> 8) & 0xff);

            ArrSent[10] = (byte) value;

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 13, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult FireHeat(int value)
        {
            ArrSent[6] = (byte)(35 & 0xff);
            ArrSent[7] = (byte)((35 >> 8) & 0xff);

            
            ArrSent[12] = (byte)(value & 0xff);
            ArrSent[13] = (byte)((value >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 16, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult NLKho1(int value)
        {
            ArrSent[6] = (byte)(80 & 0xff);
            ArrSent[7] = (byte)((80 >> 8) & 0xff);

            
            ArrSent[12] = (byte)(value & 0xff);
            ArrSent[13] = (byte)((value >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 49, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult NLKho2(int value)
        {
            ArrSent[6] = (byte)(80 & 0xff);
            ArrSent[7] = (byte)((80 >> 8) & 0xff);

            ArrSent[12] = (byte)(value & 0xff);
            ArrSent[13] = (byte)((value >> 8) & 0xff);

            _setFrameDataTobeSent.SetDataUsingActionCode(ArrSent, 52, machineIndex);
            SendDataManual();

            return Json(new { success = true });
        }

    }
}
