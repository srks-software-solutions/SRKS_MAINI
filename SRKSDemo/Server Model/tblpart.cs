//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SRKSDemo.Server_Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblpart
    {
        public int PartID { get; set; }
        public string FGCode { get; set; }
        public string OperationNo { get; set; }
        public string PartName { get; set; }
        public decimal IdealCycleTime { get; set; }
        public Nullable<int> PartsPerCycle { get; set; }
        public int UnitDesc { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string DrawingNo { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public Nullable<decimal> Std_Load_UnloadTime { get; set; }
        public Nullable<decimal> Std_SetupTime { get; set; }
        public Nullable<int> MachineID { get; set; }
        public string StdMinorLoss { get; set; }
        public Nullable<decimal> StdLoadingTime { get; set; }
        public Nullable<decimal> StdUnLoadingTime { get; set; }
    
        public virtual tblpart tblparts1 { get; set; }
        public virtual tblpart tblpart1 { get; set; }
    }
}
