﻿namespace FinalProject.Models
{
    public class Slide : BaseNameableEntity
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string Image { get; set; }
        public int Order { get; set; }
    }
}
