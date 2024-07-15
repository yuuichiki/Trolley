namespace RCabinet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public partial class ETSContext : DbContext
    {
        public ETSContext() : base("name= SHAConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<HUData> HUDatas { get; set; }
    


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


    }



}



