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
        public virtual DbSet<ZebraConfig> ZebraConfigs { get; set; }

        public virtual DbSet<TrolleyNikeCard> TrolleyNikeCards { get; set; }
        public virtual DbSet<TrolleyNikeEPCMapping> TrolleyNikeEPCMappings { get; set; }

        [Table("ZebraConfig")]
        public partial class ZebraConfig
        {
            [Key]
            public Guid Id { get; set; }
            public string DeviceId { get; set; }
            public string AntenaIP { get; set; }
            public int Rssi { get; set; }
            public int CycleTime { get; set;}
            public string DeviceType { get; set; }
            public string Position { get; set; }
            public DateTime? UpdateTime { get; set; }
        }


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


        [Table("Trolley_NikeCard", Schema = "dbo")]
        public class TrolleyNikeCard
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid MappingId { get; set; }

            [MaxLength(255)]
            public string StyleNo { get; set; }

            [MaxLength(255)]
            public string CustomerColor { get; set; }

            public int? ValidQuantity { get; set; }

            [MaxLength(255)]
            public string Mo { get; set; }

            [MaxLength(255)]
            public string ColorNo { get; set; }

            [MaxLength(255)]
            public string ColorName { get; set; }

            [MaxLength(50)]
            public string Size { get; set; }

            public int? Quantity { get; set; }

            [MaxLength(255)]
            public string CardNo { get; set; }

            public bool? IsActive { get; set; }

            [MaxLength(255)]
            public string WorklayerNo { get; set; }

            [MaxLength(255)]
            public string WorklayerName { get; set; }

            [MaxLength(255)]
            public string Group { get; set; }

            [MaxLength(255)]
            public string GangHao { get; set; }

            public int? CutType { get; set; }

            [MaxLength(255)]
            public string CutTypeName { get; set; }

            [Required]
            public DateTime DateCreated { get; set; }

            public string Department { get; set; }
            public string LineNo { get; set; }

        }



        [Table("Trolley_NikeEPCMapping", Schema = "dbo")]
        public class TrolleyNikeEPCMapping
        {
            [Key]
            public Guid Id { get; set; }

            public int? Count { get; set; }

            public Guid? NikeCardId { get; set; }

            [Required]
            public Guid MappingId { get; set; }

            [MaxLength(100)]
            public string EmpCode { get; set; }

            [MaxLength(100)]
            public string EPC { get; set; }

            [MaxLength(100)]
            public string EncodeEPC { get; set; }

            [MaxLength(100)]
            public string Size { get; set; }

            [MaxLength(100)]
            public string Color { get; set; }

            [MaxLength(250)]
            public string GangHao { get; set; }

            public DateTime? TimeCreated { get; set; }
        }


    }



}



