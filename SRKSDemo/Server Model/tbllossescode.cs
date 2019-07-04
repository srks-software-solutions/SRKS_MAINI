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
    
    public partial class tbllossescode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbllossescode()
        {
            this.tblbreakdowns = new HashSet<tblbreakdown>();
            this.tblemailescalations = new HashSet<tblemailescalation>();
            this.tblemailescalations1 = new HashSet<tblemailescalation>();
            this.tblemailescalations2 = new HashSet<tblemailescalation>();
            this.tblescalationlogs = new HashSet<tblescalationlog>();
            this.tblmodetemps = new HashSet<tblmodetemp>();
            this.tblSetupMaints = new HashSet<tblSetupMaint>();
            this.tbllossofentries = new HashSet<tbllossofentry>();
            this.tbllivemodes = new HashSet<tbllivemode>();
            this.tblmodes = new HashSet<tblmode>();
            this.tblmodes1 = new HashSet<tblmode>();
        }
    
        public int LossCodeID { get; set; }
        public string LossCode { get; set; }
        public string LossCodeDesc { get; set; }
        public string MessageType { get; set; }
        public int LossCodesLevel { get; set; }
        public Nullable<int> LossCodesLevel1ID { get; set; }
        public Nullable<int> LossCodesLevel2ID { get; set; }
        public string ContributeTo { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> EndCode { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public int ServerTabCheck { get; set; }
        public int ServerTabFlagSync { get; set; }
        public decimal TargetPercent { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblbreakdown> tblbreakdowns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblemailescalation> tblemailescalations2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblescalationlog> tblescalationlogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmodetemp> tblmodetemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblSetupMaint> tblSetupMaints { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllossofentry> tbllossofentries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbllivemode> tbllivemodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmode> tblmodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmode> tblmodes1 { get; set; }
    }
}
