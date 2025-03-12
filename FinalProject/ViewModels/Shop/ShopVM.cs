namespace FinalProject.ViewModels
{
    public class ShopVM
    {
        public ICollection<GetProductVM> Products { get; set; }

        public ICollection<GetCategoryVM> Categories { get; set; }
        public ICollection<GetBrandVM> Brands { get; set; }

        public ICollection<GetColorVM> Colors { get; set; }
        public ICollection<GetTagVM> Tags { get; set; }


        public string Search { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }

        public int? ColorId { get; set; }
        public List<int>? ColorIds { get; set; }

        public int? TagId { get; set; }
        public List<int>? TagIds { get; set; }




        public int Key { get; set; }
        public double TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalProducts { get; set; }

        public int PageSize { get; set; } = 15;
        public int TotalItems { get; set; }
    }
}
