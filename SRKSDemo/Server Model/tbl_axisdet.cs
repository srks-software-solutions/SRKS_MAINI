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
    
    public partial class tbl_axisdet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_axisdet()
        {
            this.configurationtblsensordatalinks = new HashSet<configurationtblsensordatalink>();
        }
    
        public int AxisDetID { get; set; }
        public int MachineID { get; set; }
        public int AxisID { get; set; }
        public string AxisName { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int IsDeleted { get; set; }
        public int SpindlePath { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<configurationtblsensordatalink> configurationtblsensordatalinks { get; set; }
        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
