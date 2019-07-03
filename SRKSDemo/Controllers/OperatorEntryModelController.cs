using Newtonsoft.Json;
using SRKSDemo.OperatorEntryModelClass;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    public class OperatorEntryModelController : Controller
    {
        unitworksccsEntities1 db = new unitworksccsEntities1();
        // GET: OperatorEntryModel
        public ActionResult Index()
        {
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string OperatorName = Convert.ToString(Session["OUsername"]);
            int NumberOfMachine = Convert.ToInt32(Session["OMachineNo"]);
            string LoginTime = Convert.ToString(Session["LoginTime"]);
            int NoMachine = Convert.ToInt32(Session["OMachineNo"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            string ShiftName = GetShift();
            ViewBag.Shift = ShiftName;
            ViewBag.NoMachines = NoMachine;
            ViewBag.LoginTime = LoginTime;
            ViewBag.Operatorname = OperatorName;
            ViewBag.OperatorIDs = Operatorid;
            DateTime Corr = DateTime.Now.Date;
            List<MainDetails> MainDetailsListObj = new List<MainDetails>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                if (WorkOrderentryDet.Count != 0)
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    if (NextHour == 24)
                    {
                        NextHour = 00;
                    }
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);
                    foreach (var Row in WorkOrderentryDet)
                    {
                        int ShiftID = Row.ShiftID;
                        int PartsActual = 0, PartsTarget = 0, ShiftActual = 0, ShiftTarget = 0;
                        PartsActual = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.PartCount).FirstOrDefault();
                        PartsTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.TargetQuantity).FirstOrDefault();
                        var ShiftCountA = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (ShiftCountA.Count == 0)
                        {
                            ShiftActual = 0;
                        }
                        else
                        {
                            ShiftActual = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.PartCount);
                        }
                        var ShiftCountT = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                        if (ShiftCountT.Count() == 0)
                        {
                            ShiftTarget = 0;
                        }
                        else
                        {
                            ShiftTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.TargetQuantity);
                        }
                        int PartPerCycle = Row.PartsPerCycle;
                        MainDetails MainDet = new MainDetails();
                        MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Row.MachineID).Select(m => m.MachineDisplayName).FirstOrDefault();
                        MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Row.MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                        MainDet.PartsCountActual = (PartsActual) /** PartPerCycle*/;
                        MainDet.PartsCountTarget = (PartsTarget) /**PartPerCycle*/;
                        MainDet.ShiftCountAtcual = (ShiftActual) /**PartPerCycle*/;
                        MainDet.ShiftCountTarget = (ShiftTarget) /**PartPerCycle*/;
                        MainDet.WONumber = Row.Prod_Order_No;
                        MainDet.PartNumber = Row.FGCode;
                        MainDet.OperationNo = Row.OperationNo;
                        MainDet.WOStartTime = Convert.ToString(Row.WOStart);
                        //MainDet.Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                        MainDet.MachineId = Row.MachineID;
                        MainDet.WOQty = Row.Total_Qty;
                        MainDetailsListObj.Add(MainDet);
                    }
                }
                else
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    CorrectedDate = "2019-06-24";
                    MainDetails MainDet = new MainDetails();
                    MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                    MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                    MainDet.PartsCountActual = 0;
                    MainDet.PartsCountTarget = 0;
                    MainDet.ShiftCountAtcual = 0;
                    MainDet.ShiftCountTarget = 0;
                    MainDet.WONumber = "0";
                    MainDet.PartNumber = "0";
                    MainDet.OperationNo = "0";
                    //MainDet.Shift = GetShift();
                    MainDet.MachineId = Machineid;
                    MainDet.WOQty = 0;
                    MainDet.WOStartTime = "";
                    MainDetailsListObj.Add(MainDet);
                }
            }
            return View(MainDetailsListObj);
        }

        public string getmachindet()
        {
            string res = "";
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string OperatorName = Convert.ToString(Session["OUsername"]);
            int NumberOfMachine = Convert.ToInt32(Session["OMachineNo"]);
            string LoginTime = Convert.ToString(Session["LoginTime"]);
            int NoMachine = Convert.ToInt32(Session["OMachineNo"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            string ShiftName = GetShift();
            ViewBag.Shift = ShiftName;
            ViewBag.NoMachines = NoMachine;
            ViewBag.LoginTime = LoginTime;
            ViewBag.Operatorname = OperatorName;
            ViewBag.OperatorIDs = Operatorid;
            DateTime Corr = DateTime.Now.Date;
            List<MainDetails> MainDetailsListObj = new List<MainDetails>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                int Machineid = Convert.ToInt32(MachRow.machineId);
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                if (WorkOrderentryDet.Count != 0)
                {
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    //CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    if (NextHour == 24)
                    {
                        NextHour = 00;
                    }
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);
                    foreach (var Row in WorkOrderentryDet)
                    {
                        int ShiftID = Row.ShiftID;
                        MainDetails MainDet = new MainDetails();
                        MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Row.MachineID).Select(m => m.MachineDisplayName).FirstOrDefault();
                        MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Row.MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                        MainDet.PartsCountActual = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.PartCount).FirstOrDefault();
                        MainDet.PartsCountTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.TargetQuantity).FirstOrDefault();
                        MainDet.ShiftCountAtcual = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.PartCount);
                        MainDet.ShiftCountTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == Row.MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.TargetQuantity);
                        MainDet.WONumber = Row.Prod_Order_No;
                        MainDet.PartNumber = Row.FGCode;
                        MainDet.OperationNo = Row.OperationNo;
                        MainDet.WOStartTime = Convert.ToString(Row.WOStart);
                        //MainDet.Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                        MainDet.MachineId = Row.MachineID;
                        MainDet.WOQty = Row.Total_Qty;
                        MainDetailsListObj.Add(MainDet);
                    }
                }
                else
                {
                    MainDetails MainDet = new MainDetails();
                    MainDet.MachineName = db.tblmachinedetails.Where(m => m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                    MainDet.MachineStatusColor = db.tbllivemodes.Where(m => m.MachineID == Machineid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).Select(m => m.ColorCode).FirstOrDefault();
                    MainDet.PartsCountActual = 0;
                    MainDet.PartsCountTarget = 0;
                    MainDet.ShiftCountAtcual = 0;
                    MainDet.ShiftCountTarget = 0;
                    MainDet.WONumber = "0";
                    MainDet.PartNumber = "0";
                    MainDet.OperationNo = "0";
                    //MainDet.Shift = GetShift();
                    MainDet.MachineId = Machineid;
                    MainDet.WOQty = 0;
                    MainDet.WOStartTime = "";
                    MainDetailsListObj.Add(MainDet);
                }

            }
            res = JsonConvert.SerializeObject(MainDetailsListObj);
            return res;
        }

        //public string GetShift()
        //{
        //    string shift = "";
        //    DateTime Time1 = DateTime.Now;
        //    //TimeSpan Tm1 = new TimeSpan(Time1.Hour, Time1.Minute, Time1.Second);
        //    //var Shiftdetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm1 && m.EndTime >= Tm1).FirstOrDefault();
        //    //if (Shiftdetails != null)
        //    //{
        //    //    shift = Shiftdetails.ShiftName;
        //    //}
        //    int DayHours = Time1.Hour;
        //    if (DayHours >= 6 && DayHours < 14)
        //    {
        //        shift = "A";
        //    }
        //    else if (DayHours >= 14 && DayHours < 22)
        //    {
        //        shift = "B";
        //    }
        //    else
        //    {
        //        shift = "C";
        //    }

        //    return shift;
        //}

        public string GetShift()
        {
            string ShiftValue = "";
            DateTime DateNow = DateTime.Now;
            var ShiftDetails = db.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            foreach (var row in ShiftDetails)
            {
                int ShiftStartHour = row.StartTime.Value.Hours;
                int ShiftEndHour = row.EndTime.Value.Hours;
                int CurrentHour = DateNow.Hour;
                if (CurrentHour >= ShiftStartHour && CurrentHour <= ShiftEndHour)
                {
                    ShiftValue = row.ShiftName;
                    break;
                }
            }

            return ShiftValue;
        }

        public string GetPartsCount()
        {
            string result = "";
            DateTime Corr = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            List<PartsCountDet> PartCountList = new List<PartsCountDet>();
            var OpMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var Row in OpMachineDet)
            {
                int MachineID = Convert.ToInt32(Row.machineId);
                var WorkOrderEntryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == MachineID).ToList();
                if (WorkOrderEntryDet != null)
                {
                    int PartPerCycle = Convert.ToInt32(Session["PartPerCycle"]);
                    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    CorrectedDate = "2019-06-24";
                    int Hour = DateTime.Now.Hour;
                    int NextHour = Hour + 1;
                    DateTime StartHour = Convert.ToDateTime(CorrectedDate + " " + Hour + ":00:00");
                    DateTime EndHour = Convert.ToDateTime(CorrectedDate + " " + NextHour + ":00:00");
                    string ShiftGet = GetShift();
                    var Shift = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftName == ShiftGet).FirstOrDefault();
                    DateTime ShiftStratTime = Convert.ToDateTime(CorrectedDate + " " + Shift.StartTime);
                    DateTime ShiftEndTime = Convert.ToDateTime(CorrectedDate + " " + Shift.EndTime);

                    int PartsActual = 0, PartsTarget = 0, ShiftActual = 0, ShiftTarget = 0;
                    PartsActual = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.PartCount).FirstOrDefault();
                    PartsTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime == StartHour && m.EndTime == EndHour).Select(m => m.TargetQuantity).FirstOrDefault();
                    var ShiftCountA = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                    if (ShiftCountA.Count() == 0)
                    {
                        ShiftActual = 0;
                    }
                    else
                    {
                        ShiftActual = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.PartCount);
                    }
                    var ShiftCountT = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).ToList();
                    if (ShiftCountT.Count() == 0)
                    {
                        ShiftTarget = 0;
                    }
                    else
                    {
                        ShiftTarget = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.StartTime >= ShiftStratTime && m.EndTime <= ShiftEndTime).Sum(m => m.TargetQuantity);
                    }
                    PartsCountDet PartsCountObj = new PartsCountDet();
                    PartsCountObj.MachineID = MachineID;
                    PartsCountObj.PartsCountActual = (PartsActual) /** PartPerCycle*/;
                    PartsCountObj.PartsCountTarget = (PartsTarget) /** PartPerCycle*/;
                    PartsCountObj.ShiftCountAtcual = (ShiftActual) /** PartPerCycle*/;
                    PartsCountObj.ShiftCountTarget = (ShiftTarget)/* * PartPerCycle*/;
                    PartCountList.Add(PartsCountObj);
                }
            }
            result = JsonConvert.SerializeObject(PartCountList);
            return result;
        }

        public string GetShiftDet()
        {
            string Result = "";
            var ShiftDetails = db.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            Result = JsonConvert.SerializeObject(ShiftDetails);
            return Result;
        }

        public string GetHoldDet()
        {
            string Result = "";
            var ShiftDetails = db.tblholdcodes.Where(m => m.IsDeleted == 0).ToList();
            Result = JsonConvert.SerializeObject(ShiftDetails);
            return Result;
        }

        public string InsertData(int machineID, int Shift, string PartNo, string OPNO, string WONo, int WOQValue, string OperatorID, int PartPerCycle)
        {
            string result = "Fail";
            if (machineID != 0 && PartNo != null)
            {
                //Session["PartPerCycle"] = PartPerCycle;
                DateTime Correcteddate = DateTime.Now;
                tblworkorderentry obj = new tblworkorderentry();
                obj.MachineID = machineID;
                obj.WOStart = Correcteddate;
                obj.PartNo = PartNo;
                obj.ShiftID = Shift;
                obj.OperatorID = OperatorID;
                obj.Prod_Order_No = WONo;
                obj.OperationNo = OPNO;
                obj.Yield_Qty = 0;
                obj.ScrapQty = 0;
                obj.Total_Qty = WOQValue;
                obj.ProcessQty = 0;
                obj.Status = 0;
                obj.CorrectedDate = Correcteddate.Date;
                obj.IsStarted = 1;
                obj.IsFinished = 0;
                obj.IsPartialFinish = 0;
                obj.isWorkOrder = 1;
                obj.PEStartTime = Correcteddate;
                obj.FGCode = PartNo;
                obj.PartsPerCycle = PartPerCycle;
                db.tblworkorderentries.Add(obj);
                db.SaveChanges();
                result = "Success";
            }
            return result;

        }

        public string OperatorEntryDetails(int id)
        {
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            DateTime CorrectedDate = DateTime.Now.Date;
            string result = "FAIL";
            List<WOEntry> WOEntryList = new List<WOEntry>();
            var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == CorrectedDate && m.MachineID == id).FirstOrDefault();
            if (WorkOrderentryDet != null)
            {
                int ShiftID = WorkOrderentryDet.ShiftID;
                WOEntry WOEntryObj = new WOEntry();
                WOEntryObj.OperationNo = WorkOrderentryDet.OperationNo;
                WOEntryObj.PartNo = WorkOrderentryDet.FGCode;
                WOEntryObj.ShiftID = db.tblshift_mstr.Where(m => m.IsDeleted == 0 && m.ShiftID == ShiftID).Select(m => m.ShiftName).FirstOrDefault();
                WOEntryObj.WONO = WorkOrderentryDet.Prod_Order_No;
                WOEntryObj.WOQTY = WorkOrderentryDet.Total_Qty;
                WOEntryObj.PartPerCycle = WorkOrderentryDet.PartsPerCycle;
                WOEntryList.Add(WOEntryObj);
                result = JsonConvert.SerializeObject(WOEntryList);
            }
            return result;
        }

        public string GetMachineBaseStart()
        {
            string Result = "";
            DateTime Corr = DateTime.Now.Date;
            int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            List<MachineTrue> MachineTrueList = new List<MachineTrue>();
            var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
            foreach (var MachRow in OperatorMachineDetails)
            {
                MachineTrue MachineTrueobj = new MachineTrue();
                int Machineid = Convert.ToInt32(MachRow.machineId);
                MachineTrueobj.MachineID = Machineid;
                var WorkOrderentryDet = db.tblworkorderentries.Where(m => m.OperatorID == Operatorid && m.IsStarted == 1 && m.IsFinished == 0 && m.CorrectedDate == Corr && m.MachineID == Machineid).ToList();
                if (WorkOrderentryDet.Count != 0)
                {
                    MachineTrueobj.TrueorFalse = "True";
                }
                else
                {
                    MachineTrueobj.TrueorFalse = "False";
                }
                MachineTrueList.Add(MachineTrueobj);
            }
            Result = JsonConvert.SerializeObject(MachineTrueList);
            return Result;
        }

        public string UpdateData(int machineID, string PartNo, string OPNO, string WONo, int WOQValue, string OperatorID)
        {
            string result = "Fail";
            if (machineID != 0 && PartNo != null)
            {
                var WorkOrder = db.tblworkorderentries.Where(m => m.IsFinished == 0 && m.IsStarted == 1 && m.PartNo == PartNo && m.MachineID == machineID && m.Prod_Order_No == WONo).FirstOrDefault();
                DateTime Correcteddate = DateTime.Now;
                WorkOrder.WOEnd = Correcteddate;
                WorkOrder.IsFinished = 1;
                db.SaveChanges();
                result = "Success";
            }
            return result;
        }

        public string BreakDownDet(int Machineid)
        {
            string Result = "";
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var BreakDownTickectDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.operatorId == Operatorid && m.reasonId != null).OrderByDescending(m => m.id).FirstOrDefault();

            if (BreakDownTickectDet != null)
            {
                if (BreakDownTickectDet.MaintFinished == 1 && BreakDownTickectDet.ProdFinished == 1)
                {
                    var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).ToList();
                    Result = JsonConvert.SerializeObject(BreakdownDet);
                }
                else if (BreakDownTickectDet.mntStatus == true)
                {

                    string UserName = Convert.ToString(Session["maintUser"]);
                    string Password = Convert.ToString(Session["maintpwd"]);
                    var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
                    if (OperatorLoginDet != null)
                    {
                        Result = "maintAccept";
                        MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                        MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                        var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                        int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                        int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                        MacintanceAccpobj.Reason = GetReson(ReasonID);
                        MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                        MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                        MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                        MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                        MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.MaintFinished);
                        MacintanceAccpobj.result = Result;
                        Result = JsonConvert.SerializeObject(MacintanceAccpobj);
                    }
                    else if (BreakDownTickectDet.prodStatus == true)
                    {

                        UserName = Convert.ToString(Session["maintUser"]);
                        Password = Convert.ToString(Session["maintpwd"]);
                        OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
                        if (OperatorLoginDet != null)
                        {
                            Result = "ProdAccept";
                            ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                            MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                            int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                            int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                            MacintanceAccpobj.Reason = GetReson(ReasonID);
                            MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                            MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                            MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                            MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
                            MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.ProdFinished);
                            MacintanceAccpobj.result = Result;
                            Result = JsonConvert.SerializeObject(MacintanceAccpobj);
                        }
                        else
                        {
                            Result = "ProdLogin";
                        }
                    }
                    else
                    {
                        var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                        if (BreakdownticketDet.MaintFinished == 1)
                        {
                            Result = "ProdLogin";
                        }
                        else
                        {
                            Result = "Login";
                        }
                    }
                }
                else if (BreakDownTickectDet.prodStatus == true)
                {

                    string UserName = Convert.ToString(Session["maintUser"]);
                    string Password = Convert.ToString(Session["maintpwd"]);
                    var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
                    if (OperatorLoginDet != null)
                    {
                        Result = "ProdAccept";
                        ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                        MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                        var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                        int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                        int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                        MacintanceAccpobj.Reason = GetReson(ReasonID);
                        MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                        MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                        MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                        MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
                        MacintanceAccpobj.finish = Convert.ToString(BreakdownticketDet.ProdFinished);
                        MacintanceAccpobj.result = Result;
                        Result = JsonConvert.SerializeObject(MacintanceAccpobj);
                    }
                    else
                    {
                        Result = "Login";
                    }
                }

            }

            else
            {
                var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel == 1).ToList();
                Result = JsonConvert.SerializeObject(BreakdownDet);
            }
            return Result;
        }

        public string BreakDownDetLeve1(int Level1)
        {
            string Result = "";
            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel1ID == Level1).ToList();
            Result = JsonConvert.SerializeObject(BreakdownDet);
            return Result;
        }

        public string BreakDownDetLeve2(int Level2)
        {
            string Result = "";
            var BreakdownDet = db.tblBreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakdownLevel1ID == Level2).ToList();
            Result = JsonConvert.SerializeObject(BreakdownDet);
            return Result;
        }

        public string BreakDownReasonStore(int BreakDownID, int id)
        {
            int Machineid = id;
            string Result = "";
            DateTime CorrectedDate = DateTime.Now.Date;
            DateTime StartTime = DateTime.Now;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var WoDet = db.tblworkorderentries.Where(m => m.IsStarted == 1 && m.MachineID == Machineid && m.IsFinished == 0 && m.OperatorID == Operatorid && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.HMIID).FirstOrDefault();
            if (WoDet != null)
            {
                tblBreakDownTickect obj = new tblBreakDownTickect();
                obj.machineId = Machineid;
                obj.reasonId = BreakDownID;
                obj.operatorId = Operatorid;
                obj.woId = WoDet.HMIID;
                obj.bdTktDateTime = StartTime;
                obj.isDeleted = 0;

                db.tblBreakDownTickects.Add(obj);
                db.SaveChanges();
                Result = "Success";
            }
            return Result;
        }

        public string LoginCheckMaint(string UserName, string Password, int Machineid)
        {
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 9).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                BreakdownticketDet.mntOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.mntStatus = true;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string GetReson(int id)
        {
            string result = "";
            var Reasonleve = db.tblBreakdowncodes.Where(m => m.BreakdownID == id && m.IsDeleted == 0).FirstOrDefault();
            if (Reasonleve != null)
            {
                if (Reasonleve.BreakdownLevel == 3)
                {
                    result = result + Reasonleve.BreakdownCode;
                    var det = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == Reasonleve.BreakdownLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                    var det1 = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == det.BreakdownLevel1ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                }
                else if (Reasonleve.BreakdownLevel == 2)
                {
                    result = result + Reasonleve.BreakdownCode;
                    var det = db.tblBreakdowncodes.Where(m => m.BreakdownLevel1ID == Reasonleve.BreakdownLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                    result = result + det.BreakdownCode;
                }
                else
                {
                    result = result + Reasonleve.BreakdownCode;
                }
            }
            return result;
        }

        public string UpdateMaint(int Machineid)
        {
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntStatus == true).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.mntAcp_RejDateTime);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);

            }
            return Result;
        }

        public string UpdateMaintProd(int Machineid)
        {
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.prodStatus == true).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.prodAcp_RejDateTime = DateTime.Now;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.AcceptTime = Convert.ToString(BreakdownticketDet.prodAcp_RejDateTime);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);

            }
            return Result;
        }

        public string UpdateRemarks(int id, string RemartsData)
        {
            int Machineid = id;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.mntStatus == true).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.mntRemarks = RemartsData;
                BreakdownticketDet.MaintFinished = 1;
                db.SaveChanges();
                Result = "Success";
            }
            return Result;
        }

        public string UpdateRemarksProd(int id, string RemartsData)
        {
            int Machineid = id;
            string Result = "Fail";
            var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid && m.prodStatus == true).FirstOrDefault();
            if (BreakdownticketDet != null)
            {
                MacintanceAccp MacintanceAccpobj = new MacintanceAccp();
                BreakdownticketDet.prodRemarks = RemartsData;
                BreakdownticketDet.ProdFinished = 1;
                db.SaveChanges();
                Result = "Success";
            }
            return Result;
        }

        public string LoginCheckProd(string UserName, string Password, int Machineid)
        {
            string Result = "Fail";
            DateTime dt = DateTime.Now.Date;
            string Operatorid = Convert.ToString(Session["OperatorID"]);
            var OperatorLoginDet = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorUserName == UserName && m.operatorPwd == Password && m.roleId == 6).FirstOrDefault();
            if (OperatorLoginDet != null)
            {
                Session["maintUser"] = UserName;
                Session["maintpwd"] = Password;
                ProuctioneAccp MacintanceAccpobj = new ProuctioneAccp();
                var BreakdownticketDet = db.tblBreakDownTickects.Where(m => m.isDeleted == 0 && m.machineId == Machineid).FirstOrDefault();/**/
                BreakdownticketDet.mntOpId = OperatorLoginDet.operatorLoginId;
                BreakdownticketDet.prodStatus = true;
                db.SaveChanges();
                MacintanceAccpobj.MachineName = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == Machineid).Select(m => m.MachineDisplayName).FirstOrDefault();/*&& m.operatorId == Operatorid && m.correctedDate == dt*/
                int ReasonID = Convert.ToInt32(BreakdownticketDet.reasonId);
                int OperatorID = Convert.ToInt32(BreakdownticketDet.operatorId);
                MacintanceAccpobj.Reason = GetReson(ReasonID);
                MacintanceAccpobj.DateTimeDis = Convert.ToString(BreakdownticketDet.bdTktDateTime);
                MacintanceAccpobj.Operatorname = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0 && m.operatorId == OperatorID).Select(m => m.operatorName).FirstOrDefault();
                MacintanceAccpobj.MaintName = OperatorLoginDet.operatorName;
                Result = JsonConvert.SerializeObject(MacintanceAccpobj);
            }
            return Result;
        }

        public string RejectReasonDet(int Mainineid)
        {
            string result = "";
            var RejectReason = db.tblrejectreasons.Where(m => m.isDeleted == 0 && m.Machineid == Mainineid).ToList();
            if (RejectReason != null)
            {
                result = JsonConvert.SerializeObject(RejectReason);
            }
            return result;
        }

        public string clrsessionuser()
        {
            Session["maintUser"] = "";
            Session["maintpwd"] = "";
            string res = "success";
            return res;
        }


        public JsonResult CheckIdle()
        {
            GetMode GM = new GetMode();
            int Data = 0;

            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                //var toolCounter = db.tbltoollifeoperators.Where(m => m.toollifecounter == m.StandardToolLife).Where(m => m.IsCompleted == false && m.IsReset == 0 && m.IsDeleted == 0).ToList();

                bool IdleStatus = GM.CheckIdleEntry(MachineID);
                if (IdleStatus)
                    Data = 1;
                //int toolcount = toolCounter.Count();
                //if (Data == 1 && toolcount == 0)
                //{
                //    Data = 1;
                //}
                //else if (Data == 1 && toolcount > 0)
                //{
                //    Data = 1;
                //}
                //else if (Data != 1 && toolcount > 0)
                //{
                //    Data = 2;
                //}
                //else
                //{
                //    Data = 0;
                //}
                if (Data == 1)
                {
                    return Json(Data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(Data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult IDLEPopup(FormCollection form, int LossSelect = 0)
        {
            GetMode GM = new GetMode();
            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                var prvmode = db.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                }

                var prvmodeMaint = db.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }

                var tblLossCodes = db.tbllossescodes.Where(m => m.MessageType == "IDLE").ToList();
                ViewBag.lossCodes = tblLossCodes;

                if (LossSelect == 0)//first time ,show level 1
                {
                    int lossCodeID = tblLossCodes.Find(a => a.MessageType == "IDLE").LossCodeID;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 1;
                }
                else if (tblLossCodes.Where(m => m.LossCodesLevel1ID == LossSelect).ToList().Count > 0)// show level 2
                {
                    int lossCodeID = LossSelect;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 2;
                }
                else //show level 3
                {
                    int lossCodeID = LossSelect;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 3;
                }

                #region Lock the Machine
                var MacDet = db.tblmachinedetails.Find(MachineID);

                if (MacDet.MachineModelType == 1)
                {
                    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                    //AC.setmachinelock(true, (ushort)MacDet.MachineLockBit, (ushort)MacDet.MachineIdleBit, (ushort)MacDet.MachineUnlockBit);
                }

                #endregion
            }
            return View();
        }

        public ActionResult SaveIdle(int LossSelect = 0, int machineid = 0, bool flage = false, int count = 0)
        {
            //request came from level 2 and was a last node .Level 3  code will come as parameter.

            #region Update TblMode

            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofTabSystem();
            if (flage == false)
            {
                var machinedet = db.tblmachinedetails.Where(m => m.MachineID == machineid && m.IsDeleted == 0).ToList();
                foreach (var item in machinedet)
                {
                    int machineID = item.MachineID;

                    Session["MachineID"] = machineID;
                    DateTime correctedDate = DateTime.Now;
                    SRKSDemo.Server_Model.tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                    TimeSpan Start = StartTime.StartTime;
                    if (Start.Hours <= DateTime.Now.Hour)
                    {
                        correctedDate = DateTime.Now;
                    }
                    else
                    {
                        correctedDate = DateTime.Now.AddDays(-1);
                    }
                    int durationinsec = 0;
                    //var correctedDate = "2017-11-17";   // Hard coding for time being
                    string colorCode = "YELLOW";
                    //Update TblMode with the Loss Code
                    var mode = db.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
                                .OrderByDescending(m => m.ModeID).FirstOrDefault();
                    DateTime ModeStartTime = DateTime.Now;
                    if (mode != null)
                    {
                        if (item.LossFlag == 1)
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = null;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();

                            DateTime StartNow = DateTime.Now;
                            SRKSDemo.Server_Model.tblmode tm = new SRKSDemo.Server_Model.tblmode();
                            tm.MachineID = mode.MachineID;
                            tm.MacMode = mode.MacMode;
                            tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                            tm.InsertedOn = StartNow;
                            tm.CorrectedDate = correctedDate;
                            tm.IsDeleted = 0;
                            tm.StartTime = StartNow;
                            tm.ColorCode = "YELLOW";
                            tm.IsCompleted = 0;
                            tm.LossCodeID = LossSelect;
                            tm.ModeType = "IDLE";
                            tm.ModeTypeEnd = 0;
                            tm.StartIdle = 0;
                            tm.LossCodeEnteredTime = DateTime.Now;
                            tm.LossCodeEnteredBy = Convert.ToString(Session["UserID"]);
                            tm.IsInserted = 1;
                            db.tblmodes.Add(tm);
                            db.SaveChanges();
                        }
                        else
                        {

                        }
                        ModeStartTime = (DateTime)mode.StartTime;
                        durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                        mode.LossCodeID = LossSelect;
                        mode.ModeType = "IDLE";
                        mode.LossCodeEnteredTime = DateTime.Now;
                        mode.LossCodeEnteredBy = "";
                        mode.ModeTypeEnd = 1;
                        mode.IsCompleted = 1;
                        mode.StartIdle = 0;
                        mode.EndTime = DateTime.Now;
                        mode.DurationInSec = durationinsec;
                        mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                        mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                        db.Entry(mode).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {

                    }
                    #endregion

                    //#region UnLock the Machine
                    //var MacDet = _UWcontext.tblmachinedetails.Find(machineID);
                    //if (MacDet.MachineModelType == 1)
                    //{
                    //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                    //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
                    //}
                    //#endregion   
                }

                if (count > 1)
                {
                    return RedirectToAction("IDLEPopup");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                int OperatorloginID = Convert.ToInt32(Session["OUserID"]);
                var OperatorMachineDetails = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorloginID).ToList();
                foreach (var item in OperatorMachineDetails)
                {
                    int machineID = Convert.ToInt32(item.machineId);
                    var MachDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineid).FirstOrDefault();
                    Session["MachineID"] = machineID;
                    DateTime correctedDate = DateTime.Now;
                    SRKSDemo.Server_Model.tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                    TimeSpan Start = StartTime.StartTime;
                    if (Start.Hours <= DateTime.Now.Hour)
                    {
                        correctedDate = DateTime.Now;
                    }
                    else
                    {
                        correctedDate = DateTime.Now.AddDays(-1);
                    }
                    int durationinsec = 0;
                    //var correctedDate = "2017-11-17";   // Hard coding for time being
                    string colorCode = "YELLOW";
                    //Update TblMode with the Loss Code
                    var mode = db.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
                                .OrderByDescending(m => m.ModeID).FirstOrDefault();
                    DateTime ModeStartTime = DateTime.Now;
                    if (mode != null)
                    {
                        if (MachDet.LossFlag == 1)
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = LossSelect;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();

                            DateTime StartNow = DateTime.Now;
                            SRKSDemo.Server_Model.tbllivemode tm = new SRKSDemo.Server_Model.tbllivemode();
                            tm.MachineID = mode.MachineID;
                            tm.MacMode = mode.MacMode;
                            tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                            tm.InsertedOn = StartNow;
                            tm.CorrectedDate = correctedDate;
                            tm.IsDeleted = 0;
                            tm.StartTime = StartNow;
                            tm.ColorCode = "YELLOW";
                            tm.IsCompleted = 0;
                            tm.LossCodeID = LossSelect;
                            tm.ModeType = "IDLE";
                            tm.ModeTypeEnd = 0;
                            tm.StartIdle = 0;
                            tm.LossCodeEnteredTime = DateTime.Now;
                            tm.LossCodeEnteredBy = Convert.ToString(Session["UserID"]);
                            tm.IsInserted = 1;
                            db.tbllivemodes.Add(tm);
                            db.SaveChanges();
                        }
                        else
                        {
                            ModeStartTime = (DateTime)mode.StartTime;
                            durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                            mode.LossCodeID = LossSelect;
                            mode.ModeType = "IDLE";
                            mode.LossCodeEnteredTime = DateTime.Now;
                            mode.LossCodeEnteredBy = "";
                            mode.ModeTypeEnd = 1;
                            mode.IsCompleted = 1;
                            mode.StartIdle = 0;
                            mode.EndTime = DateTime.Now;
                            mode.DurationInSec = durationinsec;
                            mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                            mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                            db.Entry(mode).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                    else
                    {

                    }

                }

            }

            //return RedirectToAction("DashboardProduction");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public string ServerPing()
        {
            string Status = "Connected";
            GetMode GM = new GetMode();
            Ping ping = new Ping();
            String TabIPAddress = GM.GetIPAddressofTabSystem();
            var MachineDetails = db.tblmachinedetails.Where(m => m.TabIPAddress == TabIPAddress && m.IsDeleted == 0).FirstOrDefault();

            //try
            //{
            //    PingReply pingresult = ping.Send(MachineDetails.ServerIPAddress);
            //    if (pingresult.Status.ToString() == "Success")
            //    {
            //        Status = "Connected";
            //    }
            //}
            //catch
            //{
            //    Status = "Disconnected";
            //}
            return Status;
        }

        public JsonResult GetMachinePopup()
        {
            DateTime correcteddate = DateTime.Now.Date;
            List<IdlePopupMachine> IdelListObj = new List<IdlePopupMachine>();
            GetMode GM = new GetMode();
            int OperatorLoginID = Convert.ToInt32(Session["OUserID"]);
            var OperatorMachineDet = db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == OperatorLoginID).ToList();
            foreach (var item in OperatorMachineDet)
            {
                int MachineID = Convert.ToInt32(item.machineId);
                bool IdleStatus = GM.CheckIdleEntry(MachineID);
                if (IdleStatus)
                {
                    List<LossCode> LoosObj = new List<LossCode>();
                    var tblLossCodes = db.tbllossescodes.Where(m => m.MessageType == "IDLE" && m.IsDeleted == 0).ToList();
                    foreach (var data in tblLossCodes)
                    {
                        LossCode Lobj = new LossCode();
                        Lobj.losscodeid = data.LossCodeID;
                        Lobj.losscode = data.LossCode;
                        Lobj.losslevel = Convert.ToInt32(data.LossCodesLevel);
                        LoosObj.Add(Lobj);
                    }
                    var IdleEntry = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0 && m.StartIdle == 1 && m.ColorCode == "YELLOW").OrderByDescending(m => m.ModeID).FirstOrDefault();
                    //var optdet = db.tbloperatordashboards.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == correcteddate).ToList();
                    //int machinecount = optdet.Count();
                    //cellIdlecount = cellIdlecount + machinecount;
                    IdlePopupMachine obj = new IdlePopupMachine();
                    obj.MachineID = Convert.ToInt32(item.machineId);
                    obj.machinename = db.tblmachinedetails.Where(m => m.MachineID == item.machineId).Select(m => m.MachineDisplayName).FirstOrDefault();
                    obj.starttimeidle = Convert.ToString(IdleEntry.StartTime);
                    obj.LLoss = LoosObj;
                    IdelListObj.Add(obj);
                }
            }
            IdelListObj.OrderBy(m => m.starttimeidle);
            return Json(IdelListObj, JsonRequestBehavior.AllowGet);
        }

        public ContentResult lastNodeIdleCheck(int id, int lev)
        {
            var tblLossCodes = db.tbllossescodes.ToList();

            if (lev == 1)
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 2 && level.LossCodesLevel1ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }
                else
                {
                    return Content("false/" + id);
                }
            }
            else
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }

                return Content("false/" + id);
            }
        }

    }

}