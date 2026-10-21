using Mailingg.Models;
using System;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPromotional { get; set; }
    public DateTime EndDate { get; set; }
    public int SectionId { get; set; }
    public MailingSection? Section { get; set; }
}