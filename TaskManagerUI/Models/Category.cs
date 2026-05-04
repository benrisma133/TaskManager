namespace TaskManagerUI.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsCustom { get; set; } = false;
    }
}
