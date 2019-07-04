using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class LoginController : Controller
    {
        unitworksccsEntities1 condb = new unitworksccsEntities1();
        // GET: Login
        [HttpGet]
        public ActionResult Login(int IPAddress = 0)
        {
            return View();
        }

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int roleid = Convert.ToInt32(Session["RoleID"]);
            ViewBag.PrimaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc");
            ViewBag.SecondaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc");
            ViewBag.MachineID = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDisplayName");
            UserModel ua = new UserModel();
            tbluser us = new tbluser();
            ua.Users = us;
            ua.UsersList = condb.tblusers.Where(m => m.IsDeleted == 0).ToList();
            return View(ua);

            //var tbllogin = db.masteruserlogindet_tbl.Where(m => m.IsDeleted == 0 && m.masterroledet_tbl.RoleName != "SuperAdmin").ToList();
            //return View(tbllogin);
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int roleid = Convert.ToInt32(Session["RoleID"]);
            String Username = Session["Username"].ToString();

            using (unitworksccsEntities1 condb = new unitworksccsEntities1())
            {
                ViewBag.PrimaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc");
                ViewBag.SecondaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc");
                ViewBag.MachineID = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDispName");
                return View();
            }
        }


        public string InsertData(string UserName, string Password, string DisplayName, int RoleID1)
        {
            string res = "";
            using (unitworksccsEntities1 condb = new unitworksccsEntities1())
            {
                tbluser tblpc = new tbluser();
                tblpc.CreatedBy = 1;
                tblpc.CreatedOn = DateTime.Now;
                tblpc.IsDeleted = 0;
                tblpc.UserName = UserName;
                tblpc.Password = Password;
                tblpc.DisplayName = DisplayName;
                tblpc.PrimaryRole = RoleID1;
                condb.tblusers.Add(tblpc);
                condb.SaveChanges();
                res = "Success";
                return res;
            }
        }

        public string InsertDat(string UserName, string Password, string DisplayName, int RoleID1, int RoleID2)
        {
            string res = "";
            using (unitworksccsEntities1 condb = new unitworksccsEntities1())
            {
                tbluser tblpc = new tbluser();
                tblpc.CreatedBy = 1;
                tblpc.CreatedOn = DateTime.Now;
                tblpc.IsDeleted = 0;
                tblpc.UserName = UserName;
                tblpc.Password = Password;
                tblpc.DisplayName = DisplayName;
                tblpc.PrimaryRole = RoleID1;
                tblpc.SecondaryRole = RoleID2;
                condb.tblusers.Add(tblpc);
                condb.SaveChanges();
                res = "Success";
                return res;
            }
        }

        public string UpdateData(string UserName, string Password, string DisplayName, int RoleID1, int userid)
        {
            string res = "";
            var doesThisExist = condb.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == UserName && m.Password == Password && m.DisplayName == DisplayName && m.UserID != userid && m.PrimaryRole == RoleID1).ToList();
            if (doesThisExist.Count == 0)
            {
                using (unitworksccsEntities1 condb = new unitworksccsEntities1())
                {
                    var UserData = condb.tblusers.Find(userid);

                    UserData.UserName = UserName;
                    UserData.Password = Password;
                    UserData.DisplayName = DisplayName;
                    UserData.PrimaryRole = RoleID1;
                    UserData.ModifiedBy = userid;
                    UserData.ModifiedOn = DateTime.Now;
                    condb.Entry(UserData).State = EntityState.Modified;
                    condb.SaveChanges();
                    res = "Success";

                }
            }
            return res;
        }

        public string UpdateDat(string UserName, string Password, string DisplayName, int RoleID1, int RoleID2, int userid)
        {
            string res = "";
            var doesThisExist = condb.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == UserName && m.Password == Password && m.DisplayName == DisplayName && m.UserID != userid && m.PrimaryRole == RoleID1 && m.SecondaryRole == RoleID2).ToList();
            if (doesThisExist.Count == 0)
            {
                using (unitworksccsEntities1 condb = new unitworksccsEntities1())
                {
                    var UserData = condb.tblusers.Find(userid);

                    UserData.UserName = UserName;
                    UserData.Password = Password;
                    UserData.DisplayName = DisplayName;
                    UserData.PrimaryRole = RoleID1;
                    UserData.ModifiedBy = userid;
                    UserData.SecondaryRole = RoleID2;
                    UserData.ModifiedOn = DateTime.Now;
                    condb.Entry(UserData).State = EntityState.Modified;
                    condb.SaveChanges();
                    res = "Success";

                }
            }
            return res;
        }

        //[HttpPost]
        //public ActionResult Create(UserModel user, int PrimaryRoleID, int SecondaryRoleID)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }

        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];
        //    int roleid = Convert.ToInt32(Session["RoleID"]);
        //    String Username = Session["Username"].ToString();

        //    //Update user data with other required fields.
        //    user.Users.PrimaryRole = PrimaryRoleID;
        //    user.Users.SecondaryRole = SecondaryRoleID;
        //    user.Users.CreatedBy = roleid;
        //    user.Users.CreatedOn = System.DateTime.Now;
        //    user.Users.IsDeleted = 0;
        //    var dupUserData = condb.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == user.Users.UserName).ToList();
        //    if (dupUserData.Count == 0)
        //    {
        //        condb.tblusers.Add(user.Users);
        //        condb.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    else // Duplicate UserName Exists so show error message.
        //    {
        //        Session["Error"] = "Duplicate UserName : " + user.Users.UserName;
        //        ViewBag.PrimaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "RoleID", "RoleDesc", user.Users.PrimaryRole);
        //        ViewBag.SecondaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "RoleID", "RoleDesc", user.Users.SecondaryRole);
        //        return View(user);
        //    }
        //}

        //[HttpPost]
        //public ActionResult Edit(UserModel user, int PrimaryRoleID, int SecondaryRoleID)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];
        //    String Username = Session["Username"].ToString();
        //    int UserID = Convert.ToInt32(Session["UserID"]);
        //    int roleid = Convert.ToInt32(Session["RoleID"]);

        //    //user.ModifiedBy = UserID;
        //    //user.ModifiedOn = System.DateTime.Now;

        //    var dupUserData = condb.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == user.Users.UserName && m.UserID != user.Users.UserID).ToList();
        //    if (dupUserData.Count == 0)
        //    {
        //        var UserData = condb.tblusers.Find(user.Users.UserID);

        //        UserData.UserName = user.Users.UserName;
        //        UserData.Password = user.Users.Password;
        //        UserData.DisplayName = user.Users.DisplayName;
        //        UserData.PrimaryRole = PrimaryRoleID;
        //        UserData.SecondaryRole = SecondaryRoleID;
        //        UserData.ModifiedBy = UserID;
        //        UserData.ModifiedOn = DateTime.Now;

        //        int primaryrole = Convert.ToInt32(user.Users.PrimaryRole);

        //        condb.Entry(UserData).State = EntityState.Modified;
        //        condb.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        Session["Error"] = "Duplicate User Name : " + user.Users.UserName;
        //        ViewBag.PrimaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc", user.Users.PrimaryRole);
        //        ViewBag.SecondaryRoleID = new SelectList(condb.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid).ToList(), "Role_ID", "RoleDesc", user.Users.SecondaryRole);
        //        return View(user);
        //    }
        //}

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            //ViewBag.IsConfigMenu = 0;
            tbluser tblusers = condb.tblusers.Find(id);
            tblusers.IsDeleted = 1;
            tblusers.ModifiedBy = UserID;
            tblusers.ModifiedOn = System.DateTime.Now;
            condb.Entry(tblusers).State = EntityState.Modified;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Deleted successfully";
            return RedirectToAction("Index");
        }


        public JsonResult GetUserById(int Id)
        {
            var Data = condb.tblusers.Where(m => m.UserID == Id).Select(m => new { username = m.UserName, password = m.Password, displayname = m.DisplayName, primaryrole = m.PrimaryRole, secondary = m.SecondaryRole });

            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Fetchroles(int Primaryroleid)
        {

            var CatData = (from row in condb.tblroles
                           where row.IsDeleted == 0 && row.Role_ID == Primaryroleid
                           select new { Value = row.Role_ID, Text = row.RoleDesc });
            foreach (var row in CatData)
            {

            }
            return Json(CatData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(tbluser userlogin)
        {
            using (unitworksccsEntities1 condb = new unitworksccsEntities1())
            {
                if (userlogin.UserName != null && userlogin.Password != null)
                {
                    var usercnt = condb.tblusers.Where(m => m.UserName == userlogin.UserName && m.Password == userlogin.Password && m.IsDeleted == 0).Count();
                    if (usercnt == 0) //There is no row with corresponding username and password.
                    {
                        //TempData["username"] = "Please enter a valid User Name & Password";
                        //return View(userlogin);

                        var OploginCount = condb.tblOperatorLoginDetails.Where(m => m.operatorUserName == userlogin.UserName && m.operatorPwd == userlogin.Password && m.isDeleted == 0).Count();
                        if (OploginCount != 0)
                        {
                            var OperatorDet = condb.tblOperatorLoginDetails.Where(m => m.operatorUserName == userlogin.UserName && m.operatorPwd == userlogin.Password && m.isDeleted == 0).Select(m => new { m.operatorLoginId, m.operatorUserName, m.NumOfMachines, m.roleId,m.operatorId,m.operatorName }).FirstOrDefault();
                            Session["OUserID"] = OperatorDet.operatorLoginId;
                            Session["OUsername"] = OperatorDet.operatorName;
                            Session["ORoleID"] = OperatorDet.roleId;
                            Session["OFullName"] = OperatorDet.operatorUserName;
                            Session["OMachineNo"] = OperatorDet.NumOfMachines;
                            Session["LoginTime"] = DateTime.Now;
                            Session["OperatorID"] = OperatorDet.operatorId;
                            int OperatorId = Convert.ToInt32(Session["UserID"]);

                            ViewBag.date = System.DateTime.Now;
                            ViewBag.Logout = Session["OUsername"].ToString().ToUpper();
                            ViewBag.roleid = OperatorDet.roleId;
                            if (OperatorDet.roleId == 6)
                            {
                                Response.Redirect("~/OperatorEntryModel/Index", false);
                            }
                            else
                            {
                                TempData["username"] = "No Login Found.";
                            }
                        }
                    }
                    else if (usercnt != 0) // username and password matches so get user details and redirect to respective Views.
                    {
                        var log = condb.tblusers.Where(m => m.UserName == userlogin.UserName && m.Password == userlogin.Password && m.IsDeleted == 0).Select(m => new { m.UserID, m.PrimaryRole, m.UserName, m.MachineID }).Single();
                        if (log != null)
                        {
                            Session["UserID"] = log.UserID;
                            Session["Username"] = log.UserName;
                            Session["RoleID"] = log.PrimaryRole;
                            Session["FullName"] = log.UserName;
                            Session["MachineID"] = log.MachineID;
                            int OperatorId = Convert.ToInt32(Session["UserID"]);

                            ViewBag.date = System.DateTime.Now;
                            ViewBag.Logout = Session["Username"].ToString().ToUpper();
                            ViewBag.roleid = log.PrimaryRole;
                            if (log.PrimaryRole == 1 || log.PrimaryRole == 2)
                            {
                                Response.Redirect("~/Dashboard/Dashboard", false);
                            }
                            else if (log.PrimaryRole == 3 || log.PrimaryRole == 6)
                            {

                                //Response.Redirect("~/OperatorEntry/DashboardProduction", false);

                                Response.Redirect("~/OperatorEntry/EntryWindow?id=" + log.MachineID + "", false);
                            }
                        }
                        TempData["username"] = "UserName or Password cannot be Empty.";
                    }                    
                }
                return View(userlogin);
            }
        }

        //Used to kill session by Calling Session.Abondon()
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login", "Login");
        }
    }
}