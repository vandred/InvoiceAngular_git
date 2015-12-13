namespace InvoiceAngular.Models
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;

    public class InvoiceDbContext : DbContext
    {
        // Your context has been configured to use a 'InvoiceDbContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'InvoiceAngular.Models.my.InvoiceDbContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'InvoiceDbContext' 
        // connection string in the application configuration file.
        public InvoiceDbContext()
            : base("name=InvoiceDbContext")
        {




        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Accuracy>()
                .HasKey(t => t.OID);

            modelBuilder.Entity<Invoice>()
                .HasRequired(t => t.Accuracy)
                .WithRequiredPrincipal(t => t.Invoice)
                .WillCascadeOnDelete(true);
        }
        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<Accuracy> Accuracy { get; set; }
    }
  


    [Table("InvoiceTable")]
    public class Invoice
    {
        [Key]
        public int OID { get; set; }
        //[unique + Vendor DUNS/ID]
        public int? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? CustomerDunsID { get; set; }
        public string CustomerName { get; set; }
        //[unique + Invoice Number]
        public int? VendorDUNSID { get; set; }
        public string VendorName { get; set; }
        public double? TotalAmount { get; set; }
        public string InvoiceFileFullPath { get; set; }
        public int? InvoceNumberScore { get; set; }
        public int? InvoiceDateScore { get; set; }
        public int? CustomerDunsIDScore { get; set; }
        public int? CustomerNameScore { get; set; }
        public int? VendoreDunsIDScore { get; set; }
        public int? VendorNameScore { get; set; }
        public decimal? TotalAmountScore { get; set; }
        public int? WeightedAverageScore { get; set; }
        public virtual Accuracy Accuracy { get; set; }

    }

    [Table("AccuracyTable")]
    public class Accuracy
    {
        [Key]
        [ForeignKey("Invoice")]
        public int OID { get; set; }
        public int? Key { get; set; }
        public int? AccuracyRate { get; set; }
        public int? ScoreWeight { get; set; }
        public virtual Invoice Invoice { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    public class InvoiceViewModel 
    {
        public int OID { get; set; }
        //[unique + Vendor DUNS/ID]
        public int? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? CustomerDunsID { get; set; }
        public string CustomerName { get; set; }
        //[unique + Invoice Number]
        public int? VendorDUNSID { get; set; }
        public string VendorName { get; set; }
        public double? TotalAmount { get; set; }
        public string InvoiceFileFullPath { get; set; }
        public int? InvoceNumberScore { get; set; }
        public int? InvoiceDateScore { get; set; }
        public int? CustomerDunsIDScore { get; set; }
        public int? CustomerNameScore { get; set; }
        public int? VendoreDunsIDScore { get; set; }
        public int? VendorNameScore { get; set; }
        public decimal? TotalAmountScore { get; set; }
        public int? WeightedAverageScore { get; set; }
        /// <summary>
        /// Accuracy
        /// </summary>
        public int? AccuracyRate { get; set; }
        public int? ScoreWeight { get; set; }
    
    }
}