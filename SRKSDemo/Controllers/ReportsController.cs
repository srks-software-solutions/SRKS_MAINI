using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using SRKSDemo;
using SRKSDemo.OperatorEntryModelClass;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;

namespace SRKSDemo.Controllers
{
    public class ReportsController : Controller
    {
        unitworksccsEntities1 Serverdb = new unitworksccsEntities1();


        #region V changes Utilization report for with new structer
        public ActionResult Utilization()
        {

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult Utilization(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.UtilizationReport UR = new ReportsCalcClass.UtilizationReport();

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }


            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\UtilixationReportMaini.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet Templatews1 = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilixationReportMaini" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetSum = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetSum = p.Workbook.Worksheets.Add("Summerized", Templatews1);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetSum = p.Workbook.Worksheets.Add("Summerized" + "1", Templatews1);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheetSum.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheetSum.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 4;
            int SlNo = 1;

            // day wise data display
            List<UtilSummerized> UtilSummerizedList = new List<UtilSummerized>();
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    UtilSummerized UtilSummerizedObj = new UtilSummerized();
                    string CorrDate = QueryDate.ToString("yyyy-MM-dd");
                    int MachID = Machine.MachineID;

                    var MachineDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachID).FirstOrDefault();
                    string MachineName = MachineDet.MachineDisplayName;
                    int Cellid = Convert.ToInt32(MachineDet.CellID);
                    var CellDet = Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == Cellid).FirstOrDefault();
                    string Cellname = CellDet.CelldisplayName;
                    int Shopid = CellDet.ShopID;
                    var ShopDet = Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == Shopid).FirstOrDefault();
                    string ShopName = ShopDet.Shopdisplayname;
                    int Plantid = ShopDet.PlantID;
                    string PlantName = Serverdb.tblplants.Where(m => m.IsDeleted == 0 && m.PlantID == Plantid).Select(m => m.PlantDisplayName).FirstOrDefault();

                    UtilSummerizedObj.PlantName = PlantName;
                    UtilSummerizedObj.ShopName = ShopName;
                    UtilSummerizedObj.CellName = Cellname;
                    UtilSummerizedObj.MachineName = MachineName;
                    UtilSummerizedObj.DateTime = FromDate + " TO " + ToDate;


                    worksheet.Cells["A" + StartRow].Value = SlNo++;
                    worksheet.Cells["B" + StartRow].Value = PlantName;
                    worksheet.Cells["C" + StartRow].Value = ShopName;
                    worksheet.Cells["D" + StartRow].Value = Cellname;
                    worksheet.Cells["E" + StartRow].Value = MachineName;
                    worksheet.Cells["F" + StartRow].Value = CorrDate;

                    int Col = 7;
                    var ShiftDet = Serverdb.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
                    List<ShiftValue> ShiftList = new List<ShiftValue>();
                    foreach (var ShiftRow in ShiftDet)
                    {
                        string ColumnNumber = ExcelColumnFromNumber(Col);
                        int TotalTime = Convert.ToInt32(ShiftRow.Duration);
                        int ShiftID = ShiftRow.ShiftID;
                        string ShiftName = ShiftRow.ShiftName;
                        double SumCuttingtime = 0, SumOperatingtime = 0, SumPowerOntime = 0;
                        double CuttinTimeT = 0, ModeOPTimeT = 0, ModePOTimeT = 0, FinalCuttinTimeT = 0, FinalModeOPTimeT = 0, FinalModePOTimeT = 0;
                        DateTime StartTime = Convert.ToDateTime(CorrDate + " " + ShiftRow.StartTime);
                        DateTime EndTime = Convert.ToDateTime(CorrDate + " " + ShiftRow.EndTime);
                        var ModePOTime = Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).ToList();
                        var ModeOPTime = Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.ColorCode == "GREEN" && m.CorrectedDate == QueryDate.Date).ToList();
                        var CuttingTime = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.ShiftName == ShiftName && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).ToList();
                        if (CuttingTime.Count != 0)
                        {
                            //CuttinTimeT = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.StartTime >= StartTime && m.EndTime <= EndTime && m.CorrectedDate == QueryDate.Date).Sum(m=>m.CuttingTime);
                            CuttinTimeT = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.ShiftName == ShiftName && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).Sum(m => m.CuttingTime);
                            FinalCuttinTimeT = (CuttinTimeT / TotalTime) * 100;
                            SumCuttingtime = SumCuttingtime + FinalCuttinTimeT;
                        }
                        if (ModeOPTime.Count != 0)
                        {
                            ModeOPTimeT = Convert.ToInt32(Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.ColorCode == "GREEN" && m.CorrectedDate == QueryDate.Date).Sum(m => m.DurationInSec));
                            FinalModeOPTimeT = ((ModeOPTimeT / 60) / TotalTime) * 100;
                            SumOperatingtime = SumOperatingtime + FinalModeOPTimeT;
                        }
                        if (ModePOTime.Count != 0)
                        {
                            ModePOTimeT = Convert.ToInt32(Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).Sum(m => m.DurationInSec));
                            FinalModePOTimeT = ((ModePOTimeT / 60) / TotalTime) * 100;
                            SumPowerOntime = SumPowerOntime + FinalModePOTimeT;
                        }

                        var PrecentColourDet = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
                        foreach (var ColourRow in PrecentColourDet)
                        {
                            double MinVal = Convert.ToDouble(ColourRow.Min);
                            double MaxVal = Convert.ToDouble(ColourRow.Max);
                            string Colour = ColourRow.Colour;

                            if (FinalModePOTimeT >= MinVal && FinalModePOTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalModePOTimeT, 2);
                                Col++;
                                ColumnNumber = ExcelColumnFromNumber(Col);

                            }
                            if (FinalModeOPTimeT >= MinVal && FinalModeOPTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalModeOPTimeT, 2);
                                Col++;
                                ColumnNumber = ExcelColumnFromNumber(Col);
                            }
                            if (FinalCuttinTimeT >= MinVal && FinalCuttinTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalCuttinTimeT, 2);
                            }

                        }



                        ShiftValue Obj = new ShiftValue();
                        Obj.CTTime = SumCuttingtime;
                        Obj.OPTime = SumOperatingtime;
                        Obj.POTime = SumPowerOntime;
                        ShiftList.Add(Obj);

                        Col++;
                    }



                    UtilSummerizedObj.ShiftDoubleVal = ShiftList;
                    UtilSummerizedList.Add(UtilSummerizedObj);
                    StartRow++;
                }
            }

            SlNo = 1;
            StartRow = 4;
            int Cols = 7;
            int dayDiff = dateDifference + 1;

            //summerized data display
            if (UtilSummerizedList != null)
            {
                foreach (var row in UtilSummerizedList)
                {
                    worksheetSum.Cells["A" + StartRow].Value = SlNo++;
                    worksheetSum.Cells["B" + StartRow].Value = row.PlantName;
                    worksheetSum.Cells["C" + StartRow].Value = row.ShopName;
                    worksheetSum.Cells["D" + StartRow].Value = row.CellName;
                    worksheetSum.Cells["E" + StartRow].Value = row.MachineName;
                    worksheetSum.Cells["F" + StartRow].Value = row.DateTime;

                    var ListShiftVal = row.ShiftDoubleVal;
                    foreach (var ShiftRow in ListShiftVal)
                    {
                        string ColumnNumber = ExcelColumnFromNumber(Cols);

                        var PrecentColourDet = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
                        foreach (var ColourRow in PrecentColourDet)
                        {
                            double MinVal = Convert.ToDouble(ColourRow.Min);
                            double MaxVal = Convert.ToDouble(ColourRow.Max);
                            string Colour = ColourRow.Colour;

                            if (ShiftRow.POTime >= MinVal && ShiftRow.POTime < MaxVal)
                            {
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.POTime) / dayDiff, 2);
                                Cols++;
                                ColumnNumber = ExcelColumnFromNumber(Cols);

                            }
                            if (ShiftRow.OPTime >= MinVal && ShiftRow.OPTime < MaxVal)
                            {
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.OPTime) / dayDiff, 2);
                                Cols++;
                                ColumnNumber = ExcelColumnFromNumber(Cols);
                            }
                            if (ShiftRow.CTTime >= MinVal && ShiftRow.CTTime < MaxVal)
                            {
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.CTTime) / dayDiff, 2);
                                Cols++;
                            }

                        }
                        //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.POTime,2);
                        //Cols++;
                        //ColumnNumber = ExcelColumnFromNumber(Cols);
                        //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.OPTime,2);
                        //Cols++;
                        //ColumnNumber = ExcelColumnFromNumber(Cols);
                        //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.CTTime,2);
                        //Cols++;
                    }
                    Cols = 7;
                    StartRow++;
                }
            }

            int Srow = StartRow - 1;
            // last row disaply in summerized
            worksheetSum.SelectedRange["A" + StartRow + ":F" + StartRow + ""].Merge = true;
            worksheetSum.Cells["A" + StartRow].Value = "Total";
            worksheetSum.Cells["G" + StartRow].Formula = "SUM(G4:G" + Srow + ")";
            worksheetSum.Cells["H" + StartRow].Formula = "SUM(H4:H" + Srow + ")";
            worksheetSum.Cells["I" + StartRow].Formula = "SUM(I4:I" + Srow + ")";
            worksheetSum.Cells["J" + StartRow].Formula = "SUM(J4:J" + Srow + ")";
            worksheetSum.Cells["K" + StartRow].Formula = "SUM(K4:K" + Srow + ")";
            worksheetSum.Cells["L" + StartRow].Formula = "SUM(L4:L" + Srow + ")";
            worksheetSum.Cells["M" + StartRow].Formula = "SUM(M4:M" + Srow + ")";
            worksheetSum.Cells["N" + StartRow].Formula = "SUM(N4:N" + Srow + ")";
            worksheetSum.Cells["O" + StartRow].Formula = "SUM(O4:O" + Srow + ")";

            //  precentage and colour display
            StartRow += 2;
            int cno = 3;
            var PrecentColourDet1 = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
            string ColumnNumberS = "";
            ColumnNumberS = ExcelColumnFromNumber(cno);
            worksheet.Cells[ColumnNumberS + StartRow].Value = "Values are in Precentage";
            ColumnNumberS = ExcelColumnFromNumber(cno);
            worksheetSum.Cells[ColumnNumberS + StartRow].Value = "Values are in Precentage";
            cno++;
            foreach (var ColourRow in PrecentColourDet1)
            {
                ColumnNumberS = ExcelColumnFromNumber(cno);
                double MinVal = Convert.ToDouble(ColourRow.Min);
                double MaxVal = Convert.ToDouble(ColourRow.Max);
                string Colour = ColourRow.Colour;
                worksheet.Cells[ColumnNumberS + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[ColumnNumberS + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                worksheet.Cells[ColumnNumberS + StartRow].Value = ">=" + MinVal + "%" + "<" + MaxVal + "%" + Colour;
                worksheetSum.Cells[ColumnNumberS + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheetSum.Cells[ColumnNumberS + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                worksheetSum.Cells[ColumnNumberS + StartRow].Value = ">=" + MinVal + "%" + "<" + MaxVal + "%" + Colour;
                cno++;
            }




            //p.Workbook.Worksheets.MoveToStart(3);
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "UtilizationReport" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "UtilizationReport", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);

            return View();
        }


        public System.Drawing.Color ReturnColour(string Colour)
        {
            switch (Colour)
            {
                case "Red": return Color.Red;
                case "Yellow": return Color.Yellow;
                case "Green": return Color.Green;

            }
            return Color.White;
        }


        public string GetShift()
        {
            string ShiftValue = "";
            DateTime DateNow = DateTime.Now;
            var ShiftDetails = Serverdb.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
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


        #endregion

        // GET: Reports
        public ActionResult MasterPartsReport()
        {
            return View();
        }

        public ActionResult Utilization_ABGraph()
        {

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult Utilization_ABGraph(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.UtilizationReport UR = new ReportsCalcClass.UtilizationReport();
            UR.CalculateUtilization(PlantID, ShopID, CellID, MachineID, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate));

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }


            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\UtilizationReport_ABGraph.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetGraph = null;
            ExcelWorksheet workSheetGraphData = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
                workSheetGraphData = p.Workbook.Worksheets.Add("GraphData", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
                workSheetGraphData = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "GraphData", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 3;
            int SlNo = 1;
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    var GetUtilList = Serverdb.tbl_UtilReport.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        worksheet.Cells["A" + StartRow].Value = SlNo++;
                        worksheet.Cells["B" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblmachinedetail.tblplant.PlantDisplayName;
                        worksheet.Cells["D" + StartRow].Value = MacRow.tblmachinedetail.tblshop.Shopdisplayname;
                        worksheet.Cells["E" + StartRow].Value = MacRow.tblmachinedetail.tblcell.CelldisplayName;
                        worksheet.Cells["F" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["G" + StartRow].Value = MacRow.TotalTime;
                        worksheet.Cells["H" + StartRow].Value = MacRow.OperatingTime;
                        worksheet.Cells["I" + StartRow].Value = MacRow.SetupTime;
                        worksheet.Cells["J" + StartRow].Value = (MacRow.MinorLossTime - MacRow.SetupMinorTime);
                        worksheet.Cells["K" + StartRow].Value = MacRow.LossTime;
                        worksheet.Cells["L" + StartRow].Value = MacRow.BDTime;
                        worksheet.Cells["M" + StartRow].Value = MacRow.PowerOffTime;
                        worksheet.Cells["N" + StartRow].Value = MacRow.UtilPercent + " %";
                        StartRow++;
                    }
                }
            }

            int rowCount = 2 + dateDifference;
            //getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            int oldcolumn = 2;
            int height = 20;
            foreach (var Machine1 in getMachineList)
            {
                int currentCOlumn = oldcolumn;
                string macName = Machine1.MachineDisplayName;
                int StartRow1 = 2;
                for (int i = 0; i <= dateDifference; i++)
                {
                    DateTime QueryDate1 = Convert.ToDateTime(FromDate).AddDays(i);
                    var GetUtilList = Serverdb.tbl_UtilReport.Where(m => m.MachineID == Machine1.MachineID && m.CorrectedDate == QueryDate1.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        string ColEntry = ExcelColumnFromNumber(currentCOlumn);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = QueryDate1.Date.ToString("dd-MM-yyyy");
                        ColEntry = ExcelColumnFromNumber(currentCOlumn + 1);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        ColEntry = ExcelColumnFromNumber(currentCOlumn + 2);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = MacRow.OperatingTime;
                        StartRow1++;
                    }

                }
                if (StartRow1 > 2)
                {
                    oldcolumn = currentCOlumn + 3;
                    var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("AB Graph-" + macName, eChartType.ColumnStacked);

                    chartIDAndUnID.SetSize((40 * rowCount), 350);

                    chartIDAndUnID.SetPosition(height, 20);
                    height = height + 400;

                    chartIDAndUnID.Title.Text = "Graph - " + macName;
                    chartIDAndUnID.Style = eChartStyle.Style18;
                    chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
                    //chartIDAndUnID.Legend.Remove();
                    chartIDAndUnID.YAxis.MaxValue = 24;
                    chartIDAndUnID.YAxis.MinValue = 0;
                    chartIDAndUnID.YAxis.MajorUnit = 4;

                    chartIDAndUnID.Locked = false;
                    chartIDAndUnID.PlotArea.Border.Width = 0;
                    chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartIDAndUnID.DataLabel.ShowValue = true;
                    chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    string ColEntry1 = ExcelColumnFromNumber(currentCOlumn);
                    ExcelRange dateWork = workSheetGraphData.Cells[ColEntry1 + "2:" + ColEntry1 + rowCount];
                    ColEntry1 = ExcelColumnFromNumber(currentCOlumn + 2);
                    ExcelRange hoursWork = workSheetGraphData.Cells[ColEntry1 + "2:" + ColEntry1 + rowCount];
                    workSheetGraphData.Hidden = eWorkSheetHidden.Hidden;
                    var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
                    hours.Header = "Operating Time (Hours)";
                    //Get reference to the worksheet xml for proper namespace
                    var chartXml = chartIDAndUnID.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("c", nsuri);

                    //XY Scatter plots have 2 value axis and no category
                    var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
                    if (valAxisNodes != null && valAxisNodes.Count > 0)
                        foreach (XmlNode valAxisNode in valAxisNodes)
                        {
                            var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                            if (major != null)
                                valAxisNode.RemoveChild(major);

                            var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                            if (minor != null)
                                valAxisNode.RemoveChild(minor);
                        }

                    //Other charts can have a category axis
                    var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
                    if (catAxisNodes != null && catAxisNodes.Count > 0)
                        foreach (XmlNode catAxisNode in catAxisNodes)
                        {
                            var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                            if (major != null)
                                catAxisNode.RemoveChild(major);

                            var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                            if (minor != null)
                                catAxisNode.RemoveChild(minor);
                        }
                }
            }
            p.Workbook.Worksheets.MoveToStart(3);
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "UtilizationReport_ABGraph", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);

            return View();
        }

        public void DownloadUtilReport(String FilePath, String FileName, String ToDate)
        {
            System.IO.FileInfo file1 = new System.IO.FileInfo(FilePath);
            string Outgoingfile = FileName + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx";
            if (file1.Exists)
            {
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                Response.AddHeader("Content-Length", file1.Length.ToString());
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.WriteFile(file1.FullName);
                Response.Flush();
                Response.Close();
                Response.End();
            }
        }

        public ActionResult ManMachineTicket()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult ManMachineTicket(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }

            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\ManMachineTicket.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            //ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 4;
            int SlNo = 1;

            int Startcolumn = 18;
            String ColNam = ExcelColumnFromNumber(Startcolumn);
            var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
            foreach (var LossRow in GetMainLossList)
            {
                ColNam = ExcelColumnFromNumber(Startcolumn);
                worksheet.Cells[ColNam + "3"].Value = LossRow.LossCode;
                Startcolumn++;
            }

            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        int MacStartcolumn = 18;
                        worksheet.Cells["A" + StartRow].Value = SlNo++;
                        worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
                        worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
                        worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
                        worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["H" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
                        worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.WOStart.ToString("hh:mm tt");
                        worksheet.Cells["J" + StartRow].Value = Convert.ToDateTime(MacRow.tblworkorderentry.WOEnd).ToString("hh:mm tt");
                        worksheet.Cells["K" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
                        worksheet.Cells["L" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
                        worksheet.Cells["M" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
                        worksheet.Cells["N" + StartRow].Value = MacRow.TotalSetup;
                        worksheet.Cells["O" + StartRow].Value = MacRow.TotalOperatingTime;
                        worksheet.Cells["P" + StartRow].Value = 0;
                        worksheet.Cells["Q" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
                        //var getWoLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID).ToList();

                        foreach (var LossRow in GetMainLossList)
                        {
                            var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                            String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
                            if (getWoLossList1 != null)
                                worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
                            else
                                worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
                            MacStartcolumn++;
                        }

                        //foreach (var LossRow in getWoLossList)
                        //{
                        //    int LossIndex = GetMainLossList.IndexOf(Serverdb.tbllossescodes.Find(LossRow.LossID));
                        //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn + LossIndex);
                        //    worksheet.Cells[ColEntry + "" + StartRow].Value = LossRow.LossDuration;
                        //}
                        StartRow++;
                    }
                }
            }

            //worksheet.View.ShowGridLines = false;
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "ManMachineTicket", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);
            return View();
        }

        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        public ActionResult OEEReport()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult OEEReport(int PlantID, string FromDate, string ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }

            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }

            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            else if (worksheetGraph == null)
            {
                worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            decimal TotalQualityPercent = 0, TotalOEEPercent = 0, TotalAvailPercent = 0, TotalPerformancePercent = 0;

            int StartRow = 2;
            int MachineCount = getMachineList.Count;
            int Startcolumn = 13;
            string ColNam = ExcelColumnFromNumber(Startcolumn);
            var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
            foreach (var LossRow in GetMainLossList)
            {
                ColNam = ExcelColumnFromNumber(Startcolumn);
                worksheet.Cells[ColNam + "1"].Value = LossRow.LossCode;
                Startcolumn++;
            }

            //Tabular sheet Data Population
            for (int i = 0; i <= dateDifference; i++)
            {
                int partscount = 0;

                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                var partcount = Serverdb.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                foreach (var partcountdet in partcount)
                {
                    partscount = partscount + partcountdet.PartCount;
                }
                foreach (var Machine in getMachineList)
                {
                    UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        int MacStartcolumn = 13;
                        worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
                        worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.OperationNumber;
                        //worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
                        worksheet.Cells["E" + StartRow].Value = Math.Round((MacRow.TotalOperatingTime) / 60, 2);
                        //worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
                        //worksheet.Cells["F" + StartRow].Value = MacRow.ava;
                        worksheet.Cells["D" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["F" + StartRow].Value = MacRow.UtilPercent;
                        worksheet.Cells["H" + StartRow].Value = MacRow.QualityPercent;
                        worksheet.Cells["G" + StartRow].Value = MacRow.PerformancePerCent;
                        double oee = Convert.ToDouble(((MacRow.UtilPercent / 100) * (100 / 100) * (MacRow.PerformancePerCent / 100)) * 100);
                        worksheet.Cells["I" + StartRow].Value = Math.Round(oee, 2);
                        worksheet.Cells["J" + StartRow].Value = partscount;
                        //worksheet.Cells["H" + StartRow].Value = MacRow.TotalOperatingTime;
                        //worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
                        //worksheet.Cells["J" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
                        //worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
                        int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
                        TotalQualityPercent += MacRow.QualityPercent;
                        TotalOEEPercent += (decimal)oee;
                        TotalAvailPercent += (decimal)MacRow.UtilPercent;
                        TotalPerformancePercent += MacRow.PerformancePerCent;

                        if (TotalQty == 0)
                            TotalQty = 1;
                        worksheet.Cells["K1"].Value = "Setup Time";
                        worksheet.Cells["K" + StartRow].Value = 0;
                        worksheet.Cells["L1"].Value = "Rejections";
                        worksheet.Cells["L" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
                        long MacTotalLoss = 0;
                        foreach (var LossRow in GetMainLossList)
                        {
                            var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                            String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
                            if (getWoLossList1 != null)
                            {
                                worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
                                MacTotalLoss += getWoLossList1.LossDuration;
                            }
                            else
                                worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
                            MacStartcolumn++;
                        }
                        string ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
                        MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
                        //MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload (in hr)";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = Math.Round((MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss) / 60, 2);
                        MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
                        ////if (MacRow.tblworkorderentry.ShiftID == 1)
                        ////    worksheet.Cells[ColEntry1 + StartRow].Value = "First Shift";
                        ////else
                        ////    worksheet.Cells[ColEntry1 + StartRow].Value = "Second Shift";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
                        //MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
                        MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
                        //MacStartcolumn++;

                        //decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
                        //MacStartcolumn++;
                        StartRow++;
                    }
                }
            }
            StartRow = 2;

            DataTable LossTbl = new DataTable();
            LossTbl.Columns.Add("LossID", typeof(int));
            LossTbl.Columns.Add("LossDuration", typeof(int));
            LossTbl.Columns.Add("LossTarget", typeof(string));
            LossTbl.Columns.Add("LossName", typeof(string));
            LossTbl.Columns.Add("LossActual", typeof(string));

            //Graph Sheet Population
            //Start Date and End Date
            worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
            worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
            int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
            int WorkingDays = dateDifference - GetHolidays + 1;
            //Working Days
            worksheetGraph.Cells["E5"].Value = WorkingDays;
            //Planned Production Time
            worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
            double TotalOperatingTime = 0;
            double TotalDownTime = 0;
            double TotalAcceptedQty = 0;
            double TotalRejectedQty = 0;
            double TotalPerformanceFactor = 0;
            int StartGrpah1 = 48;
            for (int i = 0; i <= dateDifference; i++)
            {
                double DayOperatingTime = 0;
                double DayDownTime = 0;
                double DayAcceptedQty = 0;
                double DayRejectedQty = 0;
                double DayPerformanceFactor = 0;
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);

                var plantName = Serverdb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
                worksheetGraph.Cells["C3"].Value = plantName;
                foreach (var MachRow in getMachineList)
                {
                    if (MachineID == 0)
                    {
                        worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
                        worksheetGraph.Cells["C5"].Value = "AS DIVISION";
                    }
                    else
                    {
                        worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
                        worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
                    }
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var ProdRow in GetUtilList)
                    {
                        //Total Values
                        TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
                        TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
                        TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        TotalPerformanceFactor += ProdRow.PerfromaceFactor;
                        //Day Values
                        DayOperatingTime += (double)ProdRow.TotalOperatingTime;
                        DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
                        DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        DayPerformanceFactor += ProdRow.PerfromaceFactor;
                    }
                    var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

                    foreach (var LossRow in GetLossList)
                    {
                        var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
                        if (getrow == null)
                        {
                            var GetLossTargetPercent = "1%";
                            String GetLossName = null;
                            var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
                            if (GetLossTarget != null)
                            {
                                GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
                                GetLossName = GetLossTarget.LossCode;
                            }

                            LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
                        }
                        else
                        {
                            foreach (DataRow GetRow in LossTbl.Rows)
                            {
                                if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
                                {
                                    long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
                                    LossDura += LossRow.LossDuration;
                                    GetRow["LossDuration"] = LossDura;
                                }
                            }

                        }
                    }
                }
                int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
                if (TotQty == 0)
                    TotQty = 1;

                double DayOpTime = DayOperatingTime;
                if (DayOpTime == 0)
                    DayOpTime = 1;

                decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
                decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
                decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
                decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

                worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
                worksheetGraph.Cells["C" + StartGrpah1].Value = 0.88;
                worksheetGraph.Cells["D" + StartGrpah1].Value = (DayOEEPercent / 100) / 100;

                StartGrpah1++;
            }
            worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
            worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
            worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
            worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

            decimal TotalQualityPercent1 = 0, TotalOEEPercent1 = 0, TotalAvailPercent1 = 0, TotalPerformancePercent1 = 0;

            if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
            {
                TotalAvailPercent1 = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
                TotalPerformancePercent1 = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
                TotalQualityPercent1 = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
                TotalOEEPercent1 = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);
            }


            if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
            {
                worksheetGraph.Cells["E20"].Value = TotalAvailPercent1;
                worksheetGraph.Cells["E21"].Value = TotalPerformancePercent1;
                worksheetGraph.Cells["E22"].Value = TotalQualityPercent1;
                worksheetGraph.Cells["E23"].Value = TotalOEEPercent1;
                worksheetGraph.Cells["G5"].Value = TotalOEEPercent1;
                worksheetGraph.View.ShowGridLines = false;
            }
            else
            {
                int diff = dateDifference + 1;
                worksheetGraph.Cells["E20"].Value = (TotalAvailPercent / 100) / diff;
                worksheetGraph.Cells["E21"].Value = (TotalPerformancePercent / 100) / diff;
                worksheetGraph.Cells["E22"].Value = (TotalQualityPercent / 100) / diff;
                worksheetGraph.Cells["E23"].Value = (TotalOEEPercent / 100) / diff;
                worksheetGraph.Cells["G5"].Value = (TotalOEEPercent / 100) / diff;
                worksheetGraph.View.ShowGridLines = false;
            }


            DateTime fromDate = Convert.ToDateTime(FromDate);
            DateTime toDate = Convert.ToDateTime(ToDate);
            var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
                                           where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
                                           group dbItem by dbItem.LossID into x
                                           select new
                                           {
                                               LossId = x.Key,
                                               LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
                                           }).ToList();
            var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
            int lossXccelNo = 29;
            foreach (var GetRow in item)
            {
                string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();
                decimal lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
                decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
                worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
                worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
                worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;

                lossXccelNo++;
            }

            int grphData = 5;
            decimal CumulativePercentage = 0;
            foreach (var data in top3ContrubutingFactors)
            {
                var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
                string lossCode = dbLoss.LossCode;
                decimal Target = dbLoss.TargetPercent;
                decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
                CumulativePercentage = CumulativePercentage + actualPercentage;
                worksheetGraph.Cells["K" + grphData].Value = lossCode;
                worksheetGraph.Cells["L" + grphData].Value = Target;
                worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
                worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
                grphData++;
            }

            //Code written on 05-10-2018
            int col = 12, col1 = 12, col2 = 13, col3 = 14;

            foreach (var GetRow in item)
            {
                string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();

                string columnNumber = ExcelColumnFromNumber(col);
                worksheetGraph.Cells[columnNumber + 36].Value = lossCode;

                int macLossNo = 38;

                DataTable dt = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string query = "SELECT TOP 3 SUM(tpol.LossDuration),tblma.MachineName,tbllo.LossCode,tpol.CorrectedDate from unitworksccs.unitworkccs.tbl_ProdOrderLosses tpol inner join unitworkccs.tbllossescodes tbllo on tbllo.LossCodeID = tpol.LossID inner join unitworkccs.tblmachinedetails tblma on tblma.MachineID = tpol.MachineID where tpol.CorrectedDate >= '" + fromDate.Date + "' and tpol.CorrectedDate <= '" + toDate.Date + "' AND tbllo.LossCode = '" + lossCode + "' " +
                    "group by tbllo.LossCode,tblma.MachineName,tpol.LossDuration, tpol.CorrectedDate order by tpol.LossDuration DESC";
                SqlDataAdapter sdt = new SqlDataAdapter(query, mc.msqlConnection);
                sdt.Fill(dt);
                mc.close();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    double value = Convert.ToDouble(dt.Rows[i][0]);
                    string macName = Convert.ToString(dt.Rows[i][1]);

                    decimal lossPercentage = (decimal)Math.Round(value / TotalDownTime, 2);
                    decimal lossDurationInHours = (decimal)Math.Round(value / 60.00, 2);

                    string colNum1 = ExcelColumnFromNumber(col1);
                    worksheetGraph.Cells[colNum1 + macLossNo].Value = macName;
                    string colNum2 = ExcelColumnFromNumber(col2);
                    worksheetGraph.Cells[colNum2 + macLossNo].Value = lossPercentage;
                    string colNum3 = ExcelColumnFromNumber(col3);
                    worksheetGraph.Cells[colNum3 + macLossNo].Value = lossDurationInHours;

                    macLossNo++;
                }

                col += 4; col1 += 4; col2 += 4; col3 += 4;
            }
            //Code ended on 05-10-2018

            //Code Written on 09-10-2018
            for (int i = 0; i < dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            using (SqlCommand cmd = new SqlCommand("InsertOEEReportDivision", mc.msqlConnection))
                            {
                                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                                {
                                    mc.open();
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@MachineID", Machine.MachineID);
                                    cmd.Parameters.AddWithValue("@CorrectedDate", QueryDate.Date);
                                    sda.Fill(dt);
                                    mc.close();
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            }

            var topContributingFactors = (from dbItem in Serverdb.tbl_oeereportasdivision
                                          where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
                                          group dbItem by new { dbItem.LossID, dbItem.FGCode } into x
                                          select new
                                          {
                                              x.Key.LossID,
                                              x.Key.FGCode,
                                              LossDuration = x.Select(m => m.LossDuration).Sum(),
                                          }).ToList();


            if (CellID != 0)
            {
                var getCellName = Serverdb.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault();
                // worksheetGraph.Cells["K46"].Value = getCellName;
            }


            var getValues = topContributingFactors.OrderByDescending(m => m.LossDuration).ThenBy(m => m.LossID).ToList();
            var distinctLoss = getValues.Select(m => m.LossID).Distinct().Take(10).ToList();
            int colNum = 48;
            for (int i = 0; i < distinctLoss.Count; i++)
            {
                int colVal1 = 12, colVal2 = 13;
                var getLossId = distinctLoss[i];
                string losscode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == getLossId).Select(m => m.LossCode).FirstOrDefault();
                worksheetGraph.Cells["K" + colNum].Value = losscode;

                var top3AccToLoss = getValues.Where(m => m.LossID == getLossId).OrderByDescending(m => m.LossDuration).Take(3).ToList();
                foreach (var data in top3AccToLoss)
                {
                    var FGcode = data.FGCode;
                    decimal LossDurationinHours = (decimal)Math.Round((data.LossDuration) / 60.00, 2);

                    string colName1 = ExcelColumnFromNumber(colVal1);
                    worksheetGraph.Cells[colName1 + colNum].Value = FGcode;
                    string colName2 = ExcelColumnFromNumber(colVal2);
                    worksheetGraph.Cells[colName2 + colNum].Value = LossDurationinHours;
                    colVal1 += 2; colVal2 += 2;
                }
                colNum++;
            }
            //Code written on 09-10-2018

            #region
            //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

            //chartIDAndUnID.SetSize((350), 550);

            //chartIDAndUnID.SetPosition(50, 60);

            //chartIDAndUnID.Title.Text = "AB Graph ";
            //chartIDAndUnID.Style = eChartStyle.Style18;
            //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
            ////chartIDAndUnID.Legend.Remove();
            //chartIDAndUnID.YAxis.MaxValue = 100;
            //chartIDAndUnID.YAxis.MinValue = 0;
            //chartIDAndUnID.YAxis.MajorUnit = 5;

            //chartIDAndUnID.Locked = false;
            //chartIDAndUnID.PlotArea.Border.Width = 0;
            //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
            //chartIDAndUnID.DataLabel.ShowValue = true;
            //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


            //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
            //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
            //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
            //hours.Header = "Operating Time (Hours)";
            ////Get reference to the worksheet xml for proper namespace
            //var chartXml = chartIDAndUnID.ChartXml;
            //var nsuri = chartXml.DocumentElement.NamespaceURI;
            //var nsm = new XmlNamespaceManager(chartXml.NameTable);
            //nsm.AddNamespace("c", nsuri);

            ////XY Scatter plots have 2 value axis and no category
            //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
            //if (valAxisNodes != null && valAxisNodes.Count > 0)
            //    foreach (XmlNode valAxisNode in valAxisNodes)
            //    {
            //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
            //        if (major != null)
            //            valAxisNode.RemoveChild(major);

            //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
            //        if (minor != null)
            //            valAxisNode.RemoveChild(minor);
            //    }

            ////Other charts can have a category axis
            //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
            //if (catAxisNodes != null && catAxisNodes.Count > 0)
            //    foreach (XmlNode catAxisNode in catAxisNodes)
            //    {
            //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
            //        if (major != null)
            //            catAxisNode.RemoveChild(major);

            //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
            //        if (minor != null)
            //            catAxisNode.RemoveChild(minor);
            //    }
            //worksheetGraph.View["L29"]
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            #endregion
            //worksheet.Column(29).Width = 12;
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "OEE_Report", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
            return View();
        }

        #region TVS
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    //ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();
        //    ReportsCalcClass.OEEReportCalculations OEC = new ReportsCalcClass.OEEReportCalculations();
        //    double AvailabilityPercentage = 0;
        //    double PerformancePercentage = 0;
        //    double QualityPercentage = 0;
        //    double OEEPercentage = 0;
        //    // OEC.GETCYCLETIMEAnalysis(MachineID, FromDate);


        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //    int StartRow = 2;
        //    //int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 11;
        //    String ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();

        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCode;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        string CorrectedDate = QueryDate.ToString("yyyy-MM-dd");
        //        foreach (var Machine in getMachineList)
        //        {
        //            OEC.OEE(MachineID, CorrectedDate);
        //            int MacStartcolumn = 11;
        //            var GetUtilList = Serverdb.tbl_OEEDetails.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == CorrectedDate).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                var partdet = Serverdb.tblparts.Where(m => m.MachineID == MacRow.MachineID).FirstOrDefault();
        //                worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                if (partdet != null)
        //                {
        //                    worksheet.Cells["B" + StartRow].Value = partdet.OperationNo;

        //                    worksheet.Cells["C" + StartRow].Value = partdet.FGCode;
        //                }
        //                worksheet.Cells["D" + StartRow].Value = CorrectedDate;
        //                worksheet.Cells["E" + StartRow].Value = MacRow.OperatingTimeinMin;
        //                worksheet.Cells["F" + StartRow].Value = MacRow.Availability;
        //                worksheet.Cells["G" + StartRow].Value = MacRow.Quality;
        //                if (MacRow.Performance > 100)
        //                    MacRow.Performance = 100;
        //                worksheet.Cells["H" + StartRow].Value = MacRow.Performance;
        //                worksheet.Cells["I" + StartRow].Value = MacRow.OEE;
        //                worksheet.Cells["J" + StartRow].Value = MacRow.TotalPartsCount;
        //                //worksheet.Cells["L" + StartRow].Value = "";
        //                // worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                //        int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                //        if (TotalQty == 0)
        //                //            TotalQty = 1;
        //                //        worksheet.Cells["K1"].Value = "Setup Time";
        //                //        worksheet.Cells["L1"].Value = "Rejections";
        //                //        worksheet.Cells["L" + StartRow].Value = (MacRow.OperatingTimeinMin / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                //long MacTotalLoss = 0;
        //                //foreach (var LossRow in GetMainLossList)
        //                //{
        //                //    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                //    if (getWoLossList1 != null)
        //                //    {
        //                //        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                //        MacTotalLoss += getWoLossList1.LossDuration;
        //                //    }
        //                //    else
        //                //        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                //    MacStartcolumn++;
        //                //}
        //                //        String ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                //        MacStartcolumn++;

        //                //        decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                //        MacStartcolumn++;
        //                StartRow++;
        //            }
        //        }
        //    }

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        string CorrectedDate = QueryDate.ToString("yyyy-MM-dd");
        //        var plantName = Serverdb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
        //        worksheetGraph.Cells["C3"].Value = plantName;
        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_OEEDetails.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == CorrectedDate).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                TotalOperatingTime += (double)ProdRow.OperatingTimeinMin;
        //                TotalDownTime += (double)ProdRow.TotalIDLETimeinMin;
        //                TotalAcceptedQty += Convert.ToInt32(ProdRow.TotalPartsCount);
        //                // TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                TotalPerformanceFactor += (double)ProdRow.PerformanceFactor;
        //                //Day Values
        //                DayOperatingTime += (double)ProdRow.OperatingTimeinMin;
        //                DayDownTime += (double)ProdRow.TotalIDLETimeinMin;
        //                DayAcceptedQty += Convert.ToInt32(ProdRow.TotalPartsCount);
        //                // DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                DayPerformanceFactor += (double)ProdRow.PerformanceFactor;
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCode;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.85;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = DayOEEPercent;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    if (TotalOperatingTime == 0)
        //        TotalOperatingTime = 1;
        //    if (TotalAcceptedQty == 0)
        //        TotalAcceptedQty = 1;
        //    decimal TotalAvailPercent = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //    decimal TotalPerformancePercent = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //    decimal TotalQualityPercent = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //    decimal TotalOEEPercent = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);

        //    if (TotalPerformancePercent > 100)
        //        TotalPerformancePercent = 100;
        //    worksheetGraph.Cells["E20"].Value = TotalAvailPercent;
        //    worksheetGraph.Cells["E21"].Value = TotalPerformancePercent;
        //    worksheetGraph.Cells["E22"].Value = TotalQualityPercent;
        //    worksheetGraph.Cells["E23"].Value = TotalOEEPercent;
        //    worksheetGraph.Cells["G5"].Value = TotalOEEPercent;
        //    worksheetGraph.View.ShowGridLines = false;

        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    decimal lossPercentage = 0;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();
        //        if (TotalDownTime != 0)
        //            lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;
        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCode;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;

        //    }

        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}
        #endregion

        #region OLDCode of OEEREPORT
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();
        //    ReportsCalcClass.OEEReportCalculations OEC = new ReportsCalcClass.OEEReportCalculations();
        //   double AvailabilityPercentage = 0;
        //    double PerformancePercentage = 0;
        //    double QualityPercentage = 0;
        //    double OEEPercentage = 0;
        //    OEC.GETCYCLETIMEAnalysis(MachineID, FromDate);
        //    OEC.OEE(MachineID, FromDate);

        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //    int StartRow = 2;
        //    int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 12;
        //    String ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCode;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        foreach (var Machine in getMachineList)
        //        {
        //            UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                int MacStartcolumn = 12;
        //                worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
        //                worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
        //                worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
        //                worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
        //                worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
        //                worksheet.Cells["H" + StartRow].Value = MacRow.TotalOperatingTime;
        //                worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
        //                worksheet.Cells["J" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
        //                worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                if (TotalQty == 0)
        //                    TotalQty = 1;
        //                worksheet.Cells["K1"].Value = "Setup Time";
        //                worksheet.Cells["L1"].Value = "Rejections";
        //                worksheet.Cells["L" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                long MacTotalLoss = 0;
        //                foreach (var LossRow in GetMainLossList)
        //                {
        //                    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                    if (getWoLossList1 != null)
        //                    {
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                        MacTotalLoss += getWoLossList1.LossDuration;
        //                    }
        //                    else
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                    MacStartcolumn++;
        //                }
        //                String ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                MacStartcolumn++;

        //                decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                MacStartcolumn++;
        //                StartRow++;
        //            }
        //        }
        //    }

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);

        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
        //                TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                TotalPerformanceFactor += ProdRow.PerfromaceFactor;
        //                //Day Values
        //                DayOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
        //                DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                DayPerformanceFactor += ProdRow.PerfromaceFactor;
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCode;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.85;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = DayOEEPercent;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    decimal TotalAvailPercent = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //    decimal TotalPerformancePercent = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //    decimal TotalQualityPercent = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //    decimal TotalOEEPercent = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);

        //    worksheetGraph.Cells["E20"].Value = TotalAvailPercent;
        //    worksheetGraph.Cells["E21"].Value = TotalPerformancePercent;
        //    worksheetGraph.Cells["E22"].Value = TotalQualityPercent;
        //    worksheetGraph.Cells["E23"].Value = TotalOEEPercent;
        //    worksheetGraph.Cells["G5"].Value = TotalOEEPercent;
        //    worksheetGraph.View.ShowGridLines = false;

        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();
        //        decimal lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;
        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCode;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;

        //    }

        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}
        #endregion

        public int getsundays(DateTime fdate, DateTime sdate)
        {
            //TimeSpan ts = fdate - sdate;
            //var sundays = ((ts.TotalDays / 7) + (sdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek > sdate.DayOfWeek ? 1 : 0));

            //sundays = Math.Round(sundays - .5, MidpointRounding.AwayFromZero);

            //return (int)sundays;
            int sundayCount = 0;

            for (DateTime dt = sdate; dt < fdate; dt = dt.AddDays(1.0))
            {
                if (dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundayCount++;
                }
            }

            return sundayCount;
        }

        public ActionResult CycleTime()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult CycleTime(string PlantID, string TimeType, DateTime FromDate, DateTime ToDate, string PartsList, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            #region old
            //if (report.Shift == "--Select Shift--")
            //{
            //    report.Shift = "No Use";
            //}
            //if (report.ShopNo == null)
            //{
            //    report.ShopNo = "No Use";
            //}
            //if (report.WorkCenter == null)
            //{
            //    report.WorkCenter = "No Use";
            //}
            #endregion
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            CycleTimeReportExcel(FromDate.ToString("yyyy-MM-dd"), ToDate.ToString("yyyy-MM-dd"), PartsList, PlantID.ToString(), Convert.ToString(ShopID), Convert.ToString(CellID), Convert.ToString(WorkCenterID));
            //UtilizationReportExcel(report.FromDate.ToString(), report.ToDate.ToString(), report.ShopNo.ToString(), report.WorkCenter.ToString(), TimeType);
            int p = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        // Part Learning  
        public void CycleTimeReportExcel(string StartDate, string EndDate, string PartsList, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\PartLearning.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];
            ExcelWorksheet workSheetGraphData = templatep.Workbook.Worksheets[3];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                workSheetGraphData = p.Workbook.Worksheets.Add("GraphData", workSheetGraphData);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
                workSheetGraphData = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "GraphData", workSheetGraphData);

            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    query1 = " SELECT  distinct MachineID FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Shop")
                {
                    query1 = " SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Cell")
                {
                    query1 = " SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    query1 = "SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0 and((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion
            List<int> MachineIdList = new List<int>();
            foreach (DataRow intItem in machin.Rows)
            {
                MachineIdList.Add(Convert.ToInt32(intItem["MachineID"].ToString()));
            }
            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            //For each Date ...... for all Machines.
            var Col = 'B';
            int Row = 5; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
            int Sno = 1;
            string finalLossCol = null;
            string existingPartNo = PartsList;

            //DataTable for Consolidated Data 


            string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
            PartSearchCreate obj = new PartSearchCreate();
            obj.StartTime = Convert.ToDateTime(Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd 07:00:00"));
            obj.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndDate).AddDays(1).ToString("yyyy-MM-dd 07:00:00"));
            obj.MachineId = MachineIdList;
            obj.FG_code = existingPartNo;
            obj.correctedDate = correctedDate;
            PushDataToTblPartLearingReport(obj);

            List<CycleTiemDataGraph> cycleTimeList = new List<CycleTiemDataGraph>();
            for (int i = 0; i < TotalDay + 1; i++)
            {
                int StartingRowForToday = Row;
                //string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");
                DateTime QueryDate = frmDate.AddDays(i);
                foreach (var macId in MachineIdList)
                {
                    //1) Get distinct partno,WoNo,Opno which are JF
                    //2) Get sum of green, settingTime, etc and push into excel
                    DataTable PartData = new DataTable();
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        mc.open();
                        //String query = "  select * from [unitworksccs].[unitworkccs].tblpartlearningreport where HMIID in  (SELECT HMIID FROM[unitworksccs].[unitworkccs].tblpartlearningreport where FGCode = '" + existingPartNo + "' and CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "'); ";

                        String query = "  select * from [unitworksccs].[unitworkccs].[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM [unitworksccs].[unitworkccs].[tblworkorderentry] where CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";

                        if (obj.FG_code != null && obj.FG_code != "")
                        {
                            query = "  select * from [unitworksccs].[unitworkccs].[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM [unitworksccs].[unitworkccs].[tblworkorderentry] where FGCode = '" + existingPartNo + "' and CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";
                        }

                        SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                        da.Fill(PartData);
                        mc.close();
                    }
                    for (int j = 0; j < PartData.Rows.Count; j++)
                    {
                        int MachineID = Convert.ToInt32(PartData.Rows[j][1]); //MachineID
                        List<string> HierarchyData = GetHierarchyData(MachineID);

                        worksheet.Cells["B" + Row].Value = Sno++;
                        //worksheet.Cells["C" + Row].Value = HierarchyData[0];//Plant
                        //worksheet.Cells["D" + Row].Value = HierarchyData[1];//Shop
                        //worksheet.Cells["E" + Row].Value = HierarchyData[2];//Cell
                        worksheet.Cells["C" + Row].Value = HierarchyData[3];//Mac Display Name
                        string WorkOrderNo = Convert.ToString(PartData.Rows[j][4]);//WorkOrderNo
                        worksheet.Cells["D" + Row].Value = Convert.ToDateTime(Convert.ToString(PartData.Rows[j][3])).ToString("dd-MM-yyyy");//completed Date
                        worksheet.Cells["E" + Row].Value = WorkOrderNo;
                        worksheet.Cells["F" + Row].Value = PartData.Rows[j][5];//FG Code
                        string OpNo = Convert.ToString(PartData.Rows[j][6]);//OpNo
                        worksheet.Cells["G" + Row].Value = OpNo;
                        string TargetQty = Convert.ToString(PartData.Rows[j][7]);//TargetQty
                        int TargetQtyCalc = Convert.ToInt32(PartData.Rows[j][9]) + Convert.ToInt32(PartData.Rows[j][10]);//Yield Qty
                        if (TargetQtyCalc == 0)
                        {
                            TargetQtyCalc = 1;
                        }
                        worksheet.Cells["H" + Row].Value = TargetQty;
                        worksheet.Cells["I" + Row].Value = Convert.ToString(PartData.Rows[j][9]);//Yield Qty
                        worksheet.Cells["J" + Row].Value = Convert.ToInt32(PartData.Rows[j][10]); //Scrap Qty
                        double StdCycTime = Convert.ToDouble(PartData.Rows[j][18]);
                        double StdMinorLoss = Convert.ToDouble(PartData.Rows[j][21]);
                        worksheet.Cells["K" + Row].Value = StdCycTime;//Std Cycle Time
                        worksheet.Cells["L" + Row].Value = StdMinorLoss; //Std Minor Loss
                        worksheet.Cells["M" + Row].Value = StdCycTime + StdMinorLoss; //Total Std Time

                        //worksheet.Cells["N" + Row].Value = Convert.ToInt32(PartData.Rows[j][22]); //Total Std Minor Loss
                        //worksheet.Cells["N" + Row].Value = Convert.ToInt32(PartData.Rows[j][11]); //Setting Time
                        //worksheet.Cells["O" + Row].Value = Convert.ToInt32(PartData.Rows[j][12]);//Idle

                        //worksheet.Cells["Q" + Row].Value = Convert.ToInt32(PartData.Rows[j][14]); //Blue
                        int HMIID = Convert.ToInt32(PartData.Rows[j][2]);//Hmmid
                        DataTable dt1 = new DataTable();
                        using (MsqlConnection mc = new MsqlConnection())
                        {
                            mc.open();
                            String qry = "SELECT WOStart,WOEnd FROM [unitworksccs].[unitworkccs].[tblworkorderentry] where HMIID = '" + HMIID + "'";
                            SqlDataAdapter da = new SqlDataAdapter(qry, mc.msqlConnection);
                            da.Fill(dt1);
                            mc.close();
                        }
                        int tbCount = dt1.Rows.Count;
                        int ActualCuttingTime = 0;
                        if (tbCount > 0)
                        {
                            string startDate = (dt1.Rows[0][0]).ToString();
                            string endDate = (dt1.Rows[0][1]).ToString();

                            DataTable dt2 = new DataTable();
                            using (MsqlConnection mc = new MsqlConnection())
                            {
                                mc.open();
                                String qry = "SELECT SUM(DATEDiff(MINUTE,StartTime,EndTime)) as diff FROM[unitworksccs].[unitworkccs].[tblmode] where MachineID = " + MachineID + "  and StartTime>= '" + startDate + "' and EndTime<= '" + endDate + "' and MacMode = 'PROD'";
                                SqlDataAdapter da = new SqlDataAdapter(qry, mc.msqlConnection);
                                da.Fill(dt2);
                                mc.close();
                            }
                            try
                            {
                                ActualCuttingTime = Convert.ToInt32(dt2.Rows[0][0]);
                            }
                            catch
                            {
                                ActualCuttingTime = 0;
                            }
                        }
                        worksheet.Cells["N" + Row].Value = ActualCuttingTime;
                        worksheet.Cells["O" + Row].Value = Convert.ToInt32(PartData.Rows[j][13]);//Minor Loss
                        worksheet.Cells["P" + Row].Value = ActualCuttingTime + Convert.ToInt32(PartData.Rows[j][13]);//Actual Total Operating Time
                        worksheet.Cells["Q" + Row].Value = Convert.ToInt32(PartData.Rows[j][17]);//Average Cuttng Time
                        worksheet.Cells["R" + Row].Value = Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc;//Average Minor Loss
                        worksheet.Cells["S" + Row].Value = Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc);//Average Total Operating Time
                        worksheet.Cells["T" + Row].Value = StdCycTime + StdMinorLoss - (Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc));//Cycle Time Delta
                        int CyCtimeDelta = (int)(StdCycTime + StdMinorLoss - (Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)));
                        setcellcolor(worksheet, CyCtimeDelta, "T" + Row.ToString());
                        worksheet.Cells["U" + Row].Value = Math.Round(((Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)) / ((StdCycTime + StdMinorLoss))) * 100, 0) - 100;//Cycle Time Delta %
                        double CycDel = Math.Round(((Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)) / ((StdCycTime + StdMinorLoss))) * 100, 0) - 100;
                        //settextcolor(worksheet, CycDel, "U" + Row.ToString());

                        string modelRange = "B" + Row + ":U" + Row + "";
                        var modelTable = worksheet.Cells[modelRange];
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        CycleTiemDataGraph itemCycleTime = new CycleTiemDataGraph();
                        string fgcodOpno = PartData.Rows[j][5] + "-" + OpNo;
                        itemCycleTime.fgcodOpno = fgcodOpno;
                        itemCycleTime.YieldQty = Convert.ToInt32(PartData.Rows[j][9]);
                        itemCycleTime.ScrapQty = Convert.ToInt32(PartData.Rows[j][10]);
                        itemCycleTime.TotalStdTime = StdCycTime + StdMinorLoss;
                        itemCycleTime.ActualTotalOperatingTime = ActualCuttingTime + Convert.ToInt32(PartData.Rows[j][13]);
                        cycleTimeList.Add(itemCycleTime);
                        Row++;
                    }
                }

                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            }


            #region//graph data
            int RowGraph = 5;

            int intalColumn = 2;
            var iListItem = cycleTimeList.OrderBy(m => m.fgcodOpno);
            var uniqFGCodeOpNo = cycleTimeList.Select(m => m.fgcodOpno).Distinct();

            foreach (var fgItem in uniqFGCodeOpNo)
            {
                int ActRow = 1;
                decimal diff = 0;
                string fgCodeOverAll = "";
                int totalYelidAndScrapQty = 0, TotalActualTotalOperatingTime = 0;
                double TotalTotalStdTime = 0;
                foreach (var item in iListItem)
                {
                    if (fgItem == item.fgcodOpno)
                    {
                        fgCodeOverAll = item.fgcodOpno;
                        totalYelidAndScrapQty = totalYelidAndScrapQty + item.YieldQty + item.ScrapQty;
                        TotalTotalStdTime = item.TotalStdTime;
                        TotalActualTotalOperatingTime = TotalActualTotalOperatingTime + item.ActualTotalOperatingTime;

                    }

                }
                if (totalYelidAndScrapQty != 0)
                    diff = (Convert.ToDecimal(TotalActualTotalOperatingTime) / Convert.ToDecimal(totalYelidAndScrapQty));
                // string dcdff = diff.ToString("0.##");
                int dfrnc = Convert.ToInt32(Math.Round(diff));
                workSheetGraphData.Cells["A" + RowGraph].Value = fgCodeOverAll;//FG Code
                workSheetGraphData.Cells["B" + RowGraph].Value = totalYelidAndScrapQty;//Yield Qty+Scrap Qty
                workSheetGraphData.Cells["C" + RowGraph].Value = TotalTotalStdTime; //Total Std Time
                workSheetGraphData.Cells["D" + RowGraph].Value = TotalActualTotalOperatingTime;//Actual Total Operating Time
                workSheetGraphData.Cells["E" + RowGraph].Value = Convert.ToDecimal(dfrnc);//Actual Total Op Time = Cum of Actual Total Operating Time/Cum(yeild qty+scrap qty)


                var coluName = ExcelColumnFromNumber(intalColumn);
                workSheetGraphData.Cells[coluName + ActRow].Value = fgCodeOverAll;//FG Code
                ActRow++;
                workSheetGraphData.Cells[coluName + ActRow].Value = TotalTotalStdTime; //Total Std Time
                ActRow++;
                workSheetGraphData.Cells[coluName + ActRow].Value = Convert.ToDecimal(dfrnc);//Actual Total Op Time = Cum of Actual Total Operating Time/Cum(yeild qty+scrap qty)

                RowGraph++;
                intalColumn++;
            }

            for (int i = intalColumn; i <= 104; i++)
            {
                workSheetGraphData.Column(i).Hidden = true;
            }

            workSheetGraphData.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;

            #endregion

            #region Save and Download

            //Hide Values
            //Color ColorHexWhite = System.Drawing.Color.White;
            //worksheetGraph.Cells["A1:Z50"].Style.Font.Color.SetColor(ColorHexWhite);

            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            //worksheetGraph.View.ShowGridLines = false;
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            if (file1.Exists)
            {
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                Response.AddHeader("Content-Length", file1.Length.ToString());
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.WriteFile(file1.FullName);
                Response.Flush();
                Response.Close();
            }
            #endregion
        }

        [HttpGet]
        public ActionResult PMSReport()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult PMSReport(int plantid, int shopid, int cellid, int machineid, int Year, int radiobtn = 0, int radiobtn1 = 0, int radiobtn2 = 0)
        {
            string startMonth = null;
            string endMonth = null;
            int nextyear = Year + 1;
            if (radiobtn == 1)
            {
                startMonth = ("01-03-" + Year);
                endMonth = ("01-04-" + nextyear);
            }
            else if (radiobtn1 == 2)
            {
                startMonth = ("01-06-" + Year);
                endMonth = ("01-07-" + nextyear);
            }
            else if (radiobtn2 == 3)
            {
                startMonth = ("01-12-" + Year);
                endMonth = ("01-01-" + nextyear);
            }
            DateTime ToDate = DateTime.Now;
            FileInfo templateFile = new FileInfo(@"C:\I_ShopFloorReports\MainTemplate\PMS_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\I_ShopFloorReports\ReportsList\" + ToDate.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx"));
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 5;
            int SlNo = 1;
            int i = 0;
            DataTable dt = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                var pmsdet = "Select pmsid from i_facility_configuration.tblpmsdetails where MachineID =" + machineid + " and PMStartDate >= '" + startMonth + "' and PMEndDate >= '" + endMonth + "';";
                SqlDataAdapter sda = new SqlDataAdapter(pmsdet, mc.msqlConnection);
                sda.Fill(dt);
                mc.close();
            }
            int count1 = dt.Rows.Count;
            if (count1 == 1)
            {
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).ToList();
                    foreach (var row in pmsdata)
                    {
                        string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                        string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                        worksheet.Cells["C2"].Value = cellname;
                        worksheet.Cells["F2"].Value = machinename;
                        var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).ToList();
                        foreach (var item in pmcpid)
                        {
                            SlNo = 1;
                            var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                            worksheet.Cells["A" + StartRow].Value = "";
                            worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                            StartRow++;
                            var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                            int count = checklistdata.Count;
                            foreach (var row1 in checklistdata)
                            {
                                worksheet.Cells["A" + StartRow].Value = SlNo++;
                                worksheet.Cells["B" + StartRow].Value = row1.CheckList;
                                worksheet.Cells["C" + StartRow].Value = row1.Frequency;
                                worksheet.Cells["D" + StartRow].Value = row1.How;
                                worksheet.Cells["E" + StartRow].Value = row1.Value;
                                worksheet.Cells["G3"].Value = row.CorrectedDate;
                                if (row.workdone == 1)
                                {
                                    worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                    worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["F" + StartRow].Value = "YES";
                                }
                                else
                                {
                                    worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                    worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["F" + StartRow].Value = "NO";
                                }
                                StartRow++;
                            }
                        }
                        StartRow++;
                    }
                    StartRow++;
                }
            }
            else if (count1 == 2)
            {
                int cou = 0;
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).ToList();

                    foreach (var row in pmsdata)
                    {
                        if (cou != 1)
                        {
                            string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                            string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                            worksheet.Cells["C2"].Value = cellname;
                            worksheet.Cells["F2"].Value = machinename;
                            var pmcpid1 = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).ToList();
                            foreach (var item in pmcpid1)
                            {
                                SlNo = 1;
                                var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                                worksheet.Cells["A" + StartRow].Value = "";
                                worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                                StartRow++;
                                var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                                foreach (var row1 in checklistdata)
                                {
                                    worksheet.Cells["A" + StartRow].Value = SlNo++;
                                    worksheet.Cells["B" + StartRow].Value = row1.CheckList;
                                    worksheet.Cells["C" + StartRow].Value = row1.Frequency;
                                    worksheet.Cells["D" + StartRow].Value = row1.How;
                                    worksheet.Cells["E" + StartRow].Value = row1.Value;
                                    worksheet.Cells["G3"].Value = row.CorrectedDate;
                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata.pmcpID && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work.workdone == 1)
                                    {
                                        worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                        worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["F" + StartRow].Value = "YES";
                                    }
                                    else
                                    {
                                        worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                        worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["F" + StartRow].Value = "NO";
                                    }
                                    cou++;
                                    StartRow++;
                                }
                                StartRow++;
                            }
                            StartRow++;
                            break;
                        }
                        int startrow = 6;
                        worksheet.Cells["I3"].Value = "Date:";
                        worksheet.Cells["J3"].Value = row.CorrectedDate;
                        worksheet.Cells["I4"].Value = "Work Done";
                        var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).ToList();
                        foreach (var item in pmcpid)
                        {
                            var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                            var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                            foreach (var row1 in checklistdata1)
                            {
                                var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                if (work.workdone == 1)
                                {
                                    worksheet.Cells["I" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["I" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                    worksheet.Cells["I" + startrow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["I" + startrow].Value = "YES";
                                }
                                else
                                {
                                    worksheet.Cells["I" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["I" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                    worksheet.Cells["I" + startrow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["I" + startrow].Value = "NO";
                                }
                                startrow++;

                            }
                        }
                        startrow++;
                        break;
                    }
                }
            }
            else if (count1 == 3)
            {
                int count2 = 0;
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).ToList();

                    foreach (var row in pmsdata)
                    {
                        if (count2 == 0)
                        {
                            string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                            string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                            worksheet.Cells["C2"].Value = cellname;
                            worksheet.Cells["F2"].Value = machinename;
                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                SlNo = 1;
                                var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                                worksheet.Cells["A" + StartRow].Value = "";
                                worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                                StartRow++;
                                var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                                foreach (var row1 in checklistdata)
                                {
                                    var histrecord = Serverdb.tblhistpms.Where(m => m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate && m.Pmcheckpointid == checkpointdata.pmcpID).FirstOrDefault();
                                    if (histrecord != null)
                                    {
                                        worksheet.Cells["A" + StartRow].Value = SlNo++;
                                        worksheet.Cells["B" + StartRow].Value = row1.CheckList;
                                        worksheet.Cells["C" + StartRow].Value = row1.Frequency;
                                        worksheet.Cells["D" + StartRow].Value = row1.How;
                                        worksheet.Cells["E" + StartRow].Value = row1.Value;
                                        worksheet.Cells["G3"].Value = row.CorrectedDate;
                                        var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata.pmcpID && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                        if (work != null)
                                            if (work.workdone == 1)
                                            {
                                                worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                                worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                                worksheet.Cells["F" + StartRow].Value = "YES";
                                            }
                                            else
                                            {
                                                worksheet.Cells["F" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                worksheet.Cells["F" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                                worksheet.Cells["F" + StartRow].Style.Font.Color.SetColor(Color.White);
                                                worksheet.Cells["F" + StartRow].Value = "NO";
                                            }
                                    }
                                    StartRow++;
                                }
                                StartRow++;
                            }
                            StartRow++;
                            count2++;
                            break;
                        }
                        else if (count2 == 1)
                        {
                            int startrow = 6;
                            worksheet.Cells["I3"].Value = "Date:";
                            worksheet.Cells["J3"].Value = row.CorrectedDate;
                            worksheet.Cells["I4"].Value = "Work Done";
                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                                var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                                foreach (var row1 in checklistdata1)
                                {
                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work != null)
                                        if (work.workdone == 1)
                                        {
                                            worksheet.Cells["I" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["I" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                            worksheet.Cells["I" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["I" + startrow].Value = "YES";
                                        }
                                        else
                                        {
                                            worksheet.Cells["I" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["I" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                            worksheet.Cells["I" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["I" + startrow].Value = "NO";
                                        }
                                    startrow++;
                                }
                            }
                            startrow++;
                            count2++;
                            break;
                        }
                        else if (count2 == 2)
                        {
                            int startrow = 6;
                            worksheet.Cells["L3"].Value = "Date:";
                            worksheet.Cells["M3"].Value = row.CorrectedDate;
                            worksheet.Cells["L4"].Value = "Work Done";
                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                                var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                                foreach (var row1 in checklistdata1)
                                {
                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work != null)
                                        if (work.workdone == 1)
                                        {
                                            worksheet.Cells["L" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["L" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                            worksheet.Cells["L" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["L" + startrow].Value = "YES";
                                        }
                                        else
                                        {
                                            worksheet.Cells["L" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["L" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                            worksheet.Cells["L" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["L" + startrow].Value = "NO";
                                        }
                                    count2++;
                                    startrow++;
                                }
                            }
                            startrow++;
                            break;
                        }
                    }
                    //StartRow++;
                    //SlNo = 1;
                }

            }
            p.Save();

            string path1 = System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx";
            if (file1.Exists)
            {
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                Response.AddHeader("Content-Length", file1.Length.ToString());
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.WriteFile(file1.FullName);
                Response.Flush();
                Response.Close();
            }
            using (unitworksccsEntities1 Serverdb = new unitworksccsEntities1())
            {


                ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", plantid);
                ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", shopid);
                ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", cellid);
                ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", machineid);
            }
            return View();
        }

        public void PushDataToTblPartLearingReport(PartSearchCreate obj)
        {
            //(obj.FG_code != null || obj.FG_code != "") && 
            if ((obj.StartTime != null) && (obj.EndTime != null) && (obj.MachineId != null))
            {
                foreach (var macId in obj.MachineId)
                {
                    var getWorkOrderIds = Serverdb.tblworkorderentries.Where(m => m.MachineID == macId && m.IsFinished == 1).Where(m => m.WOStart >= obj.StartTime && m.WOEnd <= obj.EndTime).ToList();

                    //String query = "  select * from [unitworksccs].[unitworkccs].[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM [unitworksccs].[unitworkccs].[tblworkorderentry] where CorrectedDate >= '" + obj.StartTime.ToString("yyyy-MM-dd") + "'  and CorrectedDate <= '" + obj.EndTime.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";

                    if (obj.FG_code != null && obj.FG_code != "")
                    {
                        getWorkOrderIds = Serverdb.tblworkorderentries.Where(m => m.MachineID == macId && m.FGCode == obj.FG_code && m.IsFinished == 1).Where(m => m.WOStart >= obj.StartTime && m.WOEnd <= obj.EndTime).ToList();

                        //query = "  select * from [unitworksccs].[unitworkccs].[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM [unitworksccs].[unitworkccs].[tblworkorderentry] where FGCode = '" + obj.FG_code + "' and CorrectedDate >= '" + obj.StartTime.ToString("yyyy-MM-dd") + "'  and CorrectedDate <= '" + obj.EndTime.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";
                    }
                    int count = getWorkOrderIds.Count();
                    if (count > 0)
                    {
                        //DataTable PartData = new DataTable();
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                        //    da.Fill(PartData);
                        //    mc.close();
                        //}
                        //int countPartData = PartData.Rows.Count;
                        //if (countPartData == 0)
                        {
                            foreach (var item in getWorkOrderIds)
                            {
                                var GetDataPre = Serverdb.tblpartlearningreports.Where(m => m.HMIID == item.HMIID).ToList();
                                if (GetDataPre.Count == 0)
                                {
                                    int OperatingTime = 0;
                                    int LossTime = 0;
                                    int MinorLossTime = 0;
                                    int MntTime = 0;
                                    int SetupTime = 0;
                                    int SetupMinorTime = 0;
                                    int PowerOffTime = 0;
                                    long idle = 0;
                                    decimal loadAndUnload = 0;
                                    int rejections = 0;
                                    DateTime ProdStartTime = item.WOStart;
                                    DateTime ProdEndtime = DateTime.Now;
                                    try
                                    {
                                        if (item.WOEnd.HasValue)
                                        {
                                            ProdEndtime = Convert.ToDateTime(item.WOEnd);
                                        }
                                    }
                                    catch { }

                                    #region Logic to get the Mode Durations between a Production Order which are completed
                                    var GetModeDurations = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                                    foreach (var ModeRow in GetModeDurations)
                                    {
                                        if (ModeRow.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                                        {
                                            LossTime += (int)(ModeRow.DurationInSec / 60);
                                            int LossDuration = (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                                        {
                                            MinorLossTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "MNT")
                                        {
                                            MntTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "SETUP")
                                        {
                                            try
                                            {
                                                SetupTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.DurationInSec).First() / 60);
                                                SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                            }
                                            catch { }
                                        }
                                    }
                                    #endregion

                                    #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                                    var GetEndModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                                    if (GetEndModeDuration != null)
                                    {
                                        if (GetEndModeDuration.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetEndModeDuration.ModeType == "IDLE")
                                        {
                                            LossTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                            int LossDuration = (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                        }
                                        else if (GetEndModeDuration.ModeType == "MNT")
                                        {
                                            MntTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetEndModeDuration.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                    }
                                    #endregion

                                    #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                                    var GetStartModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                                    if (GetStartModeDuration != null)
                                    {
                                        if (GetStartModeDuration.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetStartModeDuration.ModeType == "IDLE")
                                        {
                                            LossTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                            int LossDuration = (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                        }
                                        else if (GetStartModeDuration.ModeType == "MNT")
                                        {
                                            MntTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetStartModeDuration.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                    }
                                    #endregion

                                    int TotlaQty = item.Total_Qty;
                                    if (TotlaQty == 0)
                                        TotlaQty = 1;
                                    int GetOptime = OperatingTime;
                                    if (GetOptime == 0)
                                    {
                                        GetOptime = 1;
                                    }
                                    decimal IdealCycleTimeVal = 0;
                                    decimal UtilPercent = 0;
                                    var IdealCycTime = Serverdb.tblparts.Where(m => m.FGCode == item.FGCode && m.OperationNo == item.OperationNo).FirstOrDefault();
                                    if (IdealCycTime != null)
                                        IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
                                    double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                                    if (TotalTime != 0)
                                        UtilPercent = (decimal)Math.Round(OperatingTime / TotalTime * 100, 2);
                                    decimal Quality = (decimal)Math.Round((double)item.Yield_Qty / TotlaQty * 100, 2);
                                    decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)item.Total_Qty / GetOptime * 100, 2);
                                    int PerformanceFactor = (int)IdealCycleTimeVal * item.Total_Qty;
                                    //tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                                    //PRA.MachineID = macId;
                                    //PRA.WOID = item.HMIID;
                                    ////PRA.CorrectedDate = CorrectedDate.Date;
                                    //PRA.TotalLoss = LossTime;
                                    //PRA.TotalOperatingTime = OperatingTime;
                                    //PRA.TotalSetup = SetupTime + SetupMinorTime;
                                    //PRA.TotalMinorLoss = MinorLossTime - SetupMinorTime;
                                    //PRA.TotalSetupMinorLoss = SetupMinorTime;
                                    //PRA.TotalPowerLoss = PowerOffTime;
                                    //PRA.UtilPercent = UtilPercent;
                                    //PRA.QualityPercent = Quality;
                                    //PRA.PerformancePerCent = Performance;
                                    //PRA.PerfromaceFactor = PerformanceFactor;
                                    //PRA.InsertedOn = DateTime.Now;
                                    loadAndUnload = MinorLossTime;
                                    int TotalQty = item.Yield_Qty + item.ScrapQty;
                                    if (TotalQty == 0)
                                        TotalQty = 1;
                                    rejections = Convert.ToInt32((OperatingTime / TotalQty) * item.ScrapQty);

                                    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
                                    foreach (var LossRow in GetMainLossList)
                                    {
                                        var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == item.HMIID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                                        if (getWoLossList1 == null)
                                        {
                                            idle = idle + 0;
                                        }
                                        else
                                        {
                                            idle = idle + getWoLossList1.LossDuration;
                                        }
                                        if (LossRow.LossCode == "LOAD / UNLOAD")
                                        {
                                            if (getWoLossList1 == null)
                                            {
                                                loadAndUnload = loadAndUnload + 0;
                                            }
                                            else
                                            {
                                                loadAndUnload = loadAndUnload + getWoLossList1.LossDuration;
                                            }
                                        }
                                    }
                                    var dbParts = Serverdb.tblparts.Where(m => m.FGCode == item.FGCode && m.OperationNo == item.OperationNo).FirstOrDefault();
                                    decimal idealctime = 0;
                                    decimal? stdmloss = 0;
                                    if (dbParts != null)
                                    {
                                        idealctime = dbParts.IdealCycleTime;
                                        if (dbParts.StdMinorLoss != null)
                                        {
                                            //  stdmloss = (decimal)dbParts.StdMinorLoss;
                                        }
                                        else
                                        {
                                            stdmloss = 0;
                                        }
                                    }
                                    tblpartlearningreport partLearning = new tblpartlearningreport();
                                    partLearning.MachineID = macId;
                                    partLearning.HMIID = item.HMIID;
                                    partLearning.CorrectedDate = item.CorrectedDate.ToString("yyyy-MM-dd");
                                    partLearning.WorkOrderNo = item.Prod_Order_No;
                                    partLearning.FGCode = item.FGCode;
                                    partLearning.OpNo = item.OperationNo;
                                    partLearning.TargetQty = item.ProdOrderQty;
                                    partLearning.TotalQty = item.Total_Qty;
                                    partLearning.YieldQty = item.Yield_Qty;
                                    partLearning.ScrapQty = item.ScrapQty;
                                    partLearning.SettingTime = SetupTime + SetupMinorTime;
                                    partLearning.Idle = idle;
                                    partLearning.MinorLoss = loadAndUnload;
                                    partLearning.PowerOff = PowerOffTime;
                                    partLearning.TotalNCCuttingTime = OperatingTime;
                                    try
                                    {
                                        partLearning.AvgCuttingTime = OperatingTime / item.Total_Qty;
                                    }
                                    catch
                                    {
                                        partLearning.AvgCuttingTime = 0;
                                    }

                                    partLearning.StdCycleTime = idealctime;
                                    partLearning.TotalStdCycleTime = idealctime * item.Total_Qty;
                                    partLearning.StdMinorLoss = stdmloss;
                                    partLearning.TotalStdMinorLoss = stdmloss * item.Total_Qty;
                                    partLearning.InsertedOn = DateTime.Now;
                                    partLearning.StartTime = obj.StartTime;
                                    partLearning.EndTime = obj.EndTime;
                                    Serverdb.tblpartlearningreports.Add(partLearning);
                                    Serverdb.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

        }

        public void setcellcolor(ExcelWorksheet ws, int value, String cell)
        {
            try
            {
                ws.Cells[cell].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                if (value < 0)
                {
                    ws.Cells[cell].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                }
                else if (value >= 0)
                {
                    ws.Cells[cell].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
            }
            catch { }
        }

        public void settextcolor(ExcelWorksheet ws, double value, String cell)
        {
            try
            {
                ws.Cells[cell].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                if (value > 0)
                {
                    ws.Cells[cell].Style.Font.Color.SetColor(Color.DarkRed);
                }
                else if (value <= 0)
                {
                    ws.Cells[cell].Style.Font.Color.SetColor(Color.Green);
                }
            }
            catch { }
        }

        List<string> GetHierarchyData(int MachineID)
        {
            List<string> HierarchyData = new List<string>();
            //1st get PlantName or -
            //2nd get ShopName or -
            //3rd get CellName or -
            //4th get MachineName.

            using (unitworksccsEntities1 dbMac = new unitworksccsEntities1())
            {
                var machineData = dbMac.tblmachinedetails.Where(m => m.MachineID == MachineID).FirstOrDefault();
                int PlantID = Convert.ToInt32(machineData.PlantID);
                string name = "-";
                name = dbMac.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
                HierarchyData.Add(name);

                string ShopIDString = Convert.ToString(machineData.ShopID);
                int value;
                if (int.TryParse(ShopIDString, out value))
                {
                    name = dbMac.tblshops.Where(m => m.ShopID == value).Select(m => m.ShopName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }

                string CellIDString = Convert.ToString(machineData.CellID);
                if (int.TryParse(CellIDString, out value))
                {
                    name = dbMac.tblcells.Where(m => m.CellID == value).Select(m => m.CellName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }
                // HierarchyData.Add(Convert.ToString(machineData.MachineName));
                HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
            }
            return HierarchyData;
        }

        //code to remove major GridLines
        public void RemoveGridLines(ref ExcelChart chartName)
        {
            var chartXml = chartName.ChartXml;
            var nsuri = chartXml.DocumentElement.NamespaceURI;
            var nsm = new XmlNamespaceManager(chartXml.NameTable);
            nsm.AddNamespace("c", nsuri);

            //XY Scatter plots have 2 value axis and no category
            var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
            if (valAxisNodes != null && valAxisNodes.Count > 0)
                foreach (XmlNode valAxisNode in valAxisNodes)
                {
                    var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                    if (major != null)
                        valAxisNode.RemoveChild(major);

                    var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                    if (minor != null)
                        valAxisNode.RemoveChild(minor);
                }

            //Other charts can have a category axis
            var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
            if (catAxisNodes != null && catAxisNodes.Count > 0)
                foreach (XmlNode catAxisNode in catAxisNodes)
                {
                    var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                    if (major != null)
                        catAxisNode.RemoveChild(major);

                    var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                    if (minor != null)
                        catAxisNode.RemoveChild(minor);
                }
        }

        public ActionResult ToolLife()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult ToolLife(string PlantID, string ShopID, string CellID, string WorkCenterID, string OpNo, DateTime FromDate, DateTime ToDate, string ProdNo = null, string CTCode = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            String RetStatus = "";
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            ToolLifeReportExcel(FromDate.ToString("yyyy-MM-dd"), ToDate.ToString("yyyy-MM-dd"), PlantID.ToString(), Convert.ToString(ShopID), Convert.ToString(CellID), Convert.ToString(WorkCenterID), CTCode, OpNo, ProdNo, CTCode);
            int p = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            TempData["ToolLifeStatus"] = RetStatus;
            return View();
        }

        public void ToolLifeReportExcel(string StartDate, string EndDate, string PlantID, string ShopID, string CellID, string WorkCenterID, string PartsList, string opNo, string ProdNo = null, string CTCode = null)
        {
            string RetStatus = "";

            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\ToolLifeMonitoringSheet.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    RetStatus = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            //ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    query1 = " SELECT  distinct MachineID FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Shop")
                {
                    query1 = " SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Cell")
                {
                    query1 = " SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    query1 = "SELECT * FROM  [unitworksccs].[unitworkccs].tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0 and((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion
            List<int> MachineIdList = new List<int>();
            foreach (DataRow intItem in machin.Rows)
            {
                MachineIdList.Add(Convert.ToInt32(intItem["MachineID"].ToString()));
            }
            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            int Row = 9; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).

            var FGCodeDet = Serverdb.tblworkorderentries.Where(m => m.FGCode.Trim() == ProdNo && m.OperationNo == opNo).FirstOrDefault();
            string drawingNo = Serverdb.tblparts.Where(m => m.FGCode == ProdNo.Trim() && m.OperationNo == opNo).Select(m => m.DrawingNo).FirstOrDefault();
            int? macId = Convert.ToInt32(WorkCenterID);
            string macName = Serverdb.tblmachinedetails.Where(m => m.MachineID == macId).Select(m => m.MachineDisplayName).FirstOrDefault();
            int? stdToolLife = Serverdb.tblStdToolLives.Where(m => m.FGCode == ProdNo.Trim() && m.OperationNo == opNo && m.CTCode == CTCode).Select(m => m.StdToolLife).FirstOrDefault();
            worksheet.Cells["C4"].Value = CTCode;
            worksheet.Cells["C6"].Value = ProdNo;
            worksheet.Cells["H4"].Value = stdToolLife;
            worksheet.Cells["L4"].Value = opNo;
            worksheet.Cells["L6"].Value = macName;


            string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");

            for (int i = 0; i < TotalDay + 1; i++)
            {
                DateTime QueryDate = frmDate.AddDays(i);
                DataTable toolData = new DataTable();
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query = "SELECT wrk.*,tblop.HMIID,tblop.IsReset,tblop.ResetReason,tblop.toollifecounter FROM[unitworksccs].[unitworkccs].[tbltoollifeoperator] tblop " +
                        "left outer join[unitworksccs].[unitworkccs].[tblworkorderentry] wrk on  tblop.HMIID=wrk.HMIID where wrk.FGCode='" + ProdNo +
                        "' and wrk.OperationNo='" + opNo + "' and wrk.MachineID= " + WorkCenterID + " and tblop.ToolCTCode = '" + CTCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                    da.Fill(toolData);
                    mc.close();
                }

                int CumulativeValue = 0;
                for (int j = 0; j < toolData.Rows.Count; j++)
                {
                    int MachineID = Convert.ToInt32(toolData.Rows[j][1]); //MachineID

                    string CorrectedDate = Convert.ToString(toolData.Rows[j][14]);//CorrectedDate
                    DateTime CorrectedDate1 = Convert.ToDateTime(CorrectedDate);
                    correctedDate = CorrectedDate1.Date.ToString("dd-MM-yyyy");
                    string shift = Convert.ToString(toolData.Rows[j][5]);//shift

                    int isreset = Convert.ToInt32(toolData.Rows[j][29]);

                    string ResetReason = Convert.ToString(toolData.Rows[j][31]);//ResetReason
                    CumulativeValue += Convert.ToInt32(toolData.Rows[j][32]);
                    worksheet.Cells["B" + Row].Value = QueryDate;
                    worksheet.Cells["B" + Row].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells["C" + Row].Value = toolData.Rows[j][7].ToString();
                    worksheet.Cells["D" + Row].Value = shift;
                    worksheet.Cells["E" + Row].Value = Convert.ToInt32(toolData.Rows[j][32]);
                    worksheet.Cells["H" + Row].Value = CumulativeValue;
                    if (isreset == 0)
                    {
                        worksheet.Cells["K" + Row].Value = "NA";
                    }
                    else
                    {
                        worksheet.Cells["K" + Row].Value = ResetReason;
                    }

                    //string modelRange = "B" + Row + ":M" + Row + "";
                    worksheet.Cells["E" + Row + ":G" + Row + ""].Merge = true;
                    worksheet.Cells["H" + Row + ":J" + Row + ""].Merge = true;
                    worksheet.Cells["K" + Row + ":N" + Row + ""].Merge = true;
                    //var modelTable = worksheet.Cells[modelRange];
                    //modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Row++;
                }
            }

            #region Save and Download

            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            if (file1.Exists)
            {
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                Response.AddHeader("Content-Length", file1.Length.ToString());
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.WriteFile(file1.FullName);
                Response.Flush();
                Response.Close();
            }

            #endregion
        }
    }

    public class PartSearchCreate
    {
        public List<int> MachineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string FG_code { get; set; }
        public string correctedDate { get; set; }
    }

    public class CycleTiemDataGraph
    {
        public string fgcodOpno { get; set; }
        public int YieldQty { get; set; }
        public int ScrapQty { get; set; }
        public double TotalStdTime { get; set; }
        public int ActualTotalOperatingTime { get; set; }
    }
}