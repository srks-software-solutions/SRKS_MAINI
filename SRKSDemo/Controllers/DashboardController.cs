
using Newtonsoft.Json;
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using static SRKSDemo.Models.TargetVsActal;
using dataPoints = SRKSDemo.Models.TargetVsActal.dataPoints;
using ViewData = SRKSDemo.Models.TargetVsActal.ViewData;

namespace SRKSDemo.Controllers
{
    public class DashboardController : Controller
    {

        unitworksccsEntities1 db = new unitworksccsEntities1();

        // GET: Dashboard
        public ActionResult Dashboard()
        {
            Session["Errors"] = "";
            return View();
        }

        private string GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            //string Corre = "2019-05-25";
            //DateTime correctedDate = Convert.ToDateTime(Corre);
            var daytimings = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            if (daytimings != null)
            {
                DateTime Start = Convert.ToDateTime(correctedDate.ToString("yyyy-MM-dd") + " " + daytimings.StartTime);


                //DateTime Start = Convert.ToDateTime(dtMode.Rows[0][0].ToString());
                if (Start <= DateTime.Now)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }
            }
            string correctedDateformat = correctedDate.ToString("yyyy-MM-dd");
            return correctedDateformat;
        }

        public ActionResult NewDashboard2911()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            return View();
        }

        public List<MachineConnectivityStatusModel> MachineConnectivityDet()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            //string correctedDate = "2019-05-24";
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            var machineDetailsList = new List<tblmachinedetail>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (var machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                var livemodeData = new List<tbllivemode>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                var PowerTime = livemodeData.Sum(m => m.DurationInSec);
                int Cuttingtime = 0, totalparts = 0;
                string ColorCode = "", MacMode = "";
                if (cuttingAndPartsData != null)
                {
                    Cuttingtime = Convert.ToInt32(cuttingAndPartsData.CuttingDuration);
                    totalparts = Convert.ToInt32(cuttingAndPartsData.TotalPartsCount);
                    ColorCode = cuttingAndPartsData.ColorCode;
                    MacMode = cuttingAndPartsData.MacMode;
                }

                Cuttingtime = GetParts_Cutting(MID, correctDate, out totalparts);
                var machinmodes = livemodeData.Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                MachineConnectivityStatusModel databind = new MachineConnectivityStatusModel();
                databind.MachineName = machineDetails.MachineDisplayName;
                databind.MachineID = machineDetails.MachineID;
                double IdleTime = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "YELLOW").ToList().Sum(m => m.DurationInSec));

                double running = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                VirtualHMI objvirtual = new VirtualHMI(machineDetails.IPAddress, machineDetails.MachineName);
                double CycleTime = 0;
                short exeprogramnum = 0;
                ushort h;
                int AxisCount = 32;
                List<string> retValList = new List<string>();
                List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
                objvirtual.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);
                string programnum = retValList[6].ToString();
                objvirtual.UTFValuesforMachine(out CycleTime, out exeprogramnum, out h);
                TimeSpan tmrunning = TimeSpan.FromSeconds(running);
                TimeSpan tmIdle = TimeSpan.FromSeconds(IdleTime);
                TimeSpan tm1 = TimeSpan.FromMinutes(CycleTime);
                TimeSpan tm2 = TimeSpan.FromSeconds(Convert.ToDouble(PowerTime));
                TimeSpan tm3 = TimeSpan.FromMinutes(Convert.ToDouble(Cuttingtime));
                databind.RunningTime = tmrunning.ToString(@"hh\:mm\:ss");
                databind.IdleTime = tmIdle.ToString(@"hh\:mm\:ss");
                databind.CycleTime = tm1.ToString(@"hh\:mm\:ss");
                //databind.ExeProgramName = programnum.ToString();
                databind.Color = ColorCode;
                databind.CurrentStatus = MacMode;
                databind.PowerOnTime = tm2.ToString(@"hh\:mm\:ss");
                databind.CuttingTime = tm3.ToString(@"hh\:mm\:ss");
                databind.PartsCount = totalparts;
                if (running == 0)
                    running = 1;
                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal(((double)Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return model;
        }

        public string GetAllMachineDetails()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            var machineDetailsList = new List<tblmachinedetail>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (var machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                var livemodeData = new List<tbllivemode>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                var PowerTime = livemodeData.Sum(m => m.DurationInSec);
                int Cuttingtime = 0, totalparts = 0;
                string ColorCode = "", MacMode = "";
                if (cuttingAndPartsData != null)
                {
                    Cuttingtime = Convert.ToInt32(cuttingAndPartsData.CuttingDuration);
                    totalparts = Convert.ToInt32(cuttingAndPartsData.TotalPartsCount);
                    ColorCode = cuttingAndPartsData.ColorCode;
                    MacMode = cuttingAndPartsData.MacMode;
                }

                Cuttingtime = GetParts_Cutting(MID, correctDate, out totalparts);
                var machinmodes = livemodeData.Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                MachineConnectivityStatusModel databind = new MachineConnectivityStatusModel();
                databind.MachineName = machineDetails.MachineDisplayName;
                databind.MachineID = machineDetails.MachineID;
                double IdleTime = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "YELLOW").ToList().Sum(m => m.DurationInSec));

                double running = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                VirtualHMI objvirtual = new VirtualHMI(machineDetails.IPAddress, machineDetails.MachineName);
                double CycleTime = 0;
                short exeprogramnum = 0;
                ushort h;
                int AxisCount = 32;
                List<string> retValList = new List<string>();
                List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
                objvirtual.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);
                //string programnum = retValList[6].ToString();
                objvirtual.UTFValuesforMachine(out CycleTime, out exeprogramnum, out h);
                TimeSpan tmrunning = TimeSpan.FromSeconds(running);
                TimeSpan tmIdle = TimeSpan.FromSeconds(IdleTime);
                TimeSpan tm1 = TimeSpan.FromMinutes(CycleTime);
                TimeSpan tm2 = TimeSpan.FromSeconds(Convert.ToDouble(PowerTime));
                TimeSpan tm3 = TimeSpan.FromMinutes(Convert.ToDouble(Cuttingtime));
                databind.RunningTime = tmrunning.ToString(@"hh\:mm\:ss");
                databind.IdleTime = tmIdle.ToString(@"hh\:mm\:ss");
                databind.CycleTime = tm1.ToString(@"hh\:mm\:ss");
                //databind.ExeProgramName = programnum.ToString();
                databind.Color = ColorCode;
                databind.CurrentStatus = MacMode;
                databind.PowerOnTime = tm2.ToString(@"hh\:mm\:ss");
                databind.CuttingTime = tm3.ToString(@"hh\:mm\:ss");
                databind.PartsCount = totalparts;
                if (running == 0)
                    running = 1;
                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal(((double)Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return res;
        }

        //public string MachineDashboard()
        //{

        //    string correctedDate = GetCorrectedDate();  // get CorrectedDate
        //    string res = "";                      // string correctedDate = "2018-08-23";

        //    DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
        //    using (unitworksccsEntities1 db = new unitworksccsEntities1())
        //    {

        //        int c = 0;
        //        List<GetMachines> AllMachinesList = new List<GetMachines>();
        //        List<MachineConnectivityStatusModel> machineModel = new List<MachineConnectivityStatusModel>();

        //        var machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
        //        foreach (var row in machdet)
        //        {
        //            MachineConnectivityStatusModel machinedet = new MachineConnectivityStatusModel();
        //            GetMachines machinesdata = new GetMachines();

        //            List<Machines> machineList = new List<Machines>();
        //            machinesdata.cellName = row.tblcell.CelldisplayName;
        //            machinesdata.plantName = row.tblplant.PlantDisplayName;
        //            machinesdata.shopName = row.tblshop.Shopdisplayname;

        //            //Previous
        //            //machinedet.cellName = row.CellName;
        //            //machinedet.plantName = row.tblplant.PlantDisplayName;
        //            //machinedet.shopName = row.tblshop.Shopdisplayname;

        //            var machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
        //            foreach (var machine in machineslist)
        //            {
        //                Machines machines = new Machines();
        //            int machineID = row.MachineID;
        //            var modetails = db.tbllivemodes.Where(m => m.MachineID == machineID && m.IsDeleted == 0 && m.IsCompleted == 0 && m.CorrectedDate == correctedDate1.Date).OrderByDescending(m => m.ModeID).FirstOrDefault();
        //            if (modetails != null)
        //            {
        //                machines.Color = modetails.ColorCode;
        //                machines.CurrentStatus = modetails.MacMode;
        //            }
        //            machines.MachineName = row.MachineDisplayName;
        //            machines.MachineID = machineID;
        //            machines.Time = DateTime.Now.ToShortTimeString();
        //            machineList.Add(machines);

        //            }
        //            // machinedet.machines = machineList;
        //            machinesdata.machines = machineList;
        //            if (c == 0)
        //            {
        //                machineModel = MachineConnectivityDet();
        //                machinesdata.machineModel = machineModel;
        //                c = c + 1;
        //            }



        //            AllMachinesList.Add(machinesdata);
        //            //machineModel.Add(machinedet);
        //        }
        //        AllMachinesList = AllMachinesList.OrderBy(m => m.cellName).ToList();
        //        res = JsonConvert.SerializeObject(AllMachinesList);
        //    }
        //    return res;
        //}

        public string MachineDashboard()
        {

            string correctedDate = GetCorrectedDate();  // get CorrectedDate
            string res = "";                      // string correctedDate = "2018-08-23";

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {

                int c = 0;
                List<GetMachines> AllMachinesList = new List<GetMachines>();
                List<MachineConnectivityStatusModel> machineModel = new List<MachineConnectivityStatusModel>();

                var celldet = db.tblcells.Where(m => m.IsDeleted == 0).OrderBy(m => m.CellID).ToList();
                foreach (var row in celldet)
                {
                    MachineConnectivityStatusModel machinedet = new MachineConnectivityStatusModel();
                    GetMachines machinesdata = new GetMachines();

                    List<Machines> machineList = new List<Machines>();
                    machinesdata.cellName = row.CelldisplayName;
                    machinesdata.plantName = row.tblplant.PlantDisplayName;
                    machinesdata.shopName = row.tblshop.Shopdisplayname;

                    //Previous
                    //machinedet.cellName = row.CellName;
                    //machinedet.plantName = row.tblplant.PlantDisplayName;
                    //machinedet.shopName = row.tblshop.Shopdisplayname;

                    var machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
                    foreach (var machine in machineslist)
                    {
                        Machines machines = new Machines();
                        int machineID = machine.MachineID;
                        var modetails = db.tbllivemodes.Where(m => m.MachineID == machineID && m.IsDeleted == 0 && m.IsCompleted == 0 && m.CorrectedDate == correctedDate1.Date).OrderByDescending(m => m.ModeID).FirstOrDefault();
                        if (modetails != null)
                        {
                            machines.Color = modetails.ColorCode;
                            machines.CurrentStatus = modetails.MacMode;
                        }
                        machines.MachineName = machine.MachineName;
                        machines.MachineID = machine.MachineID;
                        machines.Time = DateTime.Now.ToShortTimeString();
                        machineList.Add(machines);

                    }
                    ///  machinedet.machines = machineList;
                    machinesdata.machines = machineList;
                    if (c == 0)
                    {
                        machineModel = MachineConnectivityDet();
                        machinesdata.machineModel = machineModel;
                        c = c + 1;
                    }



                    AllMachinesList.Add(machinesdata);
                    //machineModel.Add(machinedet);
                }
                AllMachinesList = AllMachinesList.OrderBy(m => m.cellName).ToList();
                res = JsonConvert.SerializeObject(AllMachinesList);
            }
            return res;
        }

        public ActionResult MConnectivity2()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
            string correctdate = correctedDate;
            DateTime correctedDate1 = Convert.ToDateTime(correctdate);

            return View();
        }

        public List<MachineUtilizationModel> MachineUtilization()
        {
            List<MachineUtilizationModel> machineUtilizationList = new List<MachineUtilizationModel>();
            var machinedetails = new List<tblmachinedetail>();
            //var celldet = new List<tblcell>();
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            //string correctedDate = "2017-11-18";
            string correctedDate = GetCorrectedDate();
            DateTime correctdate = Convert.ToDateTime(correctedDate);
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                //celldet = db.tblcells.Where(m => m.IsDeleted == 0).ToList();
                machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            if (machinedetails != null)
            {
                machinedetails = machinedetails.OrderBy(m => m.MachineID).ToList();
                foreach (var machine in machinedetails)
                {
                    MachineUtilizationModel mum = new MachineUtilizationModel();
                    int machineID = machine.MachineID;
                    string machineName = machine.MachineDisplayName;
                    //var cellName = celldet.Where(m => m.CellID == machine.CellID).FirstOrDefault();
                    var tblmode = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 0 && m.MachineID == machineID && m.CorrectedDate == correctdate).FirstOrDefault();
                    var machinmodes = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctdate && m.IsDeleted == 0).Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                    double RunningTimeinsec = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                    var StartTime = db.tbldaytimings.Where(d => d.IsDeleted == 0).Select(d => d.StartTime).FirstOrDefault();
                    TimeSpan correctedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    TimeSpan TotalTimeTaken = correctedTime - StartTime;
                    int PartsCount = 0;
                    //GetParts_Cutting(machine.MachineID, correctdate, out PartsCount);
                    //double LoadtimewithProd = (double)(machine.StdLoadingTime + machine.StdUnLoadingTime) * PartsCount;
                    double plannedBrkDurationinMin = 0;
                    var plannedbrks = db.tblplannedbreaks.Where(m => m.IsDeleted == 0).ToList();
                    foreach (var row in plannedbrks)
                    {
                        plannedBrkDurationinMin += Convert.ToDateTime(correctdate.Date.ToString("yyyy-MM-dd") + " " + row.EndTime).Subtract(Convert.ToDateTime(correctdate.Date.ToString("yyyy-MM-dd") + " " + row.StartTime)).TotalSeconds;
                    }
                    mum.CellName = machine.MachineDisplayName;
                    double totaltimetaken = Convert.ToDouble(TotalTimeTaken.TotalSeconds);
                    double Availability = totaltimetaken - plannedBrkDurationinMin;
                    double MachineUtilization = ((RunningTimeinsec) / Availability) * 100;
                    if (MachineUtilization > 100)
                        MachineUtilization = 100;
                    MachineUtilization = Math.Round((Double)MachineUtilization, 2);
                    mum.MachineName = machine.MachineDisplayName;
                    mum.MachineUtiization = MachineUtilization;
                    mum.CurrentTime = correctedTime;
                    machineUtilizationList.Add(mum);
                }
            }

            return machineUtilizationList;
        }

        public string GetMachineUtilization()
        {
            string res = "";
            ViewModel model = new ViewModel();
            model.MachineUtilizationModels = MachineUtilization();
            //Alarms();
            model.AlarmLists = GetAlarms();
            res = JsonConvert.SerializeObject(model);
            return res;

        }

        public string OEEs()
        {
            List<OEEModel> model = new List<OEEModel>();
            string result = "";
            string OEEOP = "";
            string cellName = "";
            string[] backgroundcolr;
            string[] borderColor;
            //string correctedDate = "2017-11-18";
            string correctedDate = GetCorrectedDate();
            DateTime correctdate = Convert.ToDateTime(correctedDate);

            var machineDetails = new List<tblmachinedetail>();
            var shopdet = new List<tblshop>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machineDetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

                //shopdet = db.tblshops.Where(m => m.IsDeleted == 0).ToList();
            }
            int count = 0;
            if (machineDetails.Count > 0)
            {
                foreach (var machine in machineDetails)
                {

                    Color color = GetRandomColour();
                    string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
                    count = count + 1;
                    borderColor = new string[] { val, val, val, val };
                    backgroundcolr = new string[] { val, val, val, val };
                    //var shop = shopdet.Where(m => m.ShopID == cell.ShopID).FirstOrDefault();

                    var machineId = machine.MachineID;
                    //cellName = shop.Shopdisplayname + " - " + cell.CelldisplayName;
                    cellName = machine.MachineDisplayName;
                    double AvailabilityPercentage = 0;
                    double PerformancePercentage = 0;
                    double QualityPercentage = 0;
                    double OEEPercentage = 0;
                    int Actual = 0;
                    int Target = 0;

                    OEE(machineId, out AvailabilityPercentage, out PerformancePercentage, out QualityPercentage, out OEEPercentage, out Actual, out Target); // GET OEE

                    OEEModel OEEListData = new OEEModel();
                    OEEListData.CellName = cellName;
                    OEEListData.CellID = machineId;
                    OEEListData.Target = Target;
                    OEEListData.Actual = Actual;
                    double[] objdata = new double[] { AvailabilityPercentage, PerformancePercentage, QualityPercentage, OEEPercentage };

                    OEEListData.backgroundColor = backgroundcolr;
                    OEEListData.borderColor = borderColor;
                    OEEListData.data = objdata;
                    model.Add(OEEListData);

                }
                OEEOP = JsonConvert.SerializeObject(model);
                result = OEEOP;
            }
            return result;
        }

        private static readonly Random rand = new Random();

        private Color GetRandomColour()
        {
            return Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }

        public void OEE(int machineId, out double AvailabilityPercentage, out double PerformancePercentage, out double QualityPercentage, out double OEEPercentage, out int Actual, out int Target)
        {
            string correctdate = GetCorrectedDate();
            DateTime correctedDate1 = Convert.ToDateTime(correctdate);
            DateTime correctedDate = Convert.ToDateTime(correctdate);
            decimal OperatingTime = 0;
            decimal LossTime = 0;
            decimal MinorLossTime = 0;
            decimal MntTime = 0;
            decimal SetupTime = 0;
            Actual = 0;
            Target = 0;
            //decimal SetupMinorTime = 0;
            decimal PowerOffTime = 0;
            decimal PowerONTime = 0;
            //decimal Utilization = 0;
            decimal DayOEEPercent = 0;
            //int PerformanceFactor = 0;
            //decimal Quality = 0;
            int TotlaQty = 0;
            int YieldQty = 0;
            int BottleNeckYieldQty = 0;
            //decimal IdealCycleTimeVal = 2;
            decimal plannedCycleTime = 0;
            decimal LoadingTime = 0;
            decimal UnloadingTime = 0;

            double plannedBrkDurationinMin = 0;
            decimal LoadingUnloadingWithProd = 0;
            decimal LoadingUnloadingwithProdBottleNeck = 0;
            int minorstoppage = 0;
            //decimal TotalProductoin = 0;
            decimal Availability;
            int rejQty = 0;
            int reject = 0;
            //  string plantName = row.tblplant.PlantName;
            var machineslist = new List<tblmachinedetail>();
            var bottleneckmachines = new tblbottelneck();
            var scrap = new tblworkorderentry();
            var scrapqty1 = new List<tblrejectqty>();
            var cellpartDet = new tblcellpart();
            var partsDet = new tblpart();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                scrap = db.tblworkorderentries.Where(m => m.MachineID == machineId && m.CorrectedDate == correctedDate1).OrderByDescending(m => m.HMIID).FirstOrDefault();  //workorder entry
                if (scrap != null)
                {
                    partsDet = db.tblparts.Where(m => m.IsDeleted == 0 && m.FGCode == scrap.PartNo).FirstOrDefault();
                    //if (partsDet != null)
                    //    bottleneckmachines = db.tblbottelnecks.Where(m => m.PartNo == partsDet.FGCode && m.MachineID == scrap.MachineID).FirstOrDefault();
                }
                else
                {
                    partsDet = db.tblparts.Where(m => m.IsDeleted == 0 ).FirstOrDefault();
                    //cellpartDet = db.tblcellparts.Where(m => m.CellID == CellID && m.IsDefault == 1 && m.IsDeleted == 0).FirstOrDefault();
                    //if (cellpartDet != null)
                    //var CellID = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).Select(m => m.CellID).FirstOrDefault();
                    //cellpartDet = db.tblcellparts.Where(m => m.CellID == CellID && m.IsDefault == 1 && m.IsDeleted == 0).FirstOrDefault();
                    //if (cellpartDet != null)
                    //    bottleneckmachines = db.tblbottelnecks.Where(m => m.PartNo == cellpartDet.partNo && m.MachineID == machineId).FirstOrDefault();
                    //string Operationnum = bottleneckmachines.tblmachinedetail.OperationNumber.ToString();
                    //partsDet = db.tblparts.Where(m => m.IsDeleted == 0 && m.FGCode == cellpartDet.partNo && m.OperationNo == Operationnum).FirstOrDefault();
                }


            }
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                // Get Machines               
                machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineId /* m.MachineID == bottleneckmachines.MachineID*/).OrderBy(m => m.MachineID).ToList();
            }

            foreach (var machine in machineslist)
            {
                Machines machines = new Machines();
                int machineID = machine.MachineID;
                // Mode details
                minorstoppage = Convert.ToInt32(machine.MachineIdleMin) * 60; // in sec
                var GetModeDurations = new List<tbllivemode>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    GetModeDurations = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1).ToList();
                }

                OperatingTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "PROD").ToList().Sum(m => m.DurationInSec));
                PowerOffTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "POWEROFF").ToList().Sum(m => m.DurationInSec));
                MntTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "MNT").ToList().Sum(m => m.DurationInSec));
                MinorLossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec < minorstoppage).ToList().Sum(m => m.DurationInSec));
                LossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec > minorstoppage).ToList().Sum(m => m.DurationInSec));
                PowerONTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "POWERON").ToList().Sum(m => m.DurationInSec));
                OperatingTime = Math.Round((OperatingTime / 60), 2);
                PowerOffTime = (PowerOffTime / 60);
                MntTime = (MntTime / 60);
                MinorLossTime = (MinorLossTime / 60);
                LossTime = (LossTime / 60);
                PowerONTime = (PowerONTime / 60);
                var plannedbrks = db.tblplannedbreaks.Where(m => m.IsDeleted == 0).ToList();
                foreach (var row in plannedbrks)
                {
                    plannedBrkDurationinMin += Convert.ToDateTime(correctedDate.Date.ToString("yyyy-MM-dd") + " " + row.EndTime).Subtract(Convert.ToDateTime(correctedDate.Date.ToString("yyyy-MM-dd") + " " + row.StartTime)).TotalMinutes;
                }
                foreach (var ModeRow in GetModeDurations)
                {
                    if (ModeRow.ModeType == "SETUP")
                    {
                        try
                        {
                            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
                            //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                        }
                        catch { }
                    }
                }
                var GetModeDurationsRunning = new List<tbllivemode>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    GetModeDurationsRunning = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 0).ToList();
                }
                foreach (var ModeRow in GetModeDurationsRunning)
                {
                    String ColorCode = ModeRow.ColorCode;
                    DateTime StartTime = (DateTime)ModeRow.StartTime;
                    decimal Duration = (decimal)System.DateTime.Now.Subtract(StartTime).TotalMinutes;
                    if (ColorCode == "YELLOW")
                    {
                        LossTime += Duration;
                    }
                    else if (ColorCode == "GREEN")
                    {
                        OperatingTime += Duration;
                    }
                    else if (ColorCode == "RED")
                    {
                        MntTime += Duration;
                    }
                    else if (ColorCode == "BLUE")
                    {
                        PowerOffTime += Duration;
                    }
                }
                LoadingTime += (decimal)partsDet.StdLoadingTime;
                UnloadingTime += (decimal)partsDet.StdUnLoadingTime;

                //using (unitworksccsEntities1 db = new unitworksccsEntities1())
                //{
                //    scrap = db.tblworkorderentries.Where(m => m.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1).FirstOrDefault();
                //    string operationnum =Convert.ToString( machine.OperationNumber);
                //    partsDet = db.tblparts.Where(m => m.IsDeleted == 0 && m.FGCode == bottleneckmachines.PartNo && m.OperationNo == operationnum).FirstOrDefault();
                //}
                if (scrap != null)
                {
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        scrapqty1 = db.tblrejectqties.Where(m => m.WOID == scrap.HMIID && m.CorrectedDate == correctdate).ToList();
                    }

                    foreach (var r1 in scrapqty1)
                    {
                        reject = reject + Convert.ToInt32(r1.RejectQty);
                    }

                }
                plannedCycleTime = Convert.ToDecimal(partsDet.IdealCycleTime);
            }
            //int bottleneckMachineID = bottleneckmachines.MachineID;
            int bottleneckMachineID = 0;
            TotlaQty = GetQuantiy(machineId, correctedDate, out YieldQty, out BottleNeckYieldQty, bottleneckMachineID);
            Actual = YieldQty;
            if (YieldQty == 0)
                YieldQty = 1;
            LoadingUnloadingWithProd = ((LoadingTime + UnloadingTime) * YieldQty) / 60;
            LoadingUnloadingwithProdBottleNeck = ((LoadingTime + UnloadingTime) * BottleNeckYieldQty) / 60;
            MinorLossTime = MinorLossTime - LoadingUnloadingWithProd;
            decimal OPwithMinorStoppage = (OperatingTime + LoadingUnloadingWithProd + MinorLossTime);
            decimal utilFactor = Math.Round((LoadingUnloadingWithProd + OperatingTime), 2);
            decimal IdleTime = LossTime;
            decimal BDTime = MntTime;
            int TotalTime = Convert.ToInt32(PowerONTime) + Convert.ToInt32(OperatingTime) + Convert.ToInt32(IdleTime) + Convert.ToInt32(BDTime) + Convert.ToInt32(PowerOffTime);
            //int TotalTime = 24 * 60;

            if (TotalTime == 0)
            {
                TotalTime = 1;
            }
            if (TotlaQty == 0)
                TotlaQty = 1;
            decimal plannedCycleTimeInMin = Math.Round((plannedCycleTime / 60), 2);
            var StdCycleTimeinMin = Convert.ToDecimal(plannedCycleTimeInMin);
            var LoadunloadTimeinMin = ((int)LoadingTime + (int)UnloadingTime) / 60;
            if (StdCycleTimeinMin < 1)
                StdCycleTimeinMin = 1;
            var Targetdec = ((decimal)TotalTime / (StdCycleTimeinMin + LoadunloadTimeinMin));
            Target = Convert.ToInt32(Targetdec);
            if (TotalTime > (int)plannedBrkDurationinMin)
                Availability = Math.Round((TotalTime - (decimal)plannedBrkDurationinMin), 2);
            else
                Availability = TotalTime;
            if (OPwithMinorStoppage == 0)
                OPwithMinorStoppage = 1;
            decimal TotalTimeWithPlannedBrk = Availability;
            decimal AvailabilityPercent = Math.Round((OPwithMinorStoppage / TotalTimeWithPlannedBrk), 2) * 100;  // From BottleNeckMachine
            if (AvailabilityPercent > 100)
                AvailabilityPercent = 100;
            decimal PerformanceBottelNeck = Math.Round(((plannedCycleTimeInMin * YieldQty) / OPwithMinorStoppage), 2) * 100;
            decimal performanceFactor = (plannedCycleTime * YieldQty);
            decimal QualityLastMachine = Math.Round((decimal)((YieldQty - reject) / YieldQty), 2) * 100;            // From LastMachine
            DayOEEPercent = (decimal)Math.Round((double)(AvailabilityPercent / 100) * (double)(PerformanceBottelNeck / 100) * (double)(QualityLastMachine / 100), 2) * 100;
            //decimal availabilityDenominator = Math.Round((plannedCycleTimeInMin + LoadingUnloadingWithProd), 2);

            //TotalProductoin = Math.Round((Availability / availabilityDenominator) * 100, 2);
            //decimal performance = Math.Round((utilFactor / TotalProductoin) * 100, 2);
            //decimal performanceFactor = Math.Round((utilFactor));

            //decimal quality = Math.Round((decimal)(YieldQty / (YieldQty + rejQty)) * 100, 2);

            //Utilization = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(utilFactor) / Convert.ToDouble(TotalTime)) * 100));

            //DayOEEPercent = (decimal)Math.Round((double)(Utilization / 100) * (double)(performance / 100) * (double)(quality / 100), 2) * 100;
            if (AvailabilityPercent == 0)
            {
                QualityLastMachine = 0;
                PerformanceBottelNeck = 0;
                DayOEEPercent = 0;
            }
            AvailabilityPercentage = (double)AvailabilityPercent;
            QualityPercentage = (double)QualityLastMachine;
            PerformancePercentage = (double)PerformanceBottelNeck;
            OEEPercentage = (double)DayOEEPercent;
        }

        private int GetQuantiy(int machineId, DateTime CorrectedDate, out int YieldQty, out int BottleNeckYieldQty, int bottlneckMachineID/*, out int BottleNeckTotalQty*/)
        {
            int TotalQty = 0;
            var machineDet = new List<tblmachinedetail>();
            var starttime = new tbldaytiming();
            var parametermasterlistAll = new List<parameters_master>();
            var parametermasterlist = new List<parameters_master>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineId).ToList();
            }
            YieldQty = 0;
            //BottleNeckTotalQty = 0;
            BottleNeckYieldQty = 0;
            string Correcteddate = CorrectedDate.ToString("yyyy-MM-dd");
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            var bottleneckmachine = machineDet.Where(m => m.IsBottelNeck == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            var lastmachine = machineDet.Where(m => m.IsLastMachine == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            var firtstmachine = machineDet.Where(m => m.IsFirstMachine == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            int firstmachineId = 0;
            int lstmachineId = 0;
            int bottleneckMachineID = 0;
            if (firtstmachine != null)
                firstmachineId = firtstmachine.MachineID;
            if (lastmachine != null)
                lstmachineId = lastmachine.MachineID;
            if (bottleneckmachine != null)
                bottleneckMachineID = bottleneckmachine.MachineID;
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                starttime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault(); //.Select(m => m.StartTime)
            }

            string StartTime = Correcteddate + " 06:00:00";
            string EndTime = NxtCorrecteddate + " 06:00:00";

            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);

            // Based on 1st Machine
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                parametermasterlistAll = db.parameters_master.Where(m => m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            }
            parametermasterlist = parametermasterlistAll.Where(m => m.MachineID == firstmachineId && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();


            // Based on Last Machine
            var parametermasterlistLast = parametermasterlistAll.Where(m => m.MachineID == lstmachineId && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var TopRowLast = parametermasterlistLast.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var RowLast = parametermasterlistLast.OrderBy(m => m.ParameterID).FirstOrDefault();

            // Based on Last Machine
            var parametermasterlistBottleNeck = parametermasterlistAll.Where(m => m.MachineID == bottlneckMachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var TopRowBottleNeck = parametermasterlistBottleNeck.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var RowLastBottleNeck = parametermasterlistBottleNeck.OrderBy(m => m.ParameterID).FirstOrDefault();


            if (TopRowLast != null && RowLast != null)
                YieldQty = Convert.ToInt32(TopRowLast.PartsTotal - RowLast.PartsTotal);

            if (TopRow != null && LastRow != null)
                TotalQty = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);

            if (TopRowBottleNeck != null && RowLastBottleNeck != null)
                BottleNeckYieldQty = Convert.ToInt32(TopRowBottleNeck.PartsTotal - RowLastBottleNeck.PartsTotal);
            //}
            return TotalQty;

        }

        private int GetParts_Cutting(int MachineID, DateTime CorrectedDate, out int TotalPartsCount)
        {
            int CuttingTime = 0;
            TotalPartsCount = 0;
            string Correcteddate = CorrectedDate.ToString("yyyy-MM-dd");
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            string StartTime = Correcteddate + " 07:15:00";
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);
            var parametermasterlist = new List<parameters_master>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                parametermasterlist = db.parameters_master.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            }
            var TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();
            if (TopRow != null && LastRow != null)
            {
                CuttingTime = Convert.ToInt32(TopRow.CuttingTime) - Convert.ToInt32(LastRow.CuttingTime);
                TotalPartsCount = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);
            }
            return CuttingTime;
        }

        public List<AlarmList> GetAlarms()
        {
            string res = "";
            List<AlarmList> AlarmList = new List<AlarmList>();
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            //string correctedDate = "2018-08-23";
            string correctedDate = GetCorrectedDate();

            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            var machdet = new List<tblmachinedetail>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
            }
            foreach (var row in machdet)
            {
                var machineslist = new List<tblmachinedetail>();
                var alaramhistory = new List<alarm_history_master>();
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    //machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
                }
                //foreach (var machine in machineslist)
                //{
                var machineId = row.MachineID;
                string machineName = row.MachineDisplayName;
                var alarmsdetails = db.tblpriorityalarms.Where(a => a.IsDeleted == 0).OrderBy(m => m.AlarmID).ToList();
                foreach (var alarm in alarmsdetails)
                {
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        string alarmNo = alarm.AlarmNumber.ToString();
                        alaramhistory = db.alarm_history_master.Where(m => m.AlarmNo == alarmNo && m.MachineID == machineId && m.CorrectedDate == CorrectedDate.Date).OrderByDescending(m => m.AlarmNo).ToList();
                    }
                    foreach (var alh in alaramhistory)
                    {
                        AlarmList al = new AlarmList();
                        al.MachineID = Convert.ToInt32(alh.MachineID);
                        //al.MachineName = dt.Rows[p][machineName].ToString();
                        al.AlarmNumber = alh.AlarmNo.ToString();
                        al.AlarmMessage = alh.AlarmMessage.ToString();
                        al.AxisNumber = alh.Axis_Num.ToString();
                        al.AlarmDateTime = Convert.ToDateTime(alh.AlarmDateTime);
                        AlarmList.Add(al);
                    }
                    //}
                }

            }
            res = JsonConvert.SerializeObject(AlarmList);
            //}
            return AlarmList;
        }

        public string GetAlarmsById(int cellId)
        {
            string res = "";
            List<AlarmList> AlarmList = new List<AlarmList>();
            string correctedDate = GetCorrectedDate();
            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            //var machdet = new List<tblmachinedetail>();
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            var machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellId).OrderBy(m => m.MachineID).FirstOrDefault();
            //}
            //foreach (var row in machdet)
            //{
            var machineslist = new List<tblmachinedetail>();
            var alaramhistory = new List<alarm_history_master>();

            var machineId = cellId;
            string machineName = machdet.MachineDisplayName;
            var alarmsdetails = db.tblpriorityalarms.Where(a => a.IsDeleted == 0).OrderBy(m => m.AlarmID).ToList();
            foreach (var alarm in alarmsdetails)
            {
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    string alarmNo = alarm.AlarmNumber.ToString();
                    alaramhistory = db.alarm_history_master.Where(m => m.AlarmNo == alarmNo && m.MachineID == machineId && m.CorrectedDate == CorrectedDate.Date).OrderByDescending(m => m.AlarmNo).ToList();
                }
                foreach (var alh in alaramhistory)
                {
                    AlarmList al = new AlarmList();
                    al.MachineID = Convert.ToInt32(alh.MachineID);
                    al.AlarmNumber = alh.AlarmNo.ToString();
                    al.AlarmMessage = alh.AlarmMessage.ToString();
                    al.AxisNumber = alh.Axis_Num.ToString();
                    al.AlarmDateTime = Convert.ToDateTime(alh.AlarmDateTime);
                    AlarmList.Add(al);
                }
            }

            //}
            res = JsonConvert.SerializeObject(AlarmList);
            return res;
        }

        public string TargetAcualsDet(int cellid)
        {
            string res = "";
            res = GetTarget_Actual(cellid);
            return res;
        }

        public string GetTarget_Actual(int cellid)
        {
            string res = "";
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            string[] backgroundcolr;
            string[] borderColor;

            string correctedDate = GetCorrectedDate();

            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            List<TargetActualListDet> TargetList = new List<TargetActualListDet>();
            List<TargetActualList> TargetActualList = new List<TargetActualList>();
            var machdet = new tblmachinedetail();
            var machineDet = new tblmachinedetail();
            var partDetails = new List<tblpartscountandcutting>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
            }
            int count = 0;
            if (machdet != null)
            {
                count = count + 1;
                backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(54, 162, 235, 1)", "rgba(255, 206, 87, 1)", "rgba(75, 192, 192, 1)" };
                borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };

                if (count == 1)
                {
                    backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };
                    borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };

                }
                else if (count > 1)
                {
                    backgroundcolr = new string[] { "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)" };
                    borderColor = new string[] { "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };
                }

                TargetActualListDet TAL = new TargetActualListDet();
                TAL.CellName = machineDet.MachineDisplayName;

                string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 07:00:00";
                string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                DateTime St = Convert.ToDateTime(StartTime);
                DateTime Et = Convert.ToDateTime(EndTime);
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineDet.MachineID && m.IsBottelNeck == 1).FirstOrDefault();
                }
                if (machineDet != null)
                {
                    int MachineID = machineDet.MachineID;
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
                    }
                    int[] data = new int[partDetails.Count];
                    List<int> Target = new List<int>();
                    List<int> Actual = new List<int>();
                    string[] Lables = new string[partDetails.Count];
                    for (int i = 0; i < partDetails.Count; i++)
                    {
                        Target.Add(partDetails[i].TargetQuantity);
                        Actual.Add(partDetails[i].PartCount);
                        Lables[i] = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                    }
                    TAL.backgroundColor = backgroundcolr;
                    TAL.borderColor = borderColor;
                    TAL.Timings = Lables;
                    TAL.Target = Target;
                    TAL.Actual = Actual;
                    TargetList.Add(TAL);
                }
            }
            res = JsonConvert.SerializeObject(TargetList);
            //}
            return res;
        }

        public string GetTarget_Actual_Line(int cellid)
        {
            string res = "";
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            string[] backgroundcolr;
            string[] borderColor;

            string correctedDate = GetCorrectedDate();

            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            List<TargetActualListDet> TargetList = new List<TargetActualListDet>();
            List<TargetActualList> TargetActualList = new List<TargetActualList>();
            var machdet = new tblmachinedetail();
            var machineDet = new tblmachinedetail();
            var partDetails = new List<tblpartscountandcutting>();
            List<ViewData> finalData = new List<ViewData>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
            }
            int count = 0;
            if (machdet != null)
            {
                count = count + 1;
                backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(54, 162, 235, 1)", "rgba(255, 206, 87, 1)", "rgba(75, 192, 192, 1)" };
                borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };

                if (count == 1)
                {
                    backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };
                    borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };

                }
                else if (count > 1)
                {
                    backgroundcolr = new string[] { "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)" };
                    borderColor = new string[] { "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };
                }
                ViewData obj = new ViewData();
                TargetActualListDet TAL = new TargetActualListDet();
                TAL.CellName = machdet.MachineDisplayName;
               obj.name = machdet.MachineDisplayName;
                obj.type = "line";
               obj.showInLegend = "true";
                string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 07:00:00";
                string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                DateTime St = Convert.ToDateTime(StartTime);
                DateTime Et = Convert.ToDateTime(EndTime);
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineDet.MachineID && m.IsBottelNeck == 1).FirstOrDefault();
                }
                if (machineDet != null)
                {
                    int MachineID = machineDet.MachineID;
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
                    }
                    int[] data = new int[partDetails.Count];
                    List<int> Target = new List<int>();
                    List<int> Actual = new List<int>();
                    List<dataPoints> ActualData = new List<dataPoints>();
                    List<dataPoints> TargetData = new List<dataPoints>();
                    string[] Lables = new string[partDetails.Count];
                    for (int i = 0; i < partDetails.Count; i++)
                    {
                        dataPoints obj1 = new dataPoints();

                        if (i == 0)
                        {
                            dataPoints obj2 = new dataPoints();
                            obj2.label = "Target";
                            obj2.markerColor = "red";
                            obj2.markerType = "triangle";
                            obj2.indexLabel = "Target";
                            obj2.y = partDetails[i].TargetQuantity;
                            ActualData.Add(obj2);

                        }
                        obj1.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                        obj1.y = partDetails[i].PartCount;
                        obj1.indexLabel = "Actual";
                        //obj2.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                        //obj2.y = partDetails[i].TargetQuantity;
                        ActualData.Add(obj1);
                        //TargetData.Add(obj2);
                        //Target.Add(partDetails[i].TargetQuantity);
                        Actual.Add(partDetails[i].PartCount);
                        Lables[i] = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                    }
                    obj.dataPoints = ActualData;
                    // obj.dataPointsTarget = TargetData;
                    TAL.backgroundColor = backgroundcolr;
                    TAL.borderColor = borderColor;
                    TAL.Timings = Lables;
                    TAL.Target = Target;
                    TAL.Actual = Actual;
                    TargetList.Add(TAL);
                    finalData.Add(obj);
                }
            }
            res = JsonConvert.SerializeObject(finalData);
            //}
            return res;
        }

        public string GetTarget_Actual_Data(int cellid)
        {
            string res = "";
            List<ChartDataVal> ListData = new List<ChartDataVal>();
            ChartDataVal objListData = new ChartDataVal();
            objListData.type = "column";
            objListData.name = "Target";
            objListData.showInLegend = "true";
            objListData.indexLabel = "{y}";
            List<PivotalData> objList = new List<PivotalData>();
            List<PivotalData> objListTarget = new List<PivotalData>();
            string correctedDate = GetCorrectedDate();
            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            var machdet = new tblmachinedetail();
            var machineDet = new tblmachinedetail();
            var partDetails = new List<tblpartscountandcutting>();
            List<ViewData> finalData = new List<ViewData>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
            }
            if (machdet != null)
            {
                string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 06:00:00";
                string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                DateTime St = Convert.ToDateTime(StartTime);
                DateTime Et = Convert.ToDateTime(EndTime);
                using (unitworksccsEntities1 db = new unitworksccsEntities1())
                {
                    machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machdet.MachineID/* && m.IsLastMachine == 1*/).FirstOrDefault(); //m.IsBottelNeck == 1
                }
                if (machineDet != null)
                {
                    int MachineID = machineDet.MachineID;
                    using (unitworksccsEntities1 db = new unitworksccsEntities1())
                    {
                        partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
                    }
                    for (int i = 0; i < partDetails.Count; i++)
                    {
                        PivotalData obj = new PivotalData();
                        PivotalData Tobj = new PivotalData();
                        obj.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                        obj.y = partDetails[i].TargetQuantity;
                        objList.Add(obj);
                        Tobj.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
                        Tobj.y = partDetails[i].PartCount;
                        objListTarget.Add(Tobj);
                    }
                }
            }
            objListData.dataPoints = objList;
            objListData.dataPointsTarget = objListTarget;
            ListData.Add(objListData);
            res = JsonConvert.SerializeObject(ListData);
            return res;
        }

        public string ContributingFactorLosses()
        {
            List<TopContributingFactors> contfacList = new List<TopContributingFactors>();
            string res = "";
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            List<ContributingFactors> ContributingFactorsList = new List<ContributingFactors>();
            string[] backgroundcolr;
            string[] borderColor;
            List<ContributingFactors> ContributingFactorsListDist = new List<ContributingFactors>();
            List<LossDetails> objLossDistinct = new List<LossDetails>();
            List<LossDetails> objLoss = new List<LossDetails>();

            string correctedDate = GetCorrectedDate();

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            var machdet = new List<tblmachinedetail>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
            }
            int count = 0;
            foreach (var machine in machdet)
            {
                Color color = GetRandomColour();
                string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
                count = count + 1;
                borderColor = new string[] { val, val, val, val };
                backgroundcolr = new string[] { val, val, val, val };
                var getmodes = new List<tbllivemode>();
                //using (unitworksccsEntities1 db = new unitworksccsEntities1())
                //{
                getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1 && (m.LossCodeID != null || m.BreakdownID != null)).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();
                if (getmodes.Count == 0)
                {
                    getmodes = getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => new { m.ModeID, m.StartTime }).ToList(); //&& (m.LossCodeID != null || m.BreakdownID != null)
                }
                //}
                var TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
                double TotalLossDura = Convert.ToDouble(TotalLossDuration);
                TopContributingFactors contf = new TopContributingFactors();
                foreach (var row in getmodes)
                {
                    if ((row.LossCodeID != null && row.LossCodeID != 0) || (row.BreakdownID != null && row.BreakdownID != 0))
                    {
                        LossDetails loss = new LossDetails();
                        if ((row.LossCodeID != null && row.LossCodeID != 0))
                        {
                            loss.LossID = row.LossCodeID;
                            loss.LossCodeDescription = row.tbllossescode.LossCode;

                        }
                        else if (row.BreakdownID != null && row.BreakdownID != 0)
                        {
                            loss.LossID = row.BreakdownID;
                            loss.LossCodeDescription = row.tblbreakdown.tbllossescode.LossCode.ToString();
                        }
                        loss.LossStartTime = Convert.ToDateTime(row.StartTime);
                        loss.LossEndTime = Convert.ToDateTime(row.EndTime);
                        double diff = loss.LossEndTime.Subtract(loss.LossStartTime).TotalMinutes;
                        loss.DurationinMin = diff;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                    }
                    else
                    {
                        LossDetails loss = new LossDetails();
                        loss.LossID = 0;
                        loss.LossCodeDescription = "NO CODE";
                        loss.DurationinMin = TimeSpan.FromSeconds(TotalLossDura).TotalMinutes;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                        TotalLossDuration = getmodes.Sum(m => m.DurationInSec).ToString();
                        TotalLossDura = Convert.ToDouble(TotalLossDuration);
                        break;
                    }
                }
                var idledistinct = objLoss.Where(m => m.CellID == machine.MachineID).Select(m => new { m.LossCodeDescription, m.LossID }).Distinct().ToList();
                foreach (var row2 in idledistinct)
                {
                    ContributingFactors conf = new ContributingFactors();
                    LossDetails det = new LossDetails();
                    double Totalduration = 0;
                    var lossrow = objLoss.Where(m => m.LossCodeDescription == row2.LossCodeDescription && m.CellID == machine.MachineID).OrderByDescending(m => m.DurationinMin).ToList();
                    foreach (var loss in lossrow)
                    {
                        if (row2.LossID == loss.LossID)
                        {
                            Totalduration += loss.DurationinMin;
                            det = loss;
                            conf.LossCodeDescription = det.LossCodeDescription;
                        }
                    }
                    det.DurationinMin = Totalduration;
                    Double TotalTimeTaken = TimeSpan.FromMinutes(TotalLossDura).TotalHours;
                    //double totaltimetaken = Convert.ToDouble(TotalTimeTaken);
                    double lossduratin = TimeSpan.FromMinutes(Totalduration).TotalHours;
                    // var lossPercentage = (Totalduration / TotalTimeTaken) * 100;
                    var lossPercentage = Convert.ToInt32(lossduratin);
                    if (TotalTimeTaken == 0)
                        lossPercentage = 0;
                    if (lossPercentage > 24)
                        lossPercentage = 24;
                    conf.cellid = machine.MachineID;
                    contf.CellName = /*cell.tblshop.Shopdisplayname + " - " +*/ machine.MachineDisplayName;
                    conf.LossPercent = Convert.ToDecimal(lossPercentage);
                    conf.LossDurationInHours = Convert.ToDecimal(lossduratin);
                    ContributingFactorsList.Add(conf);
                    objLossDistinct.Add(det);
                }
                var contributingdistinct = ContributingFactorsList.Where(m => m.cellid == machine.MachineID).Select(m => new { m.LossCodeDescription }).Distinct().ToList();
                foreach (var con in contributingdistinct)
                {
                    var row = ContributingFactorsList.Where(m => m.LossCodeDescription == con.LossCodeDescription && m.cellid == machine.MachineID).OrderByDescending(m => m.LossDurationInHours).FirstOrDefault();
                    if (con.LossCodeDescription == row.LossCodeDescription)
                    {
                        ContributingFactorsListDist.Add(row);
                    }

                }
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                double[] data = new double[3];
                string[] LossNames = new string[3];
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                int namecount = 0;
                namecount = ContributingFactorsListDist.Where(m => m.LossCodeDescription != null).ToList().Count;
                int j = 0;
                if (ContributingFactorsListDist.Count > 0 && ContributingFactorsListDist.Count == 3)
                {
                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {
                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                    }
                }
                else
                {
                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {

                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                        j = i + 1;
                    }
                    for (int i = j; i <= (3 - namecount); i++)
                    {
                        LossNames[i] = "";
                        data[i] = 0;
                    }
                }
                contf.backgroundColor = backgroundcolr;
                contf.borderColor = borderColor;
                contf.data = data;
                contf.LossName = LossNames;
                contf.indexLabel = "{y}";
                contfacList.Add(contf);
            }

            //}
            res = JsonConvert.SerializeObject(contfacList);
            //}
            return res;
        }

        public string ContributingFactorLossesByCell(int cellid)
        {
            List<TopContributingFactors> contfacList = new List<TopContributingFactors>();
            string res = "";
            //using (unitworksccsEntities1 db = new unitworksccsEntities1())
            //{
            List<ContributingFactors> ContributingFactorsList = new List<ContributingFactors>();
            string[] backgroundcolr;
            string[] borderColor;
            List<ContributingFactors> ContributingFactorsListDist = new List<ContributingFactors>();
            List<LossDetails> objLossDistinct = new List<LossDetails>();
            List<LossDetails> objLoss = new List<LossDetails>();
            List<ChartDataVal> ListData = new List<ChartDataVal>();
            List<PivotalData> objList = new List<PivotalData>();
            ChartDataVal objListData = new ChartDataVal();
            objListData.type = "column";

            objListData.showInLegend = "true";
            objListData.indexLabel = "{y}";
            string correctedDate = GetCorrectedDate();

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            var machdet = new List<tblmachinedetail>();
            using (unitworksccsEntities1 db = new unitworksccsEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).ToList();
            }
            int count = 0;
            foreach (var machine in machdet)
            {
                Color color = GetRandomColour();
                string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
                count = count + 1;
                borderColor = new string[] { val, val, val, val };
                backgroundcolr = new string[] { val, val, val, val };
                var getmodes = new List<tbllivemode>();
                //using (unitworksccsEntities1 db = new unitworksccsEntities1())
                //{
                getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1 && (m.LossCodeID != null || m.BreakdownID != null)).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();
                if (getmodes.Count == 0)
                {
                    getmodes = getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => new { m.ModeID, m.StartTime }).ToList(); //&& (m.LossCodeID != null || m.BreakdownID != null)
                }
                //string lc = getmodes.tbllossescode.LossCode;
                //}
                var TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
                double TotalLossDura = Convert.ToDouble(TotalLossDuration);
                TopContributingFactors contf = new TopContributingFactors();
                foreach (var row in getmodes)
                {
                    if ((row.LossCodeID != null && row.LossCodeID != 0) || (row.BreakdownID != null && row.BreakdownID != 0))
                    {
                        LossDetails loss = new LossDetails();
                        if ((row.LossCodeID != null && row.LossCodeID != 0))
                        {
                            loss.LossID = row.LossCodeID;
                            loss.LossCodeDescription = row.tbllossescode.LossCode;

                        }
                        else if (row.BreakdownID != null && row.BreakdownID != 0)
                        {
                            loss.LossID = row.BreakdownID;
                            loss.LossCodeDescription = row.tblbreakdown.tbllossescode.LossCode.ToString();
                        }
                        loss.LossStartTime = Convert.ToDateTime(row.StartTime);
                        loss.LossEndTime = Convert.ToDateTime(row.EndTime);
                        double diff = loss.LossEndTime.Subtract(loss.LossStartTime).TotalMinutes;
                        loss.DurationinMin = diff;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                    }
                    else
                    {
                        LossDetails loss = new LossDetails();
                        loss.LossID = 0;
                        loss.LossCodeDescription = "NO CODE";
                        loss.DurationinMin = TimeSpan.FromSeconds(TotalLossDura).TotalMinutes;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                        TotalLossDuration = getmodes.Sum(m => m.DurationInSec).ToString();
                        TotalLossDura = Convert.ToDouble(TotalLossDuration);
                        break;
                    }
                }
                var idledistinct = objLoss.Where(m => m.CellID == machine.MachineID).Select(m => new { m.LossCodeDescription, m.LossID }).Distinct().ToList();
                foreach (var row2 in idledistinct)
                {
                    ContributingFactors conf = new ContributingFactors();
                    LossDetails det = new LossDetails();
                    double Totalduration = 0;
                    var lossrow = objLoss.Where(m => m.LossCodeDescription == row2.LossCodeDescription && m.CellID == machine.MachineID).OrderByDescending(m => m.DurationinMin).ToList();
                    foreach (var loss in lossrow)
                    {
                        if (row2.LossID == loss.LossID)
                        {
                            Totalduration += loss.DurationinMin;
                            det = loss;
                            conf.LossCodeDescription = det.LossCodeDescription;
                        }
                    }
                    det.DurationinMin = Totalduration;
                    Double TotalTimeTaken = TimeSpan.FromMinutes(TotalLossDura).TotalHours;
                    //double totaltimetaken = Convert.ToDouble(TotalTimeTaken);
                    double lossduratin = TimeSpan.FromMinutes(Totalduration).TotalHours;
                    // var lossPercentage = (Totalduration / TotalTimeTaken) * 100;
                    var lossPercentage = Convert.ToInt32(lossduratin);
                    if (TotalTimeTaken == 0)
                        lossPercentage = 0;
                    if (lossPercentage > 24)
                        lossPercentage = 24;
                    conf.cellid = machine.MachineID;
                    contf.CellName = /*cell.tblshop.Shopdisplayname + " - " +*/ machine.MachineDisplayName;
                    conf.LossPercent = Convert.ToDecimal(lossPercentage);
                    conf.LossDurationInHours = Convert.ToDecimal(lossduratin);
                    ContributingFactorsList.Add(conf);
                    objLossDistinct.Add(det);
                }
                var contributingdistinct = ContributingFactorsList.Where(m => m.cellid == machine.MachineID).Select(m => new { m.LossCodeDescription }).Distinct().ToList();
                foreach (var con in contributingdistinct)
                {

                    var row = ContributingFactorsList.Where(m => m.LossCodeDescription == con.LossCodeDescription && m.cellid == machine.MachineID).OrderByDescending(m => m.LossDurationInHours).FirstOrDefault();
                    if (con.LossCodeDescription == row.LossCodeDescription)
                    {
                        ContributingFactorsListDist.Add(row);
                    }

                }
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                double[] data = new double[3];
                string[] LossNames = new string[3];
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                int namecount = 0;
                namecount = ContributingFactorsListDist.Where(m => m.LossCodeDescription != null).ToList().Count;
                int j = 0;
                if (ContributingFactorsListDist.Count > 0 && ContributingFactorsListDist.Count == 3)
                {
                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {
                        PivotalData obj = new PivotalData();
                        obj.label = ContributingFactorsListDist[i].LossCodeDescription;
                        obj.y = Convert.ToInt32(ContributingFactorsListDist[i].LossPercent);
                        objList.Add(obj);
                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                    }
                }
                else
                {

                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {
                        PivotalData obj = new PivotalData();
                        obj.label = ContributingFactorsListDist[i].LossCodeDescription;
                        var lossduration = Math.Round(Convert.ToDecimal(ContributingFactorsListDist[i].LossPercent), 2);
                        obj.y = Convert.ToInt32(lossduration);
                        objListData.name = ContributingFactorsListDist[i].LossCodeDescription;
                        objList.Add(obj);
                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                        j = i + 1;
                    }
                    for (int i = j; i <= (3 - namecount); i++)
                    {
                        LossNames[i] = "";
                        data[i] = 0;
                    }
                }
                contf.backgroundColor = backgroundcolr;
                contf.borderColor = borderColor;
                contf.data = data;
                contf.LossName = LossNames;
                contf.indexLabel = "{y}";
                contfacList.Add(contf);
                objListData.dataPoints = objList;

                //objListData.dataPointsTarget = objListTarget;
                ListData.Add(objListData);

            }

            res = JsonConvert.SerializeObject(ListData);
            //}
            //res = JsonConvert.SerializeObject(contfacList);
            //}
            return res;
        }
    }
}