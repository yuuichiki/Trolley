namespace RCabinet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public partial class ShaContext : DbContext
    {
        public ShaContext() : base("name= SHAConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<HUData> HUDatas { get; set; }
        public virtual DbSet<HUDataDetail> HUDataDetails { get; set; }
        public virtual DbSet<HUDataEPC> HUDataEPCs { get; set; }
        public virtual DbSet<CardUQModel> TrolleyUQCards { get; set; }
        public virtual DbSet<TrolleyEPCMapping> TrolleyEPCMappings { get; set; }


        [Table("HUData")]
        public partial class HUData
        {
            [Key]
            public Guid Id { get; set; }
            public string BOXNO { get; set; }
            public string CUSTOMER { get; set; }
            public string HUNO { get; set; }
            public string PO { get; set; }
            public string SO { get; set; }
            public string STYLE { get; set; }
            public string CUSTOMERSTYLE { get; set; }
            public string CHECKPOINT { get; set; }
            public string USERNAME { get; set; }
            public string PCNAME { get; set; }
            public DateTime CREATEDATE { get; set; }

        }


        [Table("HUDataDetail")]
        public partial class HUDataDetail
        {
            [Key]
            public Guid Id { get; set; }
            public Guid? HUID { get; set; }
            public string SIZE { get; set; }
            public string COLOR { get; set; }
            public int? QTY { get; set; }
            public int? ACT_QTY { get; set; }
            public DateTime? CREATEDATE { get; set; }
        }


        [Table("HUDataEPC")]
        public partial class HUDataEPC
        {
            [Key]
            public Guid Id { get; set; }
            public Guid? HUID { get; set; }
            public string SIZE { get; set; }
            public string COLOR { get; set; }
            public string HUNO { get; set; }
            public string EPC { get; set; }
            public string SKU { get; set; }
            public DateTime? CREATEDATE { get; set; }
        }


    }



}



