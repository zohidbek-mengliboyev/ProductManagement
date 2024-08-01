namespace ProductManagement.Entity
{
    public class ProductAudit
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ChangeType { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangeDate { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

}
